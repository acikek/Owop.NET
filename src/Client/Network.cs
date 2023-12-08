using System.Buffers;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace Owop;

public enum Opcode
{
    SetId,
    WorldUpdate,
    ChunkLoad,
    Teleport,
    SetRank,
    Captcha,
    SetPixelQuota,
    ChunkProtect,
    MaxPlayerCount,
    DonationTimer
}

public partial class OwopClient
{
    private void HandleMessage(ResponseMessage response, WorldData world)
    {
        if (response.Binary is not null)
        {
            var data = new ReadOnlySequence<byte>(response.Binary);
            var reader = new SequenceReader<byte>(data);
            if (reader.TryRead(out byte opcode))
            {
                HandleOpcode((Opcode)opcode, reader, world);
            }
        }
        else if (response.Text is not null)
        {
            Logger.LogDebug($"Received chat message: {response.Text}");
            var message = ChatMessage.Create(world, response.Text);
            Chat?.Invoke(this, message);
        }
    }

    private void HandleOpcode(Opcode opcode, SequenceReader<byte> reader, WorldData world)
    {
        Console.WriteLine(opcode);
        switch (opcode)
        {
            case Opcode.SetId:
                {
                    if (!reader.TryReadLittleEndian(out int id))
                    {
                        return;
                    }
                    world.ClientPlayerData.Id = (uint)id;
                    if (!world.Connected)
                    {
                        world.Connected = true;
                        Ready?.Invoke(this, world);
                        Task.Run(() =>
                        {
                            Thread.Sleep(World.CHAT_TIMEOUT);
                            ChatReady?.Invoke(this, world);
                        });
                    }
                    break;
                }
            case Opcode.WorldUpdate:
                {
                    if (!reader.TryRead(out byte count))
                    {
                        return;
                    }
                    List<uint> updated = [];
                    for (byte i = 0; i < count; i++)
                    {
                        if (!reader.TryReadLittleEndian(out int id) ||
                            !reader.TryReadLittleEndian(out int x) ||
                            !reader.TryReadLittleEndian(out int y) ||
                            !reader.TryRead(out byte r) ||
                            !reader.TryRead(out byte g) ||
                            !reader.TryRead(out byte b) ||
                            !reader.TryRead(out byte toolId))
                        {
                            return;
                        }
                        world.PlayerData.TryAdd((uint)id, new(world));
                        var data = world.PlayerData[(uint)id];
                        data.Id = (uint)id;
                        data.X = x;
                        data.Y = y;
                        data.WorldX = x / 16;
                        data.WorldY = y / 16;
                        data.Color = Color.FromArgb(255, r, g, b);
                        data.Tool = (PlayerTool)toolId;
                        updated.Add((uint)id);
                    }

                    break;
                }
        }
    }
}
