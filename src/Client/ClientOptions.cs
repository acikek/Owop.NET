namespace Owop.Client;

public class ClientOptions()
{
    public string Url = "wss://ourworldofpixels.com";
    public short WorldVerification = 25565;
    public int MaxWorldNameLength = 24;
    public string ChatVerification = "\u000A";
    public int ChatTimeout = 2000;
}
