using System.Drawing;

namespace Owop;

public class PlayerData(World world)
{
    public readonly World World = world;
    public Point Pos { get; private set; } = Point.Empty;
    public Point WorldPos { get; private set; } = Point.Empty;
    public PlayerTool Tool { get; set; } = PlayerTool.Cursor;
    public int Id { get; set; } = 0;
    public Color Color { get; set; } = Color.Black;

    public void UpdatePos(int x, int y, int chunkSize)
    {
        Pos = new(x, y);
        WorldPos = new(x / chunkSize, y / chunkSize);
    }

    public void UpdateWorldPos(int x, int y, int chunkSize)
    {
        Pos = new(x * chunkSize, y * chunkSize);
        WorldPos = new(x, y);
    }
}
