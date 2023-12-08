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
    private void HandleMessage(ResponseMessage response)
    {
        if (response.Binary is not null)
        {
            var data = new ReadOnlySequence<byte>(response.Binary);
            var reader = new SequenceReader<byte>(data);
            if (reader.TryRead(out byte opcode))
            {
                HandleOpcode((Opcode)opcode, reader);
            }
        }
        else if (response.Text is not null)
        {
            Logger.LogDebug($"Received chat message: {response.Text}");
            var message = ChatMessage.Parse(response.Text);
            Chat?.Invoke(this, message);
        }
    }

    private void HandleOpcode(Opcode opcode, SequenceReader<byte> reader)
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
                    PlayerData.Id = (uint)id;
                    if (!Connected)
                    {
                        Connected = true;
                        Ready?.Invoke(this, EventArgs.Empty);
                        Task.Run(() =>
                        {
                            Thread.Sleep(CHAT_TIMEOUT);
                            ChatReady?.Invoke(this, EventArgs.Empty);
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
                        WorldPlayerData.TryAdd((uint)id, new());
                        var data = WorldPlayerData[(uint)id];
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
