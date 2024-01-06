using System.Buffers;
using System.Drawing;
using Owop.Util;

namespace Owop.Network;

// TODO: extension methods?
public static class OwopProtocol
{
    public static bool TryReadPos(SequenceReader<byte> reader, out Position point)
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

    public static bool TryReadColor(SequenceReader<byte> reader, out Color color)
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

    public static bool TryReadPlayer(SequenceReader<byte> reader, bool hasTool, out PlayerData player)
    {
        player = new();
        if (!reader.TryReadLittleEndian(out int id) ||
            !TryReadPos(reader, out Position pos) ||
            !TryReadColor(reader, out Color color))
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

    public static bool TryReadBucket(SequenceReader<byte> reader, out PixelBucketData bucket)
    {
        if (reader.TryReadLittleEndian(out short capacity) &&
            reader.TryReadLittleEndian(out short seconds))
        {
            bucket = new(capacity, seconds);
            return true;
        }
        bucket = PixelBucketData.Empty;
        return false;
    }

    public static byte[] EncodePlayer(PlayerData data)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(EncodePixel(data.Pos, data.Color));
        writer.Write((byte)data.Tool);
        return stream.ToArray();
    }

    public static byte[] EncodePixel(Position pos, Color color)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(pos.X);
        writer.Write(pos.Y);
        writer.Write(color.R);
        writer.Write(color.G);
        writer.Write(color.B);
        return stream.ToArray();
    }
}
