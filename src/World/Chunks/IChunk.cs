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

    /// <summary>Converts a precise position to a pixel-based world position.</summary>
    /// <param name="pos">The precise position.</param>
    static Position GetWorldPos(Position pos)
    {
        // OWOP rounds up Y values (since up = negative Y)
        int y = pos.Y / Size;
        if (pos.Y % Size != 0)
        {
            y--;
        }
        return new(pos.X / Size, y);
    }

    /// <summary>Converts a pixel-based world position to its chunk's position.</summary>
    /// <param name="worldPos">The world position.</param>
    static Position GetChunkPos(Position worldPos)
        => ((int)Math.Floor((decimal)worldPos.X / Size), (int)Math.Floor((decimal)worldPos.Y / Size));
}
