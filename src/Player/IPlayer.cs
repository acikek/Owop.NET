using System.Drawing;

namespace Owop;

public interface IPlayer
{
    World World { get; }
    Point Pos { get; }
    Point WorldPos { get; }
    PlayerTool Tool { get; }
    int Id { get; }
    Color Color { get; }

    public Task Move(int x, int y);
    public Task TeleportTo();
    public Task Tell(string message);
}
