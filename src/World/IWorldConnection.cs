using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Websocket.Client;

namespace Owop;

public interface IWorldConnection : IDisposable
{
    WebsocketClient Socket { get; }
    IOwopClient Client { get; }
    IWorld World { get; }
    ILogger Logger { get; }

    Task Send(byte[] message);
    Task Send(string message);
    Task Disconnect();
}
