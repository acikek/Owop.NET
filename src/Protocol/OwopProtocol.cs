using System.Buffers;
using System.Drawing;

namespace Owop.Protocol;

public static class OwopProtocol
{
    public static bool ReadPoint(SequenceReader<byte> reader, out Point point)
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

    public static bool ReadColor(SequenceReader<byte> reader, out Color color)
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
}
