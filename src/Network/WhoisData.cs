using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Network;

public record WhoisData(int PlayerId, int Connections, IPAddress? IPAddress, string? OriginHeader, int WarningLevel, PlayerRank Rank)
{
    public static WhoisData? Parse(List<string> args)
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
        return new(id, connections, ipAddress, originHeader, warningLevel, (PlayerRank)rank);
    }
}