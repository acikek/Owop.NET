using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Owop.Game;

namespace Owop.Network;

/// <summary>Data queried from a '/whois' command.</summary>
/// <param name="Player">The queried player.</param>
/// <param name="Connections">The number of connections from the player's IP.</param>
/// <param name="IPAddress">The player's IP. Visible to site moderators.</param>
/// <param name="OriginHeader">The origin request header, if any.</param>
/// <param name="WarningLevel">The player's warning level.</param>
/// <param name="PlayerRank">The player's rank.</param>
public record WhoisData(IPlayer Player, int Connections, IPAddress? IPAddress, string? OriginHeader, int WarningLevel, PlayerRank PlayerRank)
{
    /// <summary>Parses <see cref="ServerMessageType.Whois"/> message arguments into a <see cref="WhoisData"/>instance.</summary>
    /// <param name="args">The message arguments.</param>
    /// <param name="world">The world the message was received in.</param>
    /// <returns>The resulting data.</returns>
    public static WhoisData? Parse(List<string> args, IWorld world)
    {
        if (!int.TryParse(args[0], out int id))
        {
            return null;
        }
        var values = args.Skip(1)
            .Select(str =>
            {
                string[] pair = str.Split(": ");
                return (pair[0], pair[1]);
            })
            .ToDictionary();
        if (!int.TryParse(values["Connections by this IP"], out int connections) ||
            !int.TryParse(values["Warning level"], out int warningLevel) ||
            !byte.TryParse(values["Rank"], out byte rank))
        {
            return null;
        }
        string? originHeader = values["Origin header"];
        originHeader = originHeader == "(None)" ? null : originHeader;
        var ipAddress = values.TryGetValue("IP", out string? ipStr)
            ? (IPAddress.TryParse(ipStr, out IPAddress? ip) ? ip : null)
            : null;
        return new(world.Players[id], connections, ipAddress, originHeader, warningLevel, (PlayerRank)rank);
    }
}
