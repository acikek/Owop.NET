using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Network;

// TODO: trim these, buffer these, that stuff. can't do an OOP implementation because of client access
// TODO: these are just notes
public enum ServerMessageType
{
    Info, // "[Server] content" | "Server: content"
    Error, // "Error: content"
    Nickname, // "Nickname set to: 'nick'" | "Nickname reset."
    Chat, // fucking anything
    TellPlayer, // "-> You tell id: content"
    TellClient, // "-> id tells you: content"
    Ids, // "Total: count; id, id, id"
    Whois // "Client information for: ok I just put it in discord
}

public record ServerMessage(ServerMessageType Type, List<string> Args);