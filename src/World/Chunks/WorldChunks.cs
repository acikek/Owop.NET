using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;

namespace Owop.Game;

public class WorldChunks : Dictionary<Position, IChunk>, IWorldChunks
{
    public IChunk? GetForWorldPos(Position worldPos) 
        => TryGetValue(IChunk.GetChunkPos(worldPos), out var chunk) ? chunk : null;

    public Chunk GetOrCreateForWorldPos(Position worldPos)
    {
        if (GetForWorldPos(worldPos) is IChunk chunk)
        {
            return (Chunk)chunk;
        }
        Chunk newChunk = new(IChunk.GetChunkPos(worldPos), false);
        Add(newChunk.ChunkPos, newChunk);
        return newChunk;
    }

    public void SetPixel(Position worldPos, Color color) => GetOrCreateForWorldPos(worldPos).SetPixel(worldPos, color);

    public IChunk? this[int x, int y]
    {
        get => TryGetValue((x, y), out var chunk) ? chunk : null;
        set
        {
            if (value is IChunk chunk)
            {
                this[(x, y)] = chunk;
            }
        }
    }
}
