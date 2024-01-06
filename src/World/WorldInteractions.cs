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

    public async readonly Task SetRestricted(bool restricted)
        => await RunCommand("restrict", restricted.ToString());

    public async readonly Task Restrict() => await SetRestricted(true);

    public async readonly Task Unrestrict() => await SetRestricted(false);
}
