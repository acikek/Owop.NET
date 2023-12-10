namespace Owop;

public partial struct World
{
    public readonly async Task Move(int x, int y)
    {
        Instance.ClientPlayerData.UpdatePos(x, y, Instance.Connection.Client.Options.ChunkSize);
        await Instance.Connection.SendPlayerData();
    }
}
