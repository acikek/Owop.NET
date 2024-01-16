using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;

namespace Owop.Game;

public class WorldChunks : Dictionary<Position, IChunk>, IWorldChunks
{
    public IChunk? this[int x, int y]
    {
        get => TryGetValue((x, y), out var color) ? color : default;
        set
        {
            if (value is IChunk chunk)
            {
                this[(x, y)] = chunk;
            }
        }
    }
}
