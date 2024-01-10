using System.Drawing;
using Owop.Network;
using Owop.Util;

namespace Owop;

public partial class World
{
    public async Task LogIn(string password) => await RunCommand("pass", password);

    public async Task MovePlayer(int id, Position worldPos)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("tp", id, worldPos.X, worldPos.Y);
    }

    public async Task SetPlayerRank(int id, PlayerRank rank)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("setrank", id, (byte)rank);
    }

    public async Task KickPlayer(int id)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("kick", id);
    }

    // TODO: public SetPlayerMuted
    private async Task SetPlayerMuteState(int id, int state)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("mute", id, state);
    }

    public async Task MutePlayer(int id) => await SetPlayerMuteState(id, 1);

    public async Task UnmutePlayer(int id) => await SetPlayerMuteState(id, 0);

    public async Task<WhoisData> QueryWhois(int playerId)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        if (_instance.WhoisQueue.TryGetValue(playerId, out var existing))
        {
            return await existing.Task;
        }
        var source = new TaskCompletionSource<WhoisData>(TaskCreationOptions.RunContinuationsAsynchronously);
        _instance.WhoisQueue[playerId] = source;
        await RunCommand("whois", playerId);
        return await source.Task;
    }

    public async Task SetPassword(string password)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("setworldpass", password);
    }

    public async Task RemovePassword() => await SetPassword("remove");

    public async Task SetRestricted(bool restricted)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Moderator);
        await RunCommand("restrict", restricted);
    }

    public async Task Restrict() => await SetRestricted(true);

    public async Task Unrestrict() => await SetRestricted(false);

    public async Task<bool> PlacePixel(Position? worldPos = null, Color? color = null, bool sneaky = false)
    {
        _instance.Connection.CheckInteraction(PlayerRank.Player);
        if (!_instance.ClientPlayerData.PixelBucketData.TrySpend(1))
        {
            return false;
        }
        bool update = worldPos is not null || color is not null;
        var prevPos = _instance.ClientPlayerData.Pos;
        if (worldPos is Position realPos)
        {
            _instance.ClientPlayerData.WorldPos = realPos;
        }
        else
        {
            worldPos = _instance.ClientPlayerData.WorldPos;
        }
        if (color is Color realColor)
        {
            _instance.ClientPlayerData.Color = realColor;
        }
        else
        {
            color = _instance.ClientPlayerData.Color;
        }
        if (update)
        {
            await _instance.Connection.SendPlayerData();
        }
        byte[] pixel = OwopProtocol.EncodePixel((Position) worldPos, (Color) color);
        await _instance.Connection.Send(pixel);
        if (sneaky)
        {
            await _instance.ClientPlayerData.Player.Move(prevPos);
        }
        return true;
    }

    public async Task Disconnect() => await _instance.Connection.Disconnect();
}
