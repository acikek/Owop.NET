using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;
using Owop;

namespace Owop.Game;

/// <summary>Represents internal player data.</summary>
public class PlayerData
{
    /// <summary>The player's selected tool.</summary>
    public PlayerTool Tool { get; set; } = PlayerTool.Cursor;

    /// <summary>The player's ID.</summary>
    public int Id { get; set; } = 0;

    /// <summary>The player's selected color.</summary>
    public Color Color { get; set; } = Color.Black;

    /// <summary>The internal raw position within the world.</summary>
    private Position _pos = Position.Origin;

    /// <summary>The internal pixel position within the world.</summary>
    private Position _worldPos = Position.Origin;

    /// <summary>The internal chunk position within the world.</summary>
    private Position _chunkPos = Position.Origin;

    /// <summary>Gets or sets the raw position.</summary>
    public Position Pos
    {
        get => _pos;
        set
        {
            if (value == _pos)
            {
                return;
            }
            _pos = value;
            _worldPos = _pos.ToWorldPos();
            _chunkPos = _worldPos.ToChunkPos();
        }
    }

    /// <summary>Gets or sets the pixel position.</summary>
    public Position WorldPos
    {
        get => _worldPos;
        set
        {
            if (value == _worldPos)
            {
                return;
            }
            _pos = value * IChunk.Width;
            _worldPos = value;
            _chunkPos = value.ToChunkPos();
        }
    }

    /// <summary>Gets or sets the chunk position.</summary>
    public Position ChunkPos
    {
        get => _chunkPos;
        set
        {
            if (value == _chunkPos)
            {
                return;
            }
            _worldPos = value * IChunk.Width;
            _pos = _worldPos * IChunk.Width;
            _chunkPos = value;
        }
    }
}
