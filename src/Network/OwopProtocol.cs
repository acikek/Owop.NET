using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Owop.Network;

public static class OwopProtocol
{
    public static bool TryReadPoint(SequenceReader<byte> reader, out Point point)
    {
        if (reader.TryReadLittleEndian(out int x) &&
            reader.TryReadLittleEndian(out int y))
        {
            point = new(x, y);
            return true;
        }
        point = new(0, 0);
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
            !TryReadPoint(reader, out Point point) ||
            !TryReadColor(reader, out Color color))
        {
            return false;
        }
        player.Id = id;
        player.Pos = point;
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

    public static byte[] EncodePlayer(PlayerData data)
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(data.Pos.X);
        writer.Write(data.Pos.Y);
        writer.Write(data.Color.R);
        writer.Write(data.Color.G);
        writer.Write(data.Color.B);
        writer.Write((byte)data.Tool);
        return stream.ToArray();
    }
}
