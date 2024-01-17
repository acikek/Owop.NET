using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Owop.Util;

namespace Owop.Game;

public class Chunk : IChunk
{
    public bool _isProtected;
    public readonly Memory2D<Color> _memory = new(new Color[IChunk.Size, IChunk.Size]);

    public ReadOnlyMemory2D<Color> Pixels => _memory;
    public bool IsProtected => _isProtected;
    public Position Pos { get; }
    public Position WorldPos { get; }
    public Position ChunkPos { get; }

    public Chunk(Position chunkPos, bool isProtected)
    {
        ChunkPos = chunkPos;
        WorldPos = ChunkPos * IChunk.Size;
        Pos = WorldPos * IChunk.Size;
        _isProtected = isProtected;
    }

    public void SetPixel(Position worldPos, Color color)
    {
        var posInChunk = worldPos - WorldPos;
        Console.WriteLine($"Setting pixel. WorldPos: {worldPos}, ChunkPos: {ChunkPos}, PosWithinChunk: {posInChunk}, Color: {color}");
        _memory.Span[posInChunk.X, posInChunk.Y] = color;
    }

    public Color this[int x, int y]
    {
        get => _memory.Span[x, y];
        set => _memory.Span[x, y] = value;
    }
}
