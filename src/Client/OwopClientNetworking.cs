using System.Buffers;
using System.Drawing;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Owop.Game;
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
                HandleOpcode((Opcode)opcode, ref reader, world);
            }
        }
    }

    private void HandleSetId(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadLittleEndian(out int id))
        {
            world._clientPlayer.Id = id;
            world._players[id] = world.ClientPlayer;
            world.Initialized = false;
        }
    }

    private void InitWorld(World world)
    {
        bool reconnect = world.Connected;
        if (!reconnect)
        {
            world.Connected = true;
        }
        world.Logger.LogDebug("World initialized.");
        world.Initialized = true;
        Connected?.Invoke(new(world, reconnect));
        Task.Run(async () =>
        {
            if (world._connection.Options?.Password is string pass)
            {
                await world.LogIn(pass);
            }
            if (world._connection.Options?.Nickname is string nickname)
            {
                await world.ClientPlayer.SetNickname(nickname);
            }
            if (!reconnect)
            {
                Ready?.Invoke(world);
                await Task.Delay(world.ClientPlayer.ChatBucket.FillInterval);
                world.IsChatReady = true;
                ChatReady?.Invoke(world);
            }
        });
    }

    private void HandleWorldUpdate(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryRead(out byte playerCount))
        {
            for (byte i = 0; i < playerCount; i++)
            {
                if (!reader.TryReadPlayer(hasTool: true, out var data))
                {
                    break;
                }
                if (data.Id == world.ClientPlayer.Id)
                {
                    continue;
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
                    PlayerConnected?.Invoke(player);
                }
            }
        }
        if (reader.TryReadLittleEndian(out short pixelCount) && pixelCount > 0)
        {
            for (short i = 0; i < pixelCount; i++)
            {
                if (!reader.TryReadPlayer(hasTool: false, out var data))
                {
                    break;
                }
                // data.Pos is actually the world pos in this case. TODO: Fix?
                var (chunk, prev) = world._chunks.SetPixel(data.Pos, data.Color);
                if (data.Id == world.ClientPlayer.Id)
                {
                    continue;
                }
                var player = world.Players[data.Id];
                PixelPlacedEventArgs args = new(world, player, data.Color, prev, data.Pos, chunk);
                PixelPlaced?.Invoke(args);
            }
        }
        if (reader.TryRead(out byte dcCount))
        {
            for (byte i = 0; i < dcCount; i++)
            {
                if (reader.TryReadLittleEndian(out int id) && world._players.Remove(id, out var player))
                {
                    PlayerDisconnected?.Invoke(player);
                }
            }
        }
        if (!world.Initialized)
        {
            InitWorld(world);
        }
    }

    private void HandleSetRank(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryRead(out byte rank))
        {
            world._clientPlayer.Rank = (PlayerRank)rank;
        }
    }

    private void HandlePixelQuota(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadBucket(out Bucket bucket))
        {
            world._clientPlayer._pixelBucket.SetValues(bucket.Capacity, bucket.FillTime);
        }
    }

    private void HandleTeleport(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadPos(out Position pos))
        {
            int s = IChunk.Width;
            world._clientPlayer.Pos = pos * s + (s / 2, s / 2);
            Teleported?.Invoke(new(world, world.ClientPlayer.Pos, world.ClientPlayer.WorldPos));
        }
    }

    private void HandleChunkLoad(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadChunk(world, out var chunk) && chunk is IChunk loaded)
        {
            chunk.LastLoad = DateTime.Now;
            chunk.IsLoaded = true;
            if (world._chunks.ChunkQueue.TryRemove(loaded.ChunkPos, out var task))
            {
                task.SetResult(loaded);
            }
            ChunkLoaded?.Invoke(new(world, loaded));
        }
    }

    private void HandleCaptcha(ref SequenceReader<byte> reader, World world)
    {
        if (!reader.TryRead(out byte captchaByte))
        {
            return;
        }
        var state = (CaptchaState)captchaByte;
        world.Logger.LogDebug($"Received captcha state: {state}");
        switch (state)
        {
            case CaptchaState.Waiting:
                // TODO: CaptchaPass in connection options
                break;
            case CaptchaState.Ok:
                // TODO: Connect to world?
                break;
            case CaptchaState.Invalid:
                world.Logger.LogError("Captcha failed; disconnecting...");
                Task.Run(world._connection.Disconnect);
                break;
        }
        // TODO: captcha event
        // TODO: understand when this opcode is sent
    }

    private void HandleChunkProtect(ref SequenceReader<byte> reader, World world)
    {
        if (reader.TryReadChunkMeta(out var chunkPos, out bool isProtected))
        {
            var chunk = world._chunks.GetOrCreate(chunkPos);
            chunk.IsProtected = isProtected;
            ChunkProtectionChanged?.Invoke(new(world, chunk));
        }
    }

    private void HandleOpcode(Opcode opcode, ref SequenceReader<byte> reader, World world)
    {
        world.Logger.LogDebug($"Received opcode: {opcode} ({(byte)opcode})");
        try
        {
            switch (opcode)
            {
                case Opcode.SetId:
                    HandleSetId(ref reader, world);
                    break;
                case Opcode.WorldUpdate:
                    HandleWorldUpdate(ref reader, world);
                    break;
                case Opcode.ChunkLoad:
                    HandleChunkLoad(ref reader, world);
                    break;
                case Opcode.Teleport:
                    HandleTeleport(ref reader, world);
                    break;
                case Opcode.SetRank:
                    HandleSetRank(ref reader, world);
                    break;
                case Opcode.Captcha:
                    HandleCaptcha(ref reader, world);
                    break;
                case Opcode.SetPixelQuota:
                    HandlePixelQuota(ref reader, world);
                    break;
                case Opcode.ChunkProtect:
                    HandleChunkProtect(ref reader, world);
                    break;
            }
        }
        catch (Exception ex)
        {
            world.Logger.LogError(ex, $"Exception while handling opcode '{opcode}':");
        }
    }
}
