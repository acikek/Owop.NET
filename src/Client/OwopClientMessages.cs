using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Owop.Network;

namespace Owop.Client;

/// <summary>Handles messages from the server.</summary>
public partial class OwopClient
{
    /// <summary>A regex pattern to match <see cref="ServerMessageType.TellClient"/> messages.</summary>
    [GeneratedRegex(@"-> (\d+) tells you: (.*)")]
    private static partial Regex TellClientPattern();

    /// <summary>Whether a whois message is currently being built.</summary>
    private bool _whois = false;

    /// <summary>A buffer to use when constructing multiline messages.</summary>
    private readonly List<string> _messageBuffer;

    /// <summary>Handles a string message sent from the server.</summary>
    /// <param name="text">The message content.</param>
    /// <param name="world">The world the string message was sent from.</param>
    private void HandleTextMessage(string text, World world)
    {
        try
        {
            if (HandleTextLine(text) is ServerMessage message)
            {
                HandleServerMessage(message, world);
            }
        }
        catch (Exception ex)
        {
            world.Logger.LogError(ex, $"Exception while handling text message '{text}':");
        }
    }

    /// <summary>Attempts to parse and construct a <see cref="ServerMessage"/> based on a prefix.</summary>
    /// <param name="type">The server message type.</param>
    /// <param name="prefix">The corresponding type prefix.</param>
    /// <param name="text">The message content.</param>
    /// <returns>The constructed message if the prefix was present, otherwise <c>null</c>.</returns>
    private static ServerMessage? PrefixedTextLine(ServerMessageType type, string prefix, string text)
    {
        int index = text.IndexOf(prefix, StringComparison.Ordinal);
        return index == 0 ? new(type, [text, text[prefix.Length..]]) : null;
    }

    /// <summary>Handles a line of text sent from the server.</summary>
    /// <param name="text">The text line.</param>
    /// <returns>The text line's corresponding server message, if any.</returns>
    private ServerMessage? HandleTextLine(string text)
    {
        string whoisHeader = "Client information for: ";
        if (text.StartsWith(whoisHeader))
        {
            _whois = true;
            _messageBuffer.Add(text[whoisHeader.Length..]);
            return null;
        }
        if (_whois)
        {
            _messageBuffer.Add(text[3..]);
            if (text.StartsWith("-> Rank"))
            {
                ServerMessage message = new(ServerMessageType.Whois, new List<string>(_messageBuffer));
                _whois = false;
                _messageBuffer.Clear();
                return message;
            }
            return null;
        }
        var prefixed = PrefixedTextLine(ServerMessageType.Info, "[Server] ", text)
            ?? PrefixedTextLine(ServerMessageType.Info, "Server: ", text)
            ?? PrefixedTextLine(ServerMessageType.Error, "Error: ", text)
            ?? PrefixedTextLine(ServerMessageType.Nickname, "Nickname ", text)
            ?? PrefixedTextLine(ServerMessageType.TellPlayer, "-> You tell ", text)
            ?? PrefixedTextLine(ServerMessageType.Ids, "Total: ", text);
        if (prefixed is not null)
        {
            return prefixed;
        }
        if (text.StartsWith('(') || text.StartsWith('[') || int.TryParse(text[0].ToString(), out int _))
        {
            return new(ServerMessageType.Chat, [text]);
        }
        var tellClient = TellClientPattern().Matches(text);
        if (tellClient.Count > 0)
        {
            var args = tellClient[0].Groups.Cast<Group>().Select(g => g.Value).ToList();
            return new(ServerMessageType.TellClient, args);
        }
        return new(ServerMessageType.Info, [text, text]);
    }

    /// <summary>Handles a constructed server message.</summary>
    /// <param name="message">The server message object.</param>
    /// <param name="world">The world the message was sent from.</param>
    private void HandleServerMessage(ServerMessage message, World world)
    {
        world.Logger.LogDebug($"Received server message ({message.Type}): '{message.Args[0]}'");
        ServerMessage?.Invoke(message);
        switch (message.Type)
        {
            case ServerMessageType.Chat:
                InvokeChat(message, world);
                break;
            case ServerMessageType.TellClient:
                InvokeTell(message, world);
                break;
            case ServerMessageType.Whois:
                InvokeWhois(message, world);
                break;
            case ServerMessageType.Info:
                if (message.Args[1].StartsWith("This world has a password set"))
                {
                    world.IsPasswordProtected = true;
                }
                break;
        }
    }
}
