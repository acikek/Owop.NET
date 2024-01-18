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
    private readonly World _world;
    public readonly Memory2D<Color> _memory = new(new Color[IChunk.Size, IChunk.Size]);

    public ReadOnlyMemory2D<Color> Pixels => _memory;
    public bool IsProtected { get; set; }
    public Position Pos { get; }
    public Position WorldPos { get; }
    public Position ChunkPos { get; }

    public Chunk(World world, Position chunkPos, bool isProtected)
    {
        _world = world;
        ChunkPos = chunkPos;
        WorldPos = ChunkPos * IChunk.Size;
        Pos = WorldPos * IChunk.Size;
        IsProtected = isProtected;
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

    public Color this[int index]
    {
        set
        {
            int row = index / IChunk.Size;
            int col = index % IChunk.Size;
            this[row, col] = value;
        }
    }

    public async Task Request() => await _world.Chunks.Request(ChunkPos, true);

    public async Task Query() => await _world.Chunks.Query(ChunkPos, true);
}
