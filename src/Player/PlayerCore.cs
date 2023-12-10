using System.Drawing;

namespace Owop;

public partial struct Player
{
    private WorldPlayerData Instance;

    public readonly World World => Instance.World;
    public readonly Point Pos => Instance.Pos;
    public readonly Point WorldPos => Instance.WorldPos;
    public readonly PlayerTool Tool => Instance.Tool;
    public readonly int Id => Instance.Id;
    public readonly Color Color => Instance.Color;

    public static implicit operator Player(WorldPlayerData data) => new() { Instance = data };
}
