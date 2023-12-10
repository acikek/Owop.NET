namespace Owop;

public partial struct World
{
    public readonly async Task Move(int x, int y)
    {
        Instance.ClientPlayerData.SetPos(x, y);
        await Instance.Connection.SendPlayerData();
    }
}
