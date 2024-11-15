﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <remarks><see cref="IPlayer"/> method implementations for <see cref="ClientPlayer"/>.</remarks>
public partial class ClientPlayer
{
    /// <inheritdoc/>
    public override async Task Move(Position pos)
    {
        Pos = pos;
        await Send();
    }

    /// <inheritdoc/>
    public override async Task MoveWorld(Position worldPos)
    {
        WorldPos = worldPos;
        await Send();
    }

    /// <inheritdoc/>
    public async Task SetTool(PlayerTool tool)
    {
        Tool = tool;
        await Send();
    }

    /// <inheritdoc/>
    public async Task SetColor(Color color)
    {
        Color = color;
        await Send();
    }

    /// <inheritdoc/>
    public async Task SetNickname(string nickname)
    {
        Nickname = nickname;
        await World.RunCommand("nick", nickname);
    }

    /// <inheritdoc/>
    public async Task ResetNickname()
    {
        Nickname = null;
        await World.RunCommand("nick");
    }

    /// <inheritdoc/>
    public override async Task TeleportTo() => await Task.CompletedTask;

    /// <inheritdoc/>
    public async Task TeleportToPlayer(int id)
    {
        if (Rank >= PlayerRank.Moderator)
        {
            await World.RunCommand("tp", id);
        }
        var player = World.Players[id];
        await Move(player.Pos);
    }

    /// <exception cref="InvalidOperationException">This operation cannot be performed on a <see cref="ClientPlayer"/>.</exception>
    public override Task SetRank(PlayerRank rank) => throw new InvalidOperationException("Cannot set own rank");

    /// <exception cref="InvalidOperationException">This operation cannot be performed on a <see cref="ClientPlayer"/>.</exception>
    public override Task Mute() => throw new InvalidOperationException("Cannot mute self");

    /// <exception cref="InvalidOperationException">This operation cannot be performed on a <see cref="ClientPlayer"/>.</exception>
    public override Task Unmute() => throw new InvalidOperationException("Cannot unmute self");

    /// <inheritdoc/>
    public override Task Kick() => _world.Connection.Disconnect();
}
