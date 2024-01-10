namespace Owop;

public partial class World
{
    public async Task SendChatMessage(string message) => await _instance.QueueChatMessage(message).Task;

    public void QueueChatMessage(string message) => _instance.QueueChatMessage(message);

    public async Task RunCommand(string command, params object[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await SendChatMessage(message);
    }

    public async Task TellPlayer(int id, string message)
        => await RunCommand("tell", id, message);
}
