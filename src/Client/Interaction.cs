using Owop.Game;

namespace Owop;

public partial class Client
{
    public const string CHAT_VERIFICATION = "\u000A";

    public async Task Chat(string message)
    {
        int length = Player.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + CHAT_VERIFICATION;
        await Task.Run(() => Connection.Send(data));
    }

    public async Task RunCommand(string command, params string[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await Chat(message);
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
