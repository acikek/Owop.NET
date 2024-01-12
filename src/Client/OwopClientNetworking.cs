using System.Buffers;
using System.Drawing;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Owop.Network;
using Owop.Util;
using Websocket.Client;

namespace Owop.Client;

public partial class OwopClient
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HttpClient _httpClient;

    public ServerInfo? ServerInfo { get; private set; }

    public async Task<ServerInfo?> FetchServerInfo()
    {
        var json = await _httpClient.GetStringAsync(Options.ApiUrl);
        var serverInfo = JsonSerializer.Deserialize<ServerInfo>(json, s_jsonOptions);
        ServerInfo = serverInfo;
        return serverInfo;
    }

    public void HandleMessage(ResponseMessage response, World world)
    {
        if (response.Text is string text)
        {
            HandleTextMessage(text, world);
        }
        if (response.Binary is byte[] binary)
        {
            var data = new ReadOnlySequence<byte>(binary);
            var reader = new SequenceReader<byte>(data);
            if (reader.TryRead(out byte opcode))
            {
                HandleOpcode((Opcode)opcode, reader, world);
            }
        }
    }

    private void HandleSetId(SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadLittleEndian(out int id))
        {
            world._clientPlayer.Id = id;
            world._players[id] = world.ClientPlayer;
        }
    }

    private void TryInitWorld(World world)
    {
        if (world.Initialized)
        {
            return;
        }
        bool reconnect = world.Connected;
        if (!reconnect)
        {
            world.Connected = true;
        }
        Connected?.Invoke(this, new(world, reconnect));
        if (!reconnect)
        {
            Ready?.Invoke(this, world);
            Task.Run(async () =>
            {
                await Task.Delay(world.ClientPlayer.ChatBucket.FillInterval);
                world.IsChatReady = true;
                ChatReady?.Invoke(this, world);
            });
        }
        world.Logger.LogDebug("World initialized.");
        world.Initialized = true;
    }

    private void HandleWorldUpdate(SequenceReader<byte> reader, World world)
    {
        if (reader.TryRead(out byte playerCount))
        {
            for (byte i = 0; i < playerCount; i++)
            {
                if (!reader.TryReadPlayer(hasTool: true, out PlayerData data) ||
                    data.Id == world.ClientPlayer.Id)
                {
                    break;
                }
                bool newConnection = !world.Players.ContainsKey(data.Id);
                if (newConnection)
                {
                    world._players[data.Id] = new Player(world);
                }
                var player = (Player)world.Players[data.Id];
                player.Id = data.Id;
                player.Pos = data.Pos;
                player.Color = data.Color;
                player.Tool = data.Tool;
                if (!world.Players.ContainsKey(data.Id))
                {
                    world._players[data.Id] = player;
                }
                if (world.Initialized && newConnection)
                {
                    PlayerConnected?.Invoke(this, player);
                }
            }
        }
        if (!reader.TryReadLittleEndian(out short pixelCount))
        {
            for (short i = 0; i < pixelCount; i++)
            {
                if (!reader.TryReadPlayer(hasTool: false, out PlayerData data))
                {
                    break;
                }
                //Console.WriteLine($"WorldUpdate pixel: ({x}, {y}) = {r}, {g}, {b}");
            }
        }
        if (reader.TryRead(out byte dcCount))
        {
            for (byte i = 0; i < dcCount; i++)
            {
                if (reader.TryReadLittleEndian(out int id) && world._players.Remove(id, out var player))
                {
                    PlayerDisconnected?.Invoke(this, player);
                }
            }
        }
        TryInitWorld(world);
    }

    private void HandleSetRank(SequenceReader<byte> reader, World world)
    {
        if (reader.TryRead(out byte rank))
        {
            world._clientPlayer.Rank = (PlayerRank)rank;
        }
    }

    private void HandlePixelQuota(SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadBucket(out Bucket bucket))
        {
            world._clientPlayer._pixelBucket.SetValues(bucket.Capacity, bucket.FillTime);
        }
    }

    private void HandleTeleport(SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadPos(out Position pos))
        {
            int s = IWorld.ChunkSize;
            world._clientPlayer.Pos = pos * s + (s / 2, s / 2);
            Teleported?.Invoke(this, new(world, world.ClientPlayer.Pos, world.ClientPlayer.WorldPos));
        }
    }

    private void HandleOpcode(Opcode opcode, SequenceReader<byte> reader, World world)
    {
        world.Logger.LogDebug($"Received opcode: {opcode} ({(byte)opcode})");
        switch (opcode)
        {
            case Opcode.SetId:
                HandleSetId(reader, world);
                break;
            case Opcode.WorldUpdate:
                HandleWorldUpdate(reader, world);
                break;
            case Opcode.SetRank:
                HandleSetRank(reader, world);
                break;
            case Opcode.SetPixelQuota:
                HandlePixelQuota(reader, world);
                break;
            case Opcode.Teleport:
                HandleTeleport(reader, world);
                break;
        }
    }
}
