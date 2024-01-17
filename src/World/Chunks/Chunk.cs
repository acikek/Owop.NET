using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Owop.Util;

namespace Owop.Game;

public class Chunk(Position pos, bool isProtected) : IChunk
{
    public bool _isProtected = isProtected;
    private readonly Memory2D<Color> _memory = new(new Color[IChunk.Size, IChunk.Size]);

    public ReadOnlyMemory2D<Color> Pixels => _memory;
    public bool IsProtected => _isProtected;
    public Position Pos { get; } = pos;
    public Position WorldPos { get; } = pos / IChunk.Size;

    public Color this[int x, int y]
    {
        get => _memory.Span[x, y];
        set => _memory.Span[x, y] = value;
    }
}
