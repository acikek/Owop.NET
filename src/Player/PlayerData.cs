using System.Drawing;

namespace Owop;

public class PlayerData
{
    public PlayerTool Tool { get; set; } = PlayerTool.Cursor;
    public int Id { get; set; } = 0;
    public Color Color { get; set; } = Color.Black;

    private Point _pos = Point.Empty;
    private Point _worldPos = Point.Empty;

    public Point Pos
    {
        get => _pos;
        set
        {
            _pos = value;
            _worldPos = new(value.X / World.CHUNK_SIZE, value.Y / World.CHUNK_SIZE);
        }
    }

    public Point WorldPos
    {
        get => _worldPos;
        set
        {
            _worldPos = value;
            _pos = new(value.X * World.CHUNK_SIZE, value.Y * World.CHUNK_SIZE);
        }
    }

    public void SetPos(int x, int y)
    {
        Pos = new(x, y);
    }

    public void SetWorldPos(int x, int y)
    {
        WorldPos = new(x, y);
    }
}

public class WorldPlayerData(World world) : PlayerData
{
    public readonly World World = world;
}
