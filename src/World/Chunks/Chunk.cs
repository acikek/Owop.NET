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

/// <summary>An <see cref="IChunk"/> implementation.</summary>
public class Chunk : IChunk
{
    /// <summary>The internal world instance.</summary>
    private readonly World _world;

    /// <summary>The internal pixel grid.</summary>
    public readonly Memory2D<Color> _memory = new(new Color[IChunk.Width, IChunk.Width]);

    /// <inheritdoc/>
    public ReadOnlyMemory2D<Color> Pixels => _memory;

    /// <inheritdoc/>
    public bool IsProtected { get; set; }

    /// <inheritdoc/>
    public DateTime? LastLoad { get; set; }

    /// <inheritdoc/>
    public bool IsLoaded { get; set; }

    /// <inheritdoc/>
    public Position Pos { get; }

    /// <inheritdoc/>
    public Position WorldPos { get; }

    /// <inheritdoc/>
    public Position ChunkPos { get; }

    /// <summary>Constructs a <see cref="Chunk"/>.</summary>
    /// <param name="world">The world instance.</param>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="isProtected">Whether the chunk is protected.</param>
    public Chunk(World world, Position chunkPos, bool isProtected)
    {
        _world = world;
        ChunkPos = chunkPos;
        WorldPos = ChunkPos * IChunk.Width;
        Pos = WorldPos * IChunk.Width;
        IsProtected = isProtected;
    }

    /// <summary>Returns the pixel position relative to the chunk position.</summary>
    /// <param name="worldPos">The pixel position.</param>
    public Position GetPosInChunk(Position worldPos) => worldPos - WorldPos;

    /// <summary>Sets a pixel color within this chunk.</summary>
    /// <param name="worldPos">The pixel position.</param>
    /// <param name="color">The new pixel color.</param>
    /// <returns>The previous pixel color.</returns>
    public Color SetPixel(Position worldPos, Color color)
    {
        var pos = GetPosInChunk(worldPos);
        var prev = _memory.Span[pos.X, pos.Y];
        _memory.Span[pos.X, pos.Y] = color;
        return prev;
    }

    /// <inheritdoc/>
    public Color GetPixel(Position worldPos)
    {
        var pos = GetPosInChunk(worldPos);
        return _memory.Span[pos.X, pos.Y];
    }

    /// <inheritdoc/>
    public Color this[int x, int y]
    {
        get => _memory.Span[x, y];
        set => _memory.Span[x, y] = value;
    }

    /// <inheritdoc/>
    public Color this[int index]
    {
        set
        {
            int row = index / IChunk.Width;
            int col = index % IChunk.Width;
            this[row, col] = value;
        }
    }

    /// <inheritdoc/>
    public async Task Request() => await _world.Chunks.Request(ChunkPos, true);

    /// <inheritdoc/>
    public async Task<IChunk> Query() => await _world.Chunks.Query(ChunkPos, true);

    /// <inheritdoc/>
    public async Task SetProtected(bool protect) => await _world.Chunks.SetChunkProtected(ChunkPos, protect);

    /// <inheritdoc/>
    public async Task Protect() => await _world.Chunks.Protect(ChunkPos);

    /// <inheritdoc/>
    public async Task Unprotect() => await _world.Chunks.Unprotect(ChunkPos);

    /// <inheritdoc/>
    public async Task Fill(Color? color = null) => await _world.Chunks.Fill(ChunkPos, color);

    /// <inheritdoc/>
    public async Task Erase() => await _world.Chunks.Erase(ChunkPos);

    /// <inheritdoc/>
    public async Task SetData(byte[] data) => await _world.Chunks.SetChunkData(ChunkPos, data);

    /// <inheritdoc/>
    public async Task SetData(Color[] data) => await _world.Chunks.SetChunkData(ChunkPos, data);
}
