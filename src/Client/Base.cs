using Microsoft.Extensions.Logging;

namespace Owop;

public class ClientOptions()
{
    public string Url = "wss://ourworldofpixels.com";
    public short WorldVerification = 25565;
    public int MaxWorldNameLength = 24;
    public int ChunkSize = 16;
    public string ChatVerification = "\u000A";
    public int ChatTimeout = 2000;
}

public partial class OwopClient : IDisposable
{
    public ILogger Logger;

    public readonly ClientOptions Options;
    public readonly Dictionary<string, WorldConnection> Connections = [];

    public OwopClient(ClientOptions? options = null)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger("OWOP.NET");
        Options = options ?? new ClientOptions();
    }

    private string CleanWorldId(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, Options.MaxWorldNameLength)];
        var span = fixedLength.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        return new(span.ToArray());
    }

    public bool Connect(string world = "main")
    {
        string clean = CleanWorldId(world);
        if (Connections.ContainsKey(clean))
        {
            return false;
        }
        WorldConnection connection = new(clean, this, HandleMessage);
        Connections[clean] = connection;
        connection.Connect(clean);
        return true;
    }

    public async Task<bool> Disconnect(string world = "main")
    {
        string clean = CleanWorldId(world);
        if (Connections.Remove(clean, out var connection))
        {
            await connection.Disconnect();
            return true;
        }
        return false;
    }

    public async Task Destroy()
    {
        foreach (var connection in Connections.Values)
        {
            await connection.Disconnect();
        }
    }

    public void Dispose()
    {
        foreach (var connection in Connections.Values)
        {
            connection.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
