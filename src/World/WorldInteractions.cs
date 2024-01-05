namespace Owop;

public partial struct World
{
    public readonly async Task Move(int x, int y)
    {
        _instance.ClientPlayerData.SetPos(x, y);
        await _instance.Connection.SendPlayerData();
    }
}
