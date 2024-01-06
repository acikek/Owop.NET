namespace Owop;

public partial class World
{
    public async Task Login(string password)
    {
        await RunCommand("pass", password);
    }

    public async Task MovePlayer(int id, int x, int y)
    {
        _instance.Connection.CheckRank(PlayerRank.Moderator);
        await RunCommand("tp", id.ToString(), x.ToString(), y.ToString());
    }

    public async Task SetRestricted(bool restricted)
        => await RunCommand("restrict", restricted.ToString());

    public async Task Restrict() => await SetRestricted(true);

    public async Task Unrestrict() => await SetRestricted(false);
}
