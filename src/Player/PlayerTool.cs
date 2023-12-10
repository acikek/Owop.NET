namespace Owop;

/// <summary>A player's selected tool.</summary>
public enum PlayerTool
{
    /// <summary>Default cursor. Places one pixel at a time.</summary>
    Cursor,
    /// <summary>View panning tool.</summary>
    Move,
    /// <summary>Color selection tool.</summary>
    Pipette,
    /// <summary>
    /// <b>Moderator-only</b>. 
    /// Erases an entire chunk, setting its pixels to <see cref="Color.White"/>.
    /// </summary>
    Eraser,
    /// <summary>Changes the view resolution.</summary>
    Zoom,
    /// <summary>Canvas screenshotting tool.</summary>
    Export,
    /// <summary>Continuously replaces adjacent pixels of a certain color.</summary>
    Fill,
    /// <summary>Places pixels in a rasterized line.</summary>
    Line,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Protects a chunk, meaning that only players ranked <see cref="PlayerRank.Moderator"/>
    /// or above will be able to modify it.
    /// </summary>
    Protect,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Protects a set of chunks in some rectangular bounds.
    /// </summary>
    AreaProtect,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Pastes an image onto the canvas.
    /// </summary>
    Paste,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Copies a section of the canvas.
    /// </summary>
    Copy
}
