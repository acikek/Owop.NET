using System.Drawing;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

public partial class World
{
    public async Task LogIn(string password) => await RunCommand("pass", password);

    public async Task MovePlayer(int id, Position worldPos)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("tp", id, worldPos.X, worldPos.Y);
    }

    public async Task SetPlayerRank(int id, PlayerRank rank)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("setrank", id, (byte)rank);
    }

    public async Task KickPlayer(int id)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("kick", id);
    }

    public async Task SetPlayerMuted(int id, bool muted)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("mute", id, muted ? 1 : 0);
    }

    public async Task MutePlayer(int id) => await SetPlayerMuted(id, true);

    public async Task UnmutePlayer(int id) => await SetPlayerMuted(id, false);

    public async Task<WhoisData> QueryWhois(int playerId)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        if (WhoisQueue.TryGetValue(playerId, out var existing))
        {
            return await existing.Task;
        }
        var source = new TaskCompletionSource<WhoisData>(TaskCreationOptions.RunContinuationsAsynchronously);
        WhoisQueue[playerId] = source;
        await RunCommand("whois", playerId);
        return await source.Task;
    }

    public async Task SetPassword(string password)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("setworldpass", password);
    }

    public async Task RemovePassword() => await SetPassword("remove");

    public async Task SetRestricted(bool restricted)
    {
        _connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("restrict", restricted);
    }

    public async Task Restrict() => await SetRestricted(true);

    public async Task Unrestrict() => await SetRestricted(false);

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

    public async Task<bool> PlacePixel(Position? worldPos, Color? color, bool lazy)
    {
        _connection.CheckInteraction(PlayerRank.Player);
        if (!_clientPlayer._pixelBucket.TrySpend(1.0))
        {
            return false;
        }
        if (GetPlaceDestination(worldPos, lazy) is Position destPos)
        {
            await ClientPlayer.MoveWorld(destPos);
        }
        var pixelPos = worldPos ?? ClientPlayer.WorldPos;
        var pixelColor = color ?? ClientPlayer.Color;
        byte[] pixel = OwopProtocol.EncodePixel(pixelPos, pixelColor);
        await Connection.Send(pixel);
        _chunks.SetPixel(pixelPos, pixelColor);
        return true;
    }

    public async Task Disconnect() => await Connection.Disconnect();
}
