namespace Owop;

public partial struct World
{
    public const string CHAT_VERIFICATION = "\u000A";
    public const int CHAT_TIMEOUT = 2000;

    public async readonly Task SendChatMessage(string message)
    {
        int length = ClientRank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + CHAT_VERIFICATION;
        await Instance.Connection.Send(data);
    }

    public async readonly Task RunCommand(string command, params string[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await SendChatMessage(message);
    }

    public async readonly Task SetNickname(string nickname)
    {
        Instance.ClientNickname = nickname;
        await RunCommand("nick", nickname);
    }

    public async readonly Task Tell(uint id, string message)
    {
        await RunCommand("tell", id.ToString(), message);
    }
}
