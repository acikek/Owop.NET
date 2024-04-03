using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

public partial class World
{
    public const string ChatVerification = "\u000A";

    public async Task SendChatMessageInternal(string message)
    {
        _connection.CheckInteraction();
        int length = ClientPlayer.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + ChatVerification;
        await Connection.Send(data);
    }

    public async Task SendChatMessage(string message, bool queue)
    {
        var source = ChatQueue.Add(message);
        await (queue ? Task.CompletedTask : source.Task);
    }

    public Position? GetPlaceDestination(Position? worldPos, bool lazy)
    {
        if (worldPos is not Position realPos || realPos == ClientPlayer.WorldPos || (lazy && ClientPlayer.Rank == PlayerRank.Admin))
        {
            return null;
        }
        if (!lazy)
        {
            return realPos;
        }
        double dist = (realPos.ToChunkPos() - ClientPlayer.ChunkPos).ToVector().Length();
        return dist >= 4.0 ? realPos : null;
    }

    public async Task PlacePixelInternal((Position? worldPos, Color? color, bool lazy) obj)
    {
        _connection.CheckInteraction(PlayerRank.Player);
        if (GetPlaceDestination(obj.worldPos, obj.lazy) is Position destPos)
        {
            await ClientPlayer.MoveWorld(destPos);
        }
        var pixelPos = obj.worldPos ?? ClientPlayer.WorldPos;
        var pixelColor = obj.color ?? ClientPlayer.Color;
        byte[] pixel = OwopProtocol.EncodePixel(pixelPos, pixelColor);
        await Connection.Send(pixel);
        _chunks.SetPixel(pixelPos, pixelColor);
    }

    public async Task PlacePixel(Position? worldPos, Color? color, bool lazy, bool queue)
    {
        var source = PixelQueue.Add((worldPos, color, lazy));
        await (queue ? Task.CompletedTask : source.Task);
    }
}
