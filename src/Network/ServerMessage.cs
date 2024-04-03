using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Network;

/// <summary>Enumeration of message types received from the server.</summary>
public enum ServerMessageType
{
    /// <summary>Generic server message.</summary>
    Info,
    /// <summary>Error message.</summary>
    Error,
    /// <summary>Nickname was updated or reset.</summary>
    Nickname,
    /// <summary>Player chat message.</summary>
    Chat,
    /// <summary>Client sent a message to another player.</summary>
    TellPlayer,
    /// <summary>Another player sent a message to the client.</summary>
    TellClient,
    /// <summary>Response to the '/ids' command.</summary>
    Ids,
    /// <summary>Response to the `/whois` command.</summary>
    Whois // "Client information for: ok I just put it in discord
}

/// <summary>Represents a plaintext message received from the server.</summary>
/// <param name="Type">The type of the message.</param>
/// <param name="Args">The 'arguments' of the message, semantically parsed according to the type.</param>
public record ServerMessage(ServerMessageType Type, List<string> Args);
