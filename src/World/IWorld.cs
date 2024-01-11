using Microsoft.Extensions.Logging;

namespace Owop;

public interface IWorld
{
    string Name { get; }
    Dictionary<int, Player> Players { get; }
    ClientPlayer ClientPlayer { get; }
    ILogger Logger { get; }
    bool IsChatReady { get; }

    IPlayer GetPlayerById(int id) => Players.TryGetValue(id, out var player) ? player : ClientPlayer;
}
