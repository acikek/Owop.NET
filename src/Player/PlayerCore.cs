using System.Drawing;
using Owop.Util;

namespace Owop;

public partial class Player(WorldPlayerData<Player> data) : IPlayer
{
    private readonly WorldPlayerData<Player> _instance = data;

    public World World => _instance.WorldData;
    public Position Pos => _instance.Pos;
    public Position WorldPos => _instance.WorldPos;
    public PlayerTool Tool => _instance.Tool;
    public int Id => _instance.Id;
    public Color Color => _instance.Color;
    public bool IsClient => false;

    public static implicit operator Player(WorldPlayerData<Player> data) => data.Player;
}
