using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owop.Client;
using Owop.Util;

namespace Owop;

/// <summary>Represents a player controlled by an <see cref="OwopClient"/>.</summary>
public interface IClientPlayer : IPlayer
{
    /// <summary>The player's nickname.</summary>
    string? Nickname { get; }

    /// <summary>The player's rank.</summary>
    PlayerRank Rank { get; }

    /// <summary>The player's spendable pixel bucket.</summary>
    IBucket PixelBucket { get; }

    /// <summary>The player's spendable chat bucket.</summary>
    IBucket ChatBucket { get; }

    /// <summary>Sets the player's selected tool.</summary>
    /// <param name="tool">The player's new tool.</param>
    Task SetTool(PlayerTool tool);

    /// <summary>Sets the player's selected color.</summary>
    /// <param name="color">The player's new color.</param>
    Task SetColor(Color color);

    /// <summary>Sets the player's nickname.</summary>
    /// <param name="nickname">The player's new nickname.</param>
    Task SetNickname(string nickname);

    /// <summary>Resets the player's nickname (to nothing).</summary>
    Task ResetNickname();

    /// <summary>Teleports this player to another player within the world.</summary>
    /// <param name="id">The target player ID.</param>
    Task TeleportToPlayer(int id);
}
