using System.Buffers;
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
            ChatEvent?.Invoke(this, message);
        }
    }

    private void HandleOpcode(Opcode opcode, SequenceReader<byte> reader)
    {
        Console.WriteLine(opcode);
        switch (opcode)
        {
            case Opcode.SetId:
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
                        Thread.Sleep(2000);
                        ChatReady?.Invoke(this, EventArgs.Empty);
                    });
                }
                break;
        }
    }
}
