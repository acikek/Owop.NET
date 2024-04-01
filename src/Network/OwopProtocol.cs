using System.Buffers;
using System.Drawing;
using CommunityToolkit.HighPerformance;
using Owop.Game;
using Owop.Util;

namespace Owop.Network;

public static class OwopProtocol
{
    public static bool TryReadPos(ref this SequenceReader<byte> reader, out Position point)
    {
        if (reader.TryReadLittleEndian(out int x) &&
            reader.TryReadLittleEndian(out int y))
        {
            point = (x, y);
            return true;
        }
        point = Position.Origin;
        return false;
    }

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

    public static bool TryReadChunkAmount(ref this SequenceReader<byte> reader, out int amount)
    {
        if (!reader.TryReadExact(2, out var meta))
        {
            amount = 0;
            return false;
        }
        byte[] header = meta.ToArray();
        amount = header[1] << 8 | header[0];
        return true;
    }

    public static bool TryReadChunk(ref this SequenceReader<byte> reader, World world, out Chunk? chunk)
    {
        chunk = null;
        if (!reader.TryReadChunkMeta(out var chunkPos, out bool isProtected) ||
            !reader.TryReadChunkAmount(out int length) ||
            !reader.TryReadChunkAmount(out int segments))
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
            if (reader.TryReadChunkAmount(out int location))
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
            if (!reader.TryReadChunkAmount(out int times) ||
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

    public static byte[] EncodePos(Position pos)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(pos.X);
        writer.Write(pos.Y);
        return stream.ToArray();
    }

    public static byte[] EncodeColor(Color color) => [color.R, color.G, color.B];

    public static byte[] EncodePixel(Position pos, Color color) => [.. EncodePos(pos), .. EncodeColor(color)];

    public static byte[] EncodePlayer(Player data) => [.. EncodePixel(data.Pos, data.Color), (byte)data.Tool];

    public static byte[] EncodeChunkProtect(Position chunkPos, bool protect) => [.. EncodePos(chunkPos), (byte)(protect ? 1 : 0), 0];

    public static byte[] EncodeChunkFill(Position chunkPos, Color color) => [.. EncodePixel(chunkPos, color), 0, 0];

    public static byte[] EncodeChunkData(Position chunkPos, byte[] data)
    {
        var pos = EncodePos(chunkPos);
        // TODO: set remaining data to white instead?
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
