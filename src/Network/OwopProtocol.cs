using System.Buffers;
using System.Drawing;
using CommunityToolkit.HighPerformance;
using Owop.Game;
using Owop.Util;

namespace Owop.Network;

/// <summary>Contains serialization methods in the OWOP format.</summary>
public static class OwopProtocol
{
    /// <summary>Tries to read a generic <see cref="Position"/>.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="pos">The decoded position.</param>
    /// <returns>Whether the position was read properly.</returns>
    public static bool TryReadPos(ref this SequenceReader<byte> reader, out Position pos)
    {
        if (reader.TryReadLittleEndian(out int x) &&
            reader.TryReadLittleEndian(out int y))
        {
            pos = (x, y);
            return true;
        }
        pos = Position.Origin;
        return false;
    }

    /// <summary>Tries to read a <see cref="Color"/>.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="color">The decoded color.</param>
    /// <returns>Whether the color was read properly.</returns>
    public static bool TryReadColor(ref this SequenceReader<byte> reader, out Color color)
    {
        if (reader.TryRead(out byte r) &&
            reader.TryRead(out byte g) &&
            reader.TryRead(out byte b))
        {
            color = Color.FromArgb(0, r, g, b);
            return true;
        }
        color = Color.Empty;
        return false;
    }

    /// <summary>Tries to read some <see cref="PlayerData"/>.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="hasTool">Whether to also read the player's selected tool.</param>
    /// <param name="player">The decoded player data.</param>
    /// <returns>Whether the player data was read properly.</returns>
    public static bool TryReadPlayer(ref this SequenceReader<byte> reader, bool hasTool, out PlayerData player)
    {
        player = new();
        if (!reader.TryReadLittleEndian(out int id) ||
            !reader.TryReadPos(out Position pos) ||
            !reader.TryReadColor(out Color color))
        {
            return false;
        }
        player.Id = id;
        player.Pos = pos;
        player.Color = color;
        if (!hasTool)
        {
            return true;
        }
        if (reader.TryRead(out byte toolId))
        {
            player.Tool = (PlayerTool)toolId;
            return true;
        }
        return false;
    }

    /// <summary>Tries to read a <see cref="Bucket"/>.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="bucket">The decoded bucket.</param>
    /// <returns>Whether the bucket was read properly.</returns>
    public static bool TryReadBucket(ref this SequenceReader<byte> reader, out Bucket bucket)
    {
        if (reader.TryReadLittleEndian(out short capacity) &&
            reader.TryReadLittleEndian(out short fillTime))
        {
            bucket = new(capacity, fillTime, false);
            return true;
        }
        bucket = Bucket.Empty;
        return false;
    }

    /// <summary>Tries to read metadata at the beginning of a chunk sequence.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="isProtected">Whether the chunk is protected.</param>
    /// <returns>Whether the chunk metadata was read properly.</returns>
    public static bool TryReadChunkMeta(ref this SequenceReader<byte> reader, out Position chunkPos, out bool isProtected)
    {
        if (!reader.TryReadPos(out chunkPos) ||
            !reader.TryRead(out byte protectedByte))
        {
            isProtected = false;
            return false;
        }
        isProtected = protectedByte == 1;
        return true;
    }

    /// <summary>Tries to read a number within a chunk sequence.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="number">The decoded number.</param>
    /// <returns>Whether the number was read properly.</returns>
    public static bool TryReadChunkNumber(ref this SequenceReader<byte> reader, out int number)
    {
        if (!reader.TryReadExact(2, out var meta))
        {
            number = 0;
            return false;
        }
        byte[] header = meta.ToArray();
        number = header[1] << 8 | header[0];
        return true;
    }

