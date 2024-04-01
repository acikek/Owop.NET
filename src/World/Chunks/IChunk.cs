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
    const int Width = 16;

    /// <summary>The number of bytes needed to store one chunk.</summary>
    const int DataSize = Width * Width * 3;

    /// <summary>The grid of pixels within the chunk.</summary>
    ReadOnlyMemory2D<Color> Pixels { get; }

    /// <summary>Whether modifying the chunk requires <see cref="PlayerRank.Moderator"/> permissions.</summary>
    bool IsProtected { get; }

    /// <summary>The last time this chunk was loaded by the client.</summary>
    DateTime? LastLoad { get; }

    /// <summary>Whether the client has requested and loaded this chunk at least once.</summary>
    bool IsLoaded { get; }

    /// <summary>Retrieves a pixel at the specified position within the chunk.</summary>
    /// <param name="worldPos">The pixel world position within the chunk.</param>
    /// <returns>The pixel color.</returns>
    Color GetPixel(Position worldPos);

    /// <summary>Retrieves a single pixel within the chunk.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The color of the pixel, <see cref="Color.Empty"/> if none.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    Color this[int x, int y] { get; }

    /// <summary>Requests data from this chunk.</summary>
    Task Request();

    /// <summary>Queries data from this chunk.</summary>
    /// <returns>This chunk with updated data.</returns>
    Task<IChunk> Query();

    /// <summary>Sets whether this chunk is draw-protected.</summary>
    /// <param name="protect">The protection state.</param>
    Task SetProtected(bool protect);

    /// <summary>Protects this chunk.</summary>
    /// <seealso cref="SetProtected"/>
    Task Protect();

    /// <summary>Unprotects this chunk.</summary>
    /// <seealso cref="SetProtected"/>
    Task Unprotect();

    /// <summary>Fills this chunk with a specific color.</summary>
    /// <param name="color">The color to fill with. Defaults to the client player color.</param>
    Task Fill(Color? color = null);

    /// <summary>Erases this chunk by filling it with <see cref="Color.White"/>.</summary>
    Task Erase();

    /// <summary>Sets the raw data of this chunk.</summary>
    /// <param name="data">The chunk data. Gets trimmed or extended to <see cref="DataSize"/>.</param>
    Task SetData(byte[] data);

    /// <summary>Sets the data of this chunk using a <see cref="Color"/> array.</summary>
    /// <param name="data">The chunk data. The resulting byte array gets trimmed or extended to <see cref="IChunk.DataSize"/>.</param>
    Task SetData(Color[] data);
}
