namespace Owop.Util;

public class Bucket(BucketData data)
{
    public static Bucket Empty => BucketData.Empty;

    private readonly BucketData _instance = data;

    public int Capacity => _instance.Capacity;
    public int Interval => _instance.Interval;
    public int Allowance => _instance.Allowance;
    public bool Infinite => _instance.Infinite;
    public double SpendRate => (double)Capacity / Interval;
    public bool Full => Allowance >= Capacity;

    public bool CanSpend(int pixels)
    {
        _instance.Update();
        return Infinite || pixels <= Allowance;
    }

    public TimeSpan GetTimeToFill(int amount) => Infinite || amount <= 0 ? TimeSpan.Zero : (TimeSpan.FromSeconds(amount / SpendRate) + TimeSpan.FromMilliseconds(100));

    public async Task DelayUntilFill(int amount) => await (Infinite ? Task.CompletedTask : Task.Delay(GetTimeToFill(amount)));

    public async Task DelayUntilHas(int allowance) => await DelayUntilFill(allowance - Allowance);

    public async Task DelayUntilRestore() => await DelayUntilFill(Capacity - Allowance);

    public async Task DelayOne() => await DelayUntilFill(1);

    public async Task DelayUntilOne() => await DelayUntilHas(1);

    public static implicit operator Bucket(BucketData data) => data.Bucket;
}
