using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Owop.Network;
using Owop.Util;

namespace Owop;

public class WorldData
{
    public Dictionary<int, WorldPlayerData<Player>> PlayerData = [];
    public Dictionary<int, Player> Players = [];
    public ClientPlayerData ClientPlayerData;

    private readonly ConcurrentQueue<(string, TaskCompletionSource)> _chatBuffer = [];
    private Task? _chatTask = null;

    public ConcurrentDictionary<int, TaskCompletionSource<WhoisData>> WhoisQueue = [];

    public readonly string Name;
    public readonly WorldConnection Connection;
    public readonly World World;
    public bool Connected = false;
    public bool Initialized = false;

    public BucketData ChatBucket => ClientPlayerData.ChatBucketData;

    public WorldData(string name, WorldConnection connection)
    {
        Name = name;
        ClientPlayerData = new(this);
        Connection = connection;
        World = new(this);
        ChatBucket.Bucket.Fill += async (_, value) =>
        {
            if (_chatTask is null)
            {
                await SendNextChatMessage();
            }
        };
    }

    public TaskCompletionSource QueueChatMessage(string message)
    {
        World.Logger.LogDebug($"Queuing chat message: '{message}'");
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _chatBuffer.Enqueue((message, source));
        SendChatMessages();
        return source;
    }

    public async Task SendChatMessage(string message)
    {
        Connection.CheckInteraction();
        World.Logger.LogDebug($"Sending chat message: '{message}'");
        int length = ClientPlayerData.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + Connection.Client.Options.ChatVerification;
        await Connection.Send(data);
    }

    public async Task<bool> SendNextChatMessage()
    {
        if (_chatBuffer.IsEmpty)
        {
            return false;
        }
        if (!_chatBuffer.TryDequeue(out var message))
        {
            return false;
        }
        (string content, var task) = message;
        if (!ChatBucket.TrySpend(1))
        {
            World.Logger.LogError($"Failed to send chat message '{content}'; no allowance left!");
            return false;
        }
        await SendChatMessage(content);
        task.SetResult();
        return true;
    }

    public void SendChatMessages()
    {
        if (_chatTask is not null)
        {
            return;
        }
        _chatTask = Task.Run(async () =>
        {
            World.Logger.LogDebug("Starting chat task...");
            int count = 0;
            while (!_chatBuffer.IsEmpty && !ChatBucket.Bucket.IsEmpty)
            {
                if (await SendNextChatMessage())
                {
                    count++;
                }
            }
            _chatTask = null;
            World.Logger.LogDebug($"Completed chat task with {count} message(s) sent.");
        });
    }
}
