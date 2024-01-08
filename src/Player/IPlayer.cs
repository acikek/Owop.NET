using System.Drawing;
using Owop.Network;
using Owop.Util;

namespace Owop;

public interface IPlayer
{
    World World { get; }
    Position Pos { get; }
    Position WorldPos { get; }
    PlayerTool Tool { get; }
    int Id { get; }
    Color Color { get; }

    public Task Move(Position pos);
    public Task MoveWorld(Position worldPos);
    public Task TeleportTo();
    public Task Tell(string message);
    public Task<WhoisData> QueryWhois();
}
