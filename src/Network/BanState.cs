namespace Owop.Network;

/// <summary>Represents the ban state of a client IP.</summary>
/// <param name="Banned">Whether the client is banned.</param>
/// <param name="Permanent">Whether the ban, if any, is permanent.</param>
/// <param name="EndDate">The end date of the ban, if temporary.</param>
/// <param name="Value">The originally parsed ban value.</param>
public record BanState(bool Banned, bool Permanent, DateTime? EndDate, long Value)
{
    /// <summary>An unbanned state.</summary>
    public static BanState NotBanned => new(false, false, null, 0);

    /// <summary>A permanently banned state.</summary>
    public static BanState PermanentBan => new(true, true, null, -1);

    /// <summary>Creates a temporarily-banned state for an end time.</summary>
    /// <param name="time">A unix-based end time.</param>
    /// <returns>The resulting ban state.</returns>
    public static BanState ForEndTime(long time)
    {
        var offset = DateTimeOffset.FromUnixTimeMilliseconds(time);
        return new(true, false, offset.UtcDateTime, time);
    }

    /// <summary>Creates a ban state from a serialized value.</summary>
    /// <param name="value">The ban value.</param>
    /// <returns>The resulting ban state.</returns>
    public static BanState Create(long value)
        => value switch
        {
            0 => NotBanned,
            -1 => PermanentBan,
            _ => ForEndTime(value)
        };
}
