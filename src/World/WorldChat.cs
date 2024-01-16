using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Owop.Game;

public partial class World
{
    private readonly ConcurrentQueue<(string, TaskCompletionSource)> _chatBuffer = [];
    private Task? _chatTask = null;

    public async Task SendChatMessage(string message) => await QueueChatMessageInternal(message).Task;

    public void QueueChatMessage(string message) => QueueChatMessageInternal(message);

    public async Task RunCommand(string command, params object[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await SendChatMessage(message);
    }

    public async Task TellPlayer(int id, string message)
        => await RunCommand("tell", id, message);

    public TaskCompletionSource QueueChatMessageInternal(string message)
    {
        Logger.LogDebug($"Queuing chat message: '{message}'");
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _chatBuffer.Enqueue((message, source));
        SendChatMessages();
        return source;
    }

    public async Task SendChatMessageInternal(string message)
    {
        _connection.CheckInteraction();
        Logger.LogDebug($"Sending chat message: '{message}'");
        int length = ClientPlayer.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + Connection.Client.Options.ChatVerification;
        await Connection.Send(data);
    }

    public void SendChatMessages()
    {
        if (_chatTask is not null)
        {
            return;
        }
        _chatTask = Task.Run(async () =>
        {
            Logger.LogDebug("Starting chat task...");
            int count = 0;
            while (!_chatBuffer.IsEmpty)
            {
                if (!_chatBuffer.TryDequeue(out var message))
                {
                    continue;
                }
                (string content, var task) = message;
                await _clientPlayer._chatBucket.DelayAny();
                if (!_clientPlayer._chatBucket.TrySpend(1))
                {
                    Logger.LogError($"Failed to send chat message '{content}'; no allowance left!");
                    continue;
                }
                await SendChatMessageInternal(content);
                count++;
                task.SetResult();
            }
            _chatTask = null;
            Logger.LogDebug($"Completed chat task with {count} message(s) sent.");
        });
    }
}
