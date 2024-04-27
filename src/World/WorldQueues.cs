using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <remarks>Handles chat messages and pixel placements.</remarks>
public partial class World
{
    /// <summary>A validation string to append to all outbound chat messages.</summary>
    public const string ChatVerification = "\u000A";

    /// <summary>Sends a chat message.</summary>
    /// <param name="message">The chat message to send.</param>
    public async Task<object?> SendChatMessageInternal(string message)
    {
        _connection.CheckInteraction();
        int length = ClientPlayer.Rank.GetMaxMessageLength();
        string data = message[0..Math.Min(message.Length, length)] + ChatVerification;
        await Connection.Send(data);
        return null;
    }

    public async Task SendChatMessage(string message, bool queue)
    {
        var source = ChatQueue.Add(message);
        await (queue ? Task.CompletedTask : source.Task);
    }

    /// <summary>Finds a world position the cursor needs to move to for a pixel placement.</summary>
    /// <param name="worldPos">The pixel position.</param>
    /// <param name="lazy">Whether to only move the cursor if necessary.</param>
    /// <returns>The found position, or <c>null</c> if no movement is necessary.</returns>
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

    /// <summary>Places a pixel at a world position.</summary>
    /// <param name="obj">The pixel queue entry.</param>
    /// <seealso cref="PlacePixel"/>
    public async Task<Color> PlacePixelInternal((Position? worldPos, Color? color, bool lazy) obj)
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
        return _chunks.SetPixel(pixelPos, pixelColor).Item2;
    }

    public async Task<Color> PlacePixel(Position? worldPos, Color? color, bool lazy, bool queue)
    {
        var source = PixelQueue.Add((worldPos, color, lazy));
        return await (Task<Color>)(queue ? Task.CompletedTask : source.Task);
    }
}
