using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Network;
using Owop.Util;

namespace Owop;

public partial class ClientPlayer
{
    public async Task Move(Position pos)
    {
        _instance.Pos = pos;
        await Send();
    }

    public async Task MoveWorld(Position worldPos)
    {
        _instance.WorldPos = worldPos;
        await Send();
    }

    public async Task SetTool(PlayerTool tool)
    {
        _instance.Tool = tool;
        await Send();
    }

    public async Task SetColor(Color color)
    {
        _instance.Color = color;
        await Send();
    }

    public async Task SetColor(int r, int g, int b) => await SetColor(Color.FromArgb(r, g, b));

    public async Task SetNickname(string nickname)
    {
        _instance.Nickname = nickname;
        await _instance.World.RunCommand("nick", nickname);
    }

    public async Task ResetNickname() => await _instance.World.RunCommand("nick");

    public async Task TeleportTo() => await Task.CompletedTask;

    public async Task TeleportToPlayer(int id)
    {
        if (Rank >= PlayerRank.Moderator)
        {
            Console.WriteLine("TeleportToPlayer: using direct method!");
            await _instance.World.RunCommand("tp", id.ToString());
        }
        var player = _instance.World.Players[id];
        await Move(player.Pos);
    }

    public async Task Tell(string message) => await World.TellPlayer(Id, message);

    public async Task<WhoisData> QueryWhois() => await World.QueryWhois(Id);
}
