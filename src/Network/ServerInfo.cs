using System.Text.Json;
using System.Text.Json.Serialization;

namespace Owop.Network;

class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TimeSpan.FromMilliseconds(reader.GetDouble());
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) => writer.WriteNumberValue(value.TotalMilliseconds);
}

class BanStateConverter : JsonConverter<BanState>
{
    public override BanState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => BanState.Create(reader.GetInt64());
    public override void Write(Utf8JsonWriter writer, BanState value, JsonSerializerOptions options) => writer.WriteNumberValue(value.Value);
}

public record class ServerInfo
{
    [JsonConverter(typeof(BanStateConverter))]
    [JsonPropertyName("banned")]
    [JsonInclude]
    public BanState? BanState { get; internal set; }

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
