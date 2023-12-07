using System.Buffers;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Websocket.Client;

namespace Owop;

public partial class Client
{
    public event EventHandler? ReadyEvent;
    public event EventHandler? ChatReadyEvent;
    public event EventHandler<ChatMessage>? ChatEvent;
}
