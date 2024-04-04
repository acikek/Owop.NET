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

/// <summary>Contains the fetched values from the public OWOP server API.</summary>
public record class ServerInfo
{
    /// <summary>The ban state of the client.</summary>
    [JsonConverter(typeof(BanStateConverter))]
    [JsonPropertyName("banned")]
    [JsonInclude]
    public BanState? BanState { get; internal set; }

    /// <summary>Whether the server requires a Captcha solution to connect to a world.</summary>
    [JsonInclude]
    public bool CaptchaEnabled { get; internal set; }

    /// <summary>The maximum number of connections a client can create.</summary>
    [JsonInclude]
    public int MaxConnectionsPerIp { get; internal set; }

    /// <summary>The server's message of the day.</summary>
    [JsonPropertyName("motd")]
    [JsonInclude]
    public string MessageOfTheDay { get; internal set; } = string.Empty;

    /// <summary>The total number of player connectons created during the server's uptime.</summary>
    [JsonInclude]
    public int TotalConnections { get; internal set; }

    /// <summary>The current server uptime.</summary>
    [JsonConverter(typeof(TimeSpanConverter))]
    [JsonInclude]
    public TimeSpan Uptime { get; internal set; }

    /// <summary>The number of player connections to the entire OWOP server.</summary>
    [JsonPropertyName("users")]
    [JsonInclude]
    public int PlayerCount { get; internal set; }

    /// <summary>The number of connections created by the client IP.</summary>
    [JsonPropertyName("yourConns")]
    [JsonInclude]
    public int ClientConnections { get; internal set; }
}
