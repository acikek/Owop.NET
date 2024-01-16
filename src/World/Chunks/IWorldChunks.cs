using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;

namespace Owop.Game;

public interface IWorldChunks : IReadOnlyDictionary<Position, IChunk>
{


    IChunk? this[int x, int y] { get; }
}
