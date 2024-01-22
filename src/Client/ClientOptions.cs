namespace Owop.Client;

public class ClientOptions()
{
    public string ApiUrl = "https://ourworldofpixels.com/api";
    public string SocketUrl = "wss://ourworldofpixels.com";
    // TODO: these should probably be constants instead
    public short WorldVerification = 25565;
    public int MaxWorldNameLength = 24;
    public string ChatVerification = "\u000A";
}
