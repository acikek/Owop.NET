using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop;

public readonly partial struct ClientPlayer
{
    public async Task Move(int x, int y)
    {
        _instance.SetPos(x, y);
        await _instance.WorldData.Connection.SendPlayerData();
    }

    public async Task SetNickname(string nickname)
    {
        _instance.Nickname = nickname;
        await _instance.World.RunCommand("nick", nickname);
    }

    public async readonly Task TeleportToPlayer(int id)
    {
        if (Rank >= PlayerRank.Moderator)
        {
            Console.WriteLine("TeleportToPlayer: using direct method!");
            await _instance.World.RunCommand("tp", id.ToString());
        }
        var player = _instance.World.Players[id];
        await Move(player.Pos.X, player.Pos.Y);
    }
}
