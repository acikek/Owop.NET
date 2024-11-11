using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <summary>An <see cref="IWorldChunks"/> implementation.</summary>
/// <param name="world">The corresponding world.</param>
public class WorldChunks(World world) : Dictionary<Position, IChunk>, IWorldChunks
{
    /// <summary>The internal world instance.</summary>
    private readonly World _world = world;

    /// <summary>An interaction queue for chunk queries.</summary>
    public ConcurrentDictionary<Position, TaskCompletionSource<IChunk>> ChunkQueue = [];

    /// <summary>Gets or creates a chunk at the specified chunk position.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <returns>The chunk at the position.</returns>
    public Chunk GetOrCreate(Position chunkPos)
    {
        if (TryGetValue(chunkPos, out var chunk) && chunk is Chunk existing)
        {
            return existing;
        }
        Chunk newChunk = new(_world, chunkPos, false);
        Add(newChunk.ChunkPos, newChunk);
        return newChunk;
    }

    /// <summary>Sets the pixel color at a world position.</summary>
    /// <param name="worldPos">The pixel position.</param>
    /// <param name="color">The new pixel color.</param>
    /// <returns>A tuple of the corresponding chunk and the previous pixel color.</returns>
    public (Chunk, Color) SetPixel(Position worldPos, Color color)
    {
        var chunk = GetOrCreate(worldPos.ToChunkPos());
        var prev = chunk.SetPixel(worldPos, color);
        return (chunk, prev);
    }

    /// <inheritdoc/>
    public Color? GetPixel(Position worldPos) => TryGetValue(worldPos.ToChunkPos(), out var chunk) ? chunk.GetPixel(worldPos) : null;

    /// <inheritdoc/>
    public async Task<Color> QueryPixel(Position worldPos)
    {
        var chunk = await Query(worldPos.ToChunkPos());
        return chunk.GetPixel(worldPos);
    }

    /// <inheritdoc/>
    public bool IsChunkLoaded(Position chunkPos, out IChunk? chunk)
    {
        if (TryGetValue(chunkPos, out var existing) && existing.IsLoaded)
        {
            chunk = existing;
            return true;
        }
        chunk = null;
        return false;
    }

    /// <inheritdoc/>
    public bool IsChunkLoaded(Position chunkPos) => IsChunkLoaded(chunkPos, out _);

    /// <inheritdoc/>
    public async Task Request(Position chunkPos, bool force = false)
    {
        chunkPos = chunkPos.ClampToBorder();
        if (!force && IsChunkLoaded(chunkPos))
        {
            await Task.CompletedTask;
            return;
        }
        await _world._connection.Send(OwopProtocol.EncodePos(chunkPos));
    }

    /// <inheritdoc/>
    public async Task<IChunk> Query(Position chunkPos, bool force = false)
    {
        if (!force && IsChunkLoaded(chunkPos, out var chunk) && chunk is not null)
        {
            return chunk;
        }
        if (ChunkQueue.TryGetValue(chunkPos, out var existingSource))
        {
            return await existingSource.Task;
        }
        var source = new TaskCompletionSource<IChunk>(TaskCreationOptions.RunContinuationsAsynchronously);
        ChunkQueue[chunkPos] = source;
        await Request(chunkPos, force);
        return await source.Task;
    }

    /// <inheritdoc/>
    public async Task SetChunkProtected(Position chunkPos, bool protect)
    {
        _world._connection.CheckInteraction(PlayerRank.Moderator);
        await _world._connection.Send(OwopProtocol.EncodeChunkProtect(chunkPos, protect));
    }

    /// <inheritdoc/>
    public async Task Protect(Position chunkPos) => await SetChunkProtected(chunkPos, true);

    /// <inheritdoc/>
    public async Task Unprotect(Position chunkPos) => await SetChunkProtected(chunkPos, false);

    /// <inheritdoc/>
    public async Task Fill(Position chunkPos, Color? color = null)
    {
        _world._connection.CheckInteraction(PlayerRank.Moderator);
        if (!_world._clientPlayer._pixelBucket.TrySpend(1.0))
        {
            return;
        }
        var fillColor = color ?? _world.ClientPlayer.Color;
        await _world._connection.Send(OwopProtocol.EncodeChunkFill(chunkPos, fillColor));
    }

    /// <inheritdoc/>
    public async Task Erase(Position chunkPos) => await Fill(chunkPos, OwopColors.White);

    /// <inheritdoc/>
    public async Task SetChunkData(Position chunkPos, byte[] data)
    {
        _world._connection.CheckInteraction(PlayerRank.Moderator);
        await _world._connection.Send(OwopProtocol.EncodeChunkData(chunkPos, data));
    }

    /// <inheritdoc/>
    // TODO: (post-release) Is there a better way to do this?
    public async Task SetChunkData(Position chunkPos, Color[] data)
    {
        byte[] result = new byte[IChunk.DataSize];
        for (int i = 0; i < data.Length; i++)
        {
            var idx = i * 3;
            var color = data[i];
            result[idx] = color.R;
            result[idx + 1] = color.G;
            result[idx + 2] = color.B;
        }
        await SetChunkData(chunkPos, result);
    }
}
