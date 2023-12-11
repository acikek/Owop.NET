using System.Text.Json;
using System.Text.Json.Serialization;

namespace Owop.Network;

class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TimeSpan.FromMilliseconds(reader.GetDouble());
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) => writer.WriteNumberValue(value.TotalMilliseconds);
}

public record class ServerInfo
{
    [JsonPropertyName("banned")]
    [JsonInclude]
    // TODO: proper ban state class
    public int BanState { get; internal set; }

    [JsonInclude]
    public bool CaptchaEnabled { get; internal set; }

    [JsonInclude]
    public int MaxConnectionsPerIp { get; internal set; }

    [JsonPropertyName("motd")]
    [JsonInclude]
    public string MessageOfTheDay { get; internal set; } = string.Empty;

    [JsonInclude]
    public int TotalConnections { get; internal set; }

    [JsonConverter(typeof(TimeSpanConverter))]
    [JsonInclude]
    public TimeSpan Uptime { get; internal set; }

    [JsonPropertyName("users")]
    [JsonInclude]
    public int PlayerCount { get; internal set; }

    [JsonPropertyName("yourConns")]
    [JsonInclude]
    public int ClientConnections { get; internal set; }
}
