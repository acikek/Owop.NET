using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <remarks>Handles world interaction implementations.</remarks>
public partial class World
{
    public async Task RunCommand(string command, params object[] args)
    {
        string message = $"/{command} {string.Join(' ', args)}";
        await SendChatMessage(message, false);
    }

    public async Task TellPlayer(int id, string message)
        => await RunCommand("tell", id, message);

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

    public async Task Disconnect() => await Connection.Disconnect();
}
