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
    private World _world = world;
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

    public async Task Request(Position chunkPos, bool force = false)
    {
        if (!force && ContainsKey(chunkPos))
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
        if (!force && TryGetValue(chunkPos, out var chunk) && chunk is IChunk existingChunk)
        {
            return existingChunk;
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
