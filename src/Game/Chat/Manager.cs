namespace Owop;

public class ChatManager(OwopClient client)
{
    public const string CHAT_VERIFICATION = "\u000A";

    private readonly OwopClient Client = client;
    public string? Nickname { get; private set; }

    public async Task Send(string message)
    {
        int length = Client.Player.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + CHAT_VERIFICATION;
        await Task.Run(() => Client.Connection.Socket.Send(data));
    }

    public async Task RunCommand(string command, params string[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await Send(message);
    }

    public async Task SetNickname(string nickname)
    {
        Nickname = nickname;
        await RunCommand("nick", nickname);
    }

    public async Task Tell(uint id, string message)
    {
        await RunCommand("tell", id.ToString(), message);
    }
}
