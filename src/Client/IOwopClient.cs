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
    ServerInfo? ServerInfo { get; }
    IReadOnlyDictionary<string, IWorldConnection> Connections { get; }
    ILogger Logger { get; }

    Task<ConnectResult> Connect(string world = "main", ConnectionOptions? options = null);
    Task<bool> Disconnect(string world = "main");
    Task Destroy();

    event Action<ConnectEventArgs>? Connected;
    event Action<IWorld>? Ready;
    event Action<IWorld>? ChatReady;
    event Action<ChatEventArgs>? Chat;
    event Action<TellEventArgs>? Tell;
    event Action<IPlayer>? PlayerConnected;
    event Action<IPlayer>? PlayerDisconnected;
    event Action<TeleportEventArgs>? Teleported;
    event Action<WhoisEventArgs>? Whois;
    event Action<PixelPlacedEventArgs>? PixelPlaced;
    event Action<ChunkLoadedEventArgs>? ChunkLoaded;
    event Action<IWorld>? Disconnecting;
    event Action? Destroying;

    static IOwopClient Create(ClientOptions? options = null, ILoggerFactory? loggerFactory = null)
        => new OwopClient(options, loggerFactory);
}
