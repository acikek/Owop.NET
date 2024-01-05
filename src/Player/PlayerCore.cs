using System.Drawing;

namespace Owop;

public readonly partial struct Player(WorldPlayerData data)
{
    private readonly WorldPlayerData _instance = data;

    public readonly World World => _instance.WorldData;
    public readonly Point Pos => _instance.Pos;
    public readonly Point WorldPos => _instance.WorldPos;
    public readonly PlayerTool Tool => _instance.Tool;
    public readonly int Id => _instance.Id;
    public readonly Color Color => _instance.Color;

    public static implicit operator Player(WorldPlayerData data) => new(data);
}
