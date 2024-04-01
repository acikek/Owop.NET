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

    public async Task<Color> QueryPixel(Position worldPos)
    {
        var chunk = await Query(worldPos.ToChunkPos());
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
        await _world._connection.Send(OwopProtocol.EncodePos(chunkPos));
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

    public async Task SetChunkProtected(Position chunkPos, bool protect)
    {
        _world._connection.CheckInteraction(PlayerRank.Moderator);
        await _world._connection.Send(OwopProtocol.EncodeChunkProtect(chunkPos, protect));
    }

    public async Task Protect(Position chunkPos) => await SetChunkProtected(chunkPos, true);

    public async Task Unprotect(Position chunkPos) => await SetChunkProtected(chunkPos, false);

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

    public async Task Erase(Position chunkPos) => await Fill(chunkPos, OwopColors.White);

    public async Task SetChunkData(Position chunkPos, byte[] data)
    {
        _world._connection.CheckInteraction(PlayerRank.Moderator);
        await _world._connection.Send(OwopProtocol.EncodeChunkData(chunkPos, data));
    }

    // TODO: Find a better way to do this
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
