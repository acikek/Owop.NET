using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Owop.Network;

namespace Owop.Client;

public interface IOwopClient : IDisposable
{
    ClientOptions Options { get; }
    ServerInfo? ServerInfo { get; }
    IReadOnlyDictionary<string, IWorldConnection> Connections { get; }
    ILogger Logger { get; }

    Task<ConnectResult> Connect(string world = "main");
    Task<bool> Disconnect(string world = "main");
    Task Destroy();

    event EventHandler<ConnectEventArgs>? Connected;
    event EventHandler<IWorld>? Ready;
    event EventHandler<IWorld>? ChatReady;
    event EventHandler<ChatEventArgs>? Chat;
    event EventHandler<TellEventArgs>? Tell;
    event EventHandler<IPlayer>? PlayerConnected;
    event EventHandler<IPlayer>? PlayerDisconnected;
    event EventHandler<TeleportEventArgs>? Teleported;
    event EventHandler<WhoisEventArgs>? Whois;
    event EventHandler<PixelPlacedEventArgs>? PixelPlaced;
    event EventHandler<IWorld>? Disconnecting;
    event EventHandler? Destroying;

    static IOwopClient Create(ClientOptions? options = null, ILoggerFactory? loggerFactory = null)
        => new OwopClient(options, loggerFactory);
}
