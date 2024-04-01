using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;

namespace Owop.Game;

/// <summary>Represents the chunks within an <see cref="IWorld"/>.</summary>
public interface IWorldChunks : IReadOnlyDictionary<Position, IChunk>
{
    /// <summary>Returns whether the client has loaded a chunk position.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <returns>Whether the chunk has been loaded.</returns>
    bool IsChunkLoaded(Position chunkPos);

    /// <summary>Returns whether the client has loaded a chunk position.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="chunk">The chunk, if loaded.</param>
    /// <returns>Whether the chunk has been loaded.</returns>
    bool IsChunkLoaded(Position chunkPos, out IChunk? chunk);

    /// <summary>Retrieves a pixel at a position.</summary>
    /// <param name="worldPos">The pixel world position.</param>
    /// <returns>The pixel color, or <c>null</c> if the corresponding chunk isn't loaded.</returns>
    Color? GetPixel(Position worldPos);

    /// <summary>Queries a pixel at a position, requesting the chunk if necessary.</summary>
    /// <param name="worldPos">The pixel world position.</param>
    /// <returns>The pixel color.</returns>
    Task<Color> QueryPixel(Position worldPos);

    /// <summary>Loads chunk data at the specified chunk position.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="force">Whether to request data even if it has already been loaded.</param>
    Task Request(Position chunkPos, bool force = false);

    /// <summary>Queries chunk data at the specified chunk position.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="force">Whether to request data even if it has already been loaded.</param>
    /// <returns>The queried chunk data.</returns>
    Task<IChunk> Query(Position chunkPos, bool force = false);

    /// <summary>Sets whether a chunk is draw-protected.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="protect">The protection state.</param>
    Task SetChunkProtected(Position chunkPos, bool protect);

    /// <summary>Protects a chunk.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <seealso cref="SetChunkProtected"/> 
    Task Protect(Position chunkPos);

    /// <summary>Unprotects a chunk.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <seealso cref="SetChunkProtected"/> 
    Task Unprotect(Position chunkPos);

    /// <summary>Fills a chunk with a specific color.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    /// <param name="color">The color to fill with. Defaults to the client player color.</param>
    Task Fill(Position chunkPos, Color? color = null);

    /// <summary>Erases a chunk by filling it with <see cref="Color.White"/>.</summary>
    /// <param name="chunkPos">The chunk position.</param>
    Task Erase(Position chunkPos);
}
