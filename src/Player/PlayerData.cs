using System.Drawing;
using Owop.Util;

namespace Owop;

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
            _worldPos = value / World.ChunkSize;
        }
    }

    public Position WorldPos
    {
        get => _worldPos;
        set
        {
            _worldPos = value;
            _pos = value * World.ChunkSize;
        }
    }

    public static WorldPlayerData<Player> Create(WorldData worldData)
    {
        var data = new WorldPlayerData<Player>(worldData, null!);
        data.Player = new Player(data);
        return data;
    }
}

public class WorldPlayerData<T>(WorldData worldData, T player) : PlayerData
{
    public readonly WorldData WorldData = worldData;
    public World World => WorldData;
    public T Player = player;
}
