namespace Owop.Client;

using Owop.Network;

/// <summary>Represents individual options for an individual <see cref="IOwopClient"/>.</summary>
public record ClientOptions()
{
    /// <summary>The URL to fetch <see cref="ServerInfo"/> from.</summary>
    public string ApiUrl = "https://ourworldofpixels.com/api";

    /// <summary>The websocket URL to connect to.</summary>
    public string SocketUrl = "wss://ourworldofpixels.com";
}
