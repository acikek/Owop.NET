using System.Buffers;
using System.Drawing;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Owop.Network;
using Websocket.Client;

namespace Owop.Client;

public partial class OwopClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HttpClient _httpClient;

    public ServerInfo? ServerInfo { get; private set; }

    public async Task<ServerInfo?> FetchServerInfo()
    {
        var json = await _httpClient.GetStringAsync(Options.ApiUrl);
        var serverInfo = JsonSerializer.Deserialize<ServerInfo>(json, JsonOptions);
        ServerInfo = serverInfo;
        return serverInfo;
    }

    private void HandleMessage(ResponseMessage response, WorldData world)
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

    private void HandleSetId(SequenceReader<byte> reader, WorldData world)
    {
        if (!reader.TryReadLittleEndian(out int id))
        {
            return;
        }
        world.ClientPlayerData.Id = id;
        if (!world.Connected)
        {
            world.Connected = true;
            Ready?.Invoke(this, world);
            Task.Run(() =>
            {
                Thread.Sleep(Options.ChatTimeout);
                ChatReady?.Invoke(this, world);
            });
        }
    }

    private void HandleWorldUpdate(SequenceReader<byte> reader, WorldData world)
    {
        if (reader.TryRead(out byte playerCount))
        {
            for (byte i = 0; i < playerCount; i++)
            {
                if (!OwopProtocol.TryReadPlayer(reader, hasTool: true, out PlayerData data) ||
                    data.Id == world.ClientPlayerData.Id)
                {
                    return;
                }
                bool newConnection = !world.PlayerData.ContainsKey(data.Id);
                if (newConnection)
                {
                    world.PlayerData[data.Id] = PlayerData.Create(world);
                }
                var player = world.PlayerData[data.Id];
                player.Id = data.Id;
                player.Pos = data.Pos;
                player.Color = data.Color;
                player.Tool = data.Tool;
                if (!world.Players.ContainsKey(data.Id))
                {
                    world.Players[data.Id] = player;
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
                if (!OwopProtocol.TryReadPlayer(reader, hasTool: false, out PlayerData data))
                {
                    return;
                }
                //Console.WriteLine($"WorldUpdate pixel: ({x}, {y}) = {r}, {g}, {b}");
            }
        }
        if (reader.TryRead(out byte dcCount))
        {
            for (byte i = 0; i < dcCount; i++)
            {
                if (reader.TryReadLittleEndian(out int id))
                {
                    var player = world.PlayerData[id];
                    PlayerDisconnected?.Invoke(this, player);
                    world.PlayerData.Remove(id);
                    world.Players.Remove(id);
                }
            }
        }
        if (!world.Initialized)
        {
            world.Initialized = true;
        }
    }

    private void HandleOpcode(Opcode opcode, SequenceReader<byte> reader, WorldData world)
    {
        Console.WriteLine($"[OPCODE] {opcode}");
        switch (opcode)
        {
            case Opcode.SetId:
                HandleSetId(reader, world);
                break;
            case Opcode.WorldUpdate:
                HandleWorldUpdate(reader, world);
                break;
        }
    }
}
