using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;

namespace Owop.Game;

public interface IWorldChunks : IReadOnlyDictionary<Position, IChunk>
{
    bool IsChunkLoaded(Position chunkPos);

    bool IsChunkLoaded(Position chunkPos, out IChunk? chunk);

    Color? GetPixel(Position worldPos);

    Task Request(Position chunkPos, bool force = false);

    Task<IChunk> Query(Position chunkPos, bool force = false);
}
