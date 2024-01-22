using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

public class WorldChunks(World world) : Dictionary<Position, IChunk>, IWorldChunks
{
    private readonly World _world = world;
    public ConcurrentDictionary<Position, TaskCompletionSource<IChunk>> ChunkQueue = [];

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

    public Chunk SetPixel(Position worldPos, Color color)
    {
        var chunk = GetOrCreate(worldPos.ToChunkPos());
        chunk.SetPixel(worldPos, color);
        return chunk;
    }

    public Color? GetPixel(Position worldPos)
    {
        if (!TryGetValue(worldPos.ToChunkPos(), out var chunk))
        {
            return null;
        }
        return chunk.GetPixel(worldPos);
    }

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

    public bool IsChunkLoaded(Position chunkPos) => IsChunkLoaded(chunkPos, out _);

    public async Task Request(Position chunkPos, bool force = false)
    {
        if (!force && IsChunkLoaded(chunkPos))
        {
            await Task.CompletedTask;
            return;
        }
        // TODO: Check for world border
        byte[] pos = OwopProtocol.EncodePos(chunkPos);
        await _world._connection.Send(pos);
    }

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
}
