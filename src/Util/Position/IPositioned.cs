using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Game;

namespace Owop.Util;

/// <summary>Represents an object that has a pixel position within an <see cref="IWorld"/>.</summary>
public interface IPositioned
{
    /// <summary>The raw position within the world.</summary>
    Position Pos { get; }

    /// <summary>The pixel position within the world.</summary>
    Position WorldPos { get; }

    /// <summary>The chunk position within the world.</summary>
    Position ChunkPos { get; }
}
