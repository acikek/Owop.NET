using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Util;
using Owop;

namespace Owop.Game;

public class PlayerData
{
    public PlayerTool Tool { get; set; } = PlayerTool.Cursor;
    public int Id { get; set; } = 0;
    public Color Color { get; set; } = Color.Black;

    private Position _pos = Position.Origin;
    private Position _worldPos = Position.Origin;
    private Position _chunkPos = Position.Origin;

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
