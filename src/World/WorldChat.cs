namespace Owop;

public partial class World
{
    public async Task SendChatMessage(string message)
    {
        int length = ClientPlayer.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + _instance.Connection.Client.Options.ChatVerification;
        await _instance.Connection.Send(data);
    }

    public async Task RunCommand(string command, params string[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await SendChatMessage(message);
    }

    public async Task TellPlayer(int id, string message)
        => await RunCommand("tell", id.ToString(), message);
}
