using System.Drawing;

namespace Owop;

public partial class Player(WorldPlayerData<Player> data) : IPlayer
{
    private readonly WorldPlayerData<Player> _instance = data;

    public World World => _instance.WorldData;
    public Point Pos => _instance.Pos;
    public Point WorldPos => _instance.WorldPos;
    public PlayerTool Tool => _instance.Tool;
    public int Id => _instance.Id;
    public Color Color => _instance.Color;

    public static implicit operator Player(WorldPlayerData<Player> data) => data.Player;
}