    /// <summary>Tries to read a <see cref="Chunk"/>.</summary>
    /// <param name="reader">The byte reader.</param>
    /// <param name="world">The world the chunk is being read from.</param>
    /// <param name="chunk">The decoded chunk, if any.</param>
    /// <returns>Whether the chunk was read properly.</returns>
    public static bool TryReadChunk(ref this SequenceReader<byte> reader, World world, out Chunk? chunk)
    {
        chunk = null;
        if (!reader.TryReadChunkMeta(out var chunkPos, out bool isProtected) ||
            !reader.TryReadChunkNumber(out int length) ||
            !reader.TryReadChunkNumber(out int segments))
        {
            return false;
        }
        MemoryStream stream = new(length); // length should always be 768
        BinaryWriter writer = new(stream);
        chunk = world._chunks.GetOrCreate(chunkPos);
        chunk.IsProtected = isProtected;
        int repeatOffset = (2 * segments) + 14; // 17, but 1 indexed, then subtract 2 for no reason?
        int[] repeatLocations = new int[segments];
        for (int i = 0; i < segments; i++)
        {
            if (reader.TryReadChunkNumber(out int location))
            {
                repeatLocations[i] = location + repeatOffset;
            }
        }
        for (int i = 0; i < segments; i++)
        {
            int diff = repeatLocations[i] - reader.Position.GetInteger();
            if (diff > 0 && reader.TryReadExact(diff, out var fillData))
            {
                writer.Write(fillData.ToArray());
            }
            if (!reader.TryReadChunkNumber(out int times) ||
                !reader.TryReadExact(3, out var colorData))
            {
                return false;
            }
            byte[] colorArr = colorData.ToArray();
            for (int t = 0; t < times; t++)
            {
                writer.Write(colorArr);
            }
        }
        if (reader.Remaining > 0 && reader.TryReadExact((int)reader.Remaining, out var remaining))
        {
            writer.Write(remaining.ToArray());
        }
        byte[] data = stream.ToArray();
        // Write to span directly to preserve references
        for (int i = 0; i < data.Length; i += 3)
        {
            var color = Color.FromArgb(data[i], data[i + 1], data[i + 2]);
            int index = i / 3;
            chunk._memory.Span[index / IChunk.Width, index % IChunk.Width] = color;
        }
        return true;
    }

    /// <summary>Encodes a generic <see cref="Position"/>.</summary>
    /// <param name="pos">The position.</param>
    /// <returns>The position byte array.</returns>
    public static byte[] EncodePos(Position pos)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(pos.X);
        writer.Write(pos.Y);
        return stream.ToArray();
    }

    /// <summary>Encodes a <see cref="Color"/>.</summary>
    /// <param name="color">The color.</param>
    /// <returns>The color byte array.</returns>
    public static byte[] EncodeColor(Color color) => [color.R, color.G, color.B];

    /// <summary>Encodes a pixel's position and color.</summary>
    /// <param name="pos">The pixel position.</param>
    /// <param name="color">The pixel color.</param>
    /// <returns>The pixel byte array.</returns>
    public static byte[] EncodePixel(Position pos, Color color) => [.. EncodePos(pos), .. EncodeColor(color)];

    /// <summary>Encodes a <see cref="Player"/>'s data.</summary>
    /// <param name="player">The player.</param>
    /// <returns>The player data byte array.</returns>
    public static byte[] EncodePlayer(Player player) => [.. EncodePixel(player.Pos, player.Color), (byte)player.Tool];

    /// <summary>Encodes a chunk protection message.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="protect">Whether to protect the chunk.</param>
    /// <returns>The chunk protection message.</returns>
    public static byte[] EncodeChunkProtect(Position chunkPos, bool protect) => [.. EncodePos(chunkPos), (byte)(protect ? 1 : 0), 0];

    /// <summary>Encodes a chunk fill message.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="color">The color to fill the chunk with.</param>
    /// <returns>The chunk fill message.</returns>
    public static byte[] EncodeChunkFill(Position chunkPos, Color color) => [.. EncodePixel(chunkPos, color), 0, 0];

    /// <summary>Encodes complete outbound chunk data.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="data">A pixel color byte array.</param>
    /// <returns>The chunk data byte array.</returns>
    public static byte[] EncodeChunkData(Position chunkPos, byte[] data)
    {
        var pos = EncodePos(chunkPos);
        MemoryStream stream = new(new byte[IChunk.DataSize + pos.Length]);
        BinaryWriter writer = new(stream);
        writer.Write(pos);
        if (data.Length > IChunk.DataSize)
        {
            data = data[0..IChunk.DataSize];
        }
        writer.Write(data);
        return stream.ToArray();
    }
}
