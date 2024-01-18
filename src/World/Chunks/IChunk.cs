using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Owop.Util;

namespace Owop.Game;

/// <summary>Represents a square chunk of pixels within an <see cref="IWorld">.</summary>
public interface IChunk : IPositioned
{
    /// <summary>The width and height of a chunk.</summary>
    const int Size = 16;

    /// <summary>The grid of pixels within the chunk.</summary>
    ReadOnlyMemory2D<Color> Pixels { get; }

    /// <summary>Whether modifying the chunk requires <see cref="PlayerRank.Moderator"/> permissions.</summary>
    bool IsProtected { get; }

    /// <summary>Retrieves a single pixel within the chunk.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The color of the pixel, <see cref="Color.Empty"/> if none.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    Color this[int x, int y] { get; }

    Task Request();

    Task Query();
}
