using System.Buffers;
using System.Drawing;
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

    public static bool TryDecompressChunk(ref this SequenceReader<byte> reader, World world, out Chunk? chunk)
    {
        chunk = null;
        if (!reader.TryReadPos(out var chunkPos) ||
            !reader.TryRead(out byte protectedByte) ||
            !reader.TryReadChunkAmount(out int length) ||
            !reader.TryReadChunkAmount(out int segments))
        {
            return false;
        }
        MemoryStream stream = new(length);
        BinaryWriter writer = new(stream);
        chunk = world._chunks.GetOrCreate(chunkPos);
        chunk.IsProtected = protectedByte == 1;
        Console.WriteLine(string.Join(' ', reader.UnreadSequence.ToArray()));
        int[] repeatTimes = new int[segments];
        for (int i = 0; i < segments; i++)
        {
            if (!reader.TryReadChunkAmount(out int times))
            {
                return false;
            }
            Console.WriteLine("Times: " + times);
            for (int j = 0; j < times; j++)
            {
                if (reader.TryRead(out byte next))
                {
                    writer.Write(next);
                }
            }
            if (!reader.TryReadChunkAmount(out int repeat) ||
                !reader.TryReadColor(out var repeatColor))
            {
                return false;
            }
            byte[] repeatColorData = EncodeColor(repeatColor);
            for (int k = 0; k < repeat; k++)
            {
                writer.Write(repeatColorData);
            }
        }
        while (!reader.End && reader.TryRead(out byte next))
        {
            writer.Write(next);
        }
        Console.WriteLine(string.Join(' ', stream.ToArray()));
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

    public static byte[] EncodePixel(Position pos, Color color)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(EncodePos(pos));
        writer.Write(EncodeColor(color));
        return stream.ToArray();
    }

    public static byte[] EncodePlayer(Player data)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(EncodePixel(data.Pos, data.Color));
        writer.Write((byte)data.Tool);
        return stream.ToArray();
    }
}
