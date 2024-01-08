using System.Drawing;
using Owop.Network;
using Owop.Util;

namespace Owop;

public partial class World
{
    public async Task Login(string password)
    {
        await RunCommand("pass", password);
    }

    public async Task MovePlayer(int id, Position worldPos)
    {
        _instance.Connection.CheckRank(PlayerRank.Moderator);
        await RunCommand("tp", id.ToString(), worldPos.X.ToString(), worldPos.Y.ToString());
    }

    public async Task SetRestricted(bool restricted)
    {
        _instance.Connection.CheckRank(PlayerRank.Moderator);
        await RunCommand("restrict", restricted.ToString());
    }

    public async Task Restrict() => await SetRestricted(true);

    public async Task Unrestrict() => await SetRestricted(false);

    public async Task<bool> PlacePixel(Position? worldPos = null, Color? color = null, bool sneaky = false)
    {
        if (!_instance.Connection.Socket.IsRunning)
        {
            return false;
        }
        _instance.Connection.CheckRank(PlayerRank.Player);
        if (!_instance.ClientPlayerData.BucketData.TrySpend(1))
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

    public async Task<WhoisData> QueryWhois(int playerId)
    {
        if (_instance.WhoisQueue.TryGetValue(playerId, out var existing))
        {
            return await existing.Task;
        }
        var source = new TaskCompletionSource<WhoisData>(TaskCreationOptions.RunContinuationsAsynchronously);
        _instance.WhoisQueue[playerId] = source;
        await RunCommand("whois", playerId.ToString());
        return await source.Task;
    }
}
