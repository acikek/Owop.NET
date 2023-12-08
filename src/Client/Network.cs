using System.Buffers;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace Owop;

/// <summary>OWOP Websocket protocol opcode.</summary>
public enum Opcode
{
    /// <summary>Sets the client player's ID.</summary>
    SetId,
    /// <summary>Updates players, pixels, and disconnects within a world.</summary>
    WorldUpdate,
    /// <summary>Loads a world chunk.</summary>
    ChunkLoad,
    /// <summary>Teleports the client player.</summary>
    Teleport,
    /// <summary>Sets the client player's <see cref="PlayerRank"/>.</summary>
    SetRank,
    /// <summary>Updates captcha status.</summary>
    Captcha,
    /// <summary>Sets the client player's ...something. TODO: update this when I understand it better</summary>
    SetPixelQuota,
    /// <summary>Protects a chunk within a world.</summary>
    ChunkProtect,
    /// <summary>Sets the maximum amount of players that can connect to a world.</summary>
    MaxPlayerCount,
    /// <summary>Updates the duration of donation boost remaining.</summary>
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
            var message = ChatEventArgs.Create(world, response.Text);
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
                        world.PlayerData.TryAdd(id, new(world));
                        var data = world.PlayerData[id];
                        data.Id = id;
                        data.UpdatePos(x, y, Options.ChunkSize);
                        data.Color = Color.FromArgb(255, r, g, b);
                        data.Tool = (PlayerTool)toolId;
                        updated.Add((uint)id);
                    }

                    break;
                }
        }
    }
}
