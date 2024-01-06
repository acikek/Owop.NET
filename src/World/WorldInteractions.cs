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

    public async Task PlacePixel(Position? worldPos = null, Color? color = null, bool sneaky = false)
    {
        _instance.Connection.CheckRank(PlayerRank.Player);
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
    }
}
