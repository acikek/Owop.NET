namespace Owop.Network;

public record BanState(bool Banned, bool Permanent, DateTime? EndDate, long Value)
{
    public static BanState NotBanned => new(false, false, null, 0);
    public static BanState PermanentBan => new(true, true, null, -1);

    public static BanState ForEndTime(long time)
    {
        var offset = DateTimeOffset.FromUnixTimeMilliseconds(time);
        return new(true, false, offset.UtcDateTime, time);
    }

    public static BanState Create(long value)
        => value switch
        {
            0 => NotBanned,
            -1 => PermanentBan,
            _ => ForEndTime(value)
        };
}
