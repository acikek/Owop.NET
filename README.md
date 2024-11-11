# Owop.Net

Owop.Net is an asynchronous .NET library for interfacing with [OurWorldOfPixels](https://ourworldofpixels.com/) (OWOP). It follows in the footsteps of projects such as [OWOP.js](https://github.com/dimdenGD/OWOP.js_v2) and is designed for easily creating OWOP bots.

## Example

```cs
using Microsoft.Extensions.Logging;
using Owop.Client;

// Create our client instance
using var client = IOwopClient.Create();

// Fires when our client connects to the world
client.Connected += args =>
{
    args.World.Logger.LogInformation($"Connected as ID {args.World.ClientPlayer.Id}");
};

// Fires when another player uses the /tell command on us
client.Tell += async msg =>
{
    await msg.Player.Tell($"Hello from {msg.World.Name}!");
};

// Connect to a world!
// World name defaults to "main" if none is provided
await client.Connect(options: new ConnectionOptions
{
    Nickname = "MyFirstBot"
});
```
