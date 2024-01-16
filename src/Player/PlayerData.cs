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

    public Position Pos
    {
        get => _pos;
        set
        {
            _pos = value;
            _worldPos = IChunk.GetWorldPos(_pos);
        }
    }

    public Position WorldPos
    {
        get => _worldPos;
        set
        {
            _worldPos = value;
            _pos = value * IChunk.Size;
        }
    }
}