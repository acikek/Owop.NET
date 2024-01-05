namespace Owop;

public partial struct World
{
    public async readonly Task SendChatMessage(string message)
    {
        int length = ClientPlayer.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + _instance.Connection.Client.Options.ChatVerification;
        await _instance.Connection.Send(data);
    }

    public async readonly Task RunCommand(string command, params string[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await SendChatMessage(message);
    }

    public async readonly Task SetNickname(string nickname)
    {
        _instance.ClientPlayerData.Nickname = nickname;
        await RunCommand("nick", nickname);
    }

    public async readonly Task Tell(int id, string message)
    {
        await RunCommand("tell", id.ToString(), message);
    }
}
