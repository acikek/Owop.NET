using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Websocket.Client;

namespace Owop.Client;

/// <summary>Represents a client connection to an <see cref="IWorld"/>.</summary>
public interface IWorldConnection : IDisposable
{
    /// <summary>The underlying websocket connection.</summary>
    WebsocketClient Socket { get; }

    /// <summary>The attached client.</summary>
    IOwopClient Client { get; }

    /// <summary>The connection options.</summary>
    ConnectionOptions? Options { get; }

    /// <summary>The connection's world.</summary>
    IWorld World { get; }

    /// <summary>The connection's distinct logger.</summary>
    ILogger Logger { get; }

    /// <summary>Sends a binary message via the websocket.</summary>
    /// <param name="message">The message data.</param>
    Task Send(byte[] message);

    /// <summary>Sends a string message via the websocket.</summary>
    /// <param name="message">The message content.</param>
    Task Send(string message);

    /// <summary>Disconnects from the corresponding world.</summary>
    Task Disconnect();
}
