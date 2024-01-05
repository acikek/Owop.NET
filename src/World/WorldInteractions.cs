namespace Owop;

public partial struct World
{
    public async readonly Task Login(string password)
    {
        await RunCommand("pass", password);
    }

    public async readonly Task MovePlayer(int id, int x, int y)
    {
        _instance.Connection.CheckRank(PlayerRank.Moderator);
        await RunCommand("tp", id.ToString(), x.ToString(), y.ToString());
    }
}
