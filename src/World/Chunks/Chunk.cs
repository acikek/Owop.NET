using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

public class Chunk : IChunk
{
    private readonly World _world;
    public readonly Memory2D<Color> _memory = new(new Color[IChunk.Width, IChunk.Width]);

    public ReadOnlyMemory2D<Color> Pixels => _memory;
    public bool IsProtected { get; set; }
    public DateTime? LastLoad { get; set; }
    public bool IsLoaded { get; set; }
    public Position Pos { get; }
    public Position WorldPos { get; }
    public Position ChunkPos { get; }

    public Chunk(World world, Position chunkPos, bool isProtected)
    {
        _world = world;
        ChunkPos = chunkPos;
        WorldPos = ChunkPos * IChunk.Width;
        Pos = WorldPos * IChunk.Width;
        IsProtected = isProtected;
    }

    // TODO: Bounds-check these?
    public Position GetPosInChunk(Position worldPos) => worldPos - WorldPos;

    public Color SetPixel(Position worldPos, Color color)
    {
        var pos = GetPosInChunk(worldPos);
        var prev = _memory.Span[pos.X, pos.Y];
        _memory.Span[pos.X, pos.Y] = color;
        return prev;
    }

    public Color GetPixel(Position worldPos)
    {
        var pos = GetPosInChunk(worldPos);
        return _memory.Span[pos.X, pos.Y];
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
            int row = index / IChunk.Width;
            int col = index % IChunk.Width;
            this[row, col] = value;
        }
    }

    public async Task Request() => await _world.Chunks.Request(ChunkPos, true);

    public async Task<IChunk> Query() => await _world.Chunks.Query(ChunkPos, true);

    public async Task SetProtected(bool protect) => await _world.Chunks.SetChunkProtected(ChunkPos, protect);

    public async Task Protect() => await _world.Chunks.Protect(ChunkPos);

    public async Task Unprotect() => await _world.Chunks.Unprotect(ChunkPos);

    public async Task Fill(Color? color = null) => await _world.Chunks.Fill(ChunkPos, color);

    public async Task Erase() => await _world.Chunks.Erase(ChunkPos);

    public async Task SetData(byte[] data) => await _world.Chunks.SetChunkData(ChunkPos, data);

    public async Task SetData(Color[] data) => await _world.Chunks.SetChunkData(ChunkPos, data);
}
