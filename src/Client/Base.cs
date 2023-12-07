using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Websocket.Client;

namespace Owop;

public partial class Client
{
    public ILogger Logger;

    private PlayerData PlayerData = PlayerData.Empty;
    public Player Player => PlayerData;

    public string? Nickname { get; private set; }
    public World? World { get; private set; }

    public Client()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger("OWOP.NET");
        Connection = new WebsocketClient(new Uri(URL));
    }
}
