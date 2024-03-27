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
            _pos = value * IChunk.Size;
            _worldPos = value;
            _chunkPos = value.ToChunkPos();
        }
    }

    public Position ChunkPos
    {
        get => _chunkPos;
        set
        {
            _worldPos = value * IChunk.Size;
            _pos = _worldPos * IChunk.Size;
            _chunkPos = value;
        }
    }
}
