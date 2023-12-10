using System.Buffers;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Protocol;
using Websocket.Client;

namespace Owop.Client;

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
                    if (reader.TryRead(out byte playerCount))
                    {
                        for (byte i = 0; i < playerCount; i++)
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
                            bool newConnection = world.PlayerData.TryAdd(id, new(world));
                            var data = world.PlayerData[id];
                            data.Id = id;
                            data.UpdatePos(x, y, Options.ChunkSize);
                            data.Color = Color.FromArgb(255, r, g, b);
                            data.Tool = (PlayerTool)toolId;
                            if (newConnection)
                            {
                                PlayerConnected?.Invoke(this, data);
                            }
                        }
                    }
                    if (!reader.TryReadLittleEndian(out short pixelCount))
                    {
                        for (short i = 0; i < pixelCount; i++)
                        {
                            // TODO extract into method
                            if (!reader.TryReadLittleEndian(out int id) ||
                                !reader.TryReadLittleEndian(out int x) ||
                                !reader.TryReadLittleEndian(out int y) ||
                                !reader.TryRead(out byte r) ||
                                !reader.TryRead(out byte g) ||
                                !reader.TryRead(out byte b))
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
                            }
                        }
                    }
                    break;
                }
        }
    }
}
