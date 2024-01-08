namespace Owop.Util;

public class Bucket(BucketData data)
{
    public static Bucket Empty => BucketData.Empty;

    private readonly BucketData _instance = data;

    public int Capacity => _instance.Capacity;
    public int Seconds => _instance.Seconds;
    public int Allowance => _instance.Allowance;
    public bool Infinite => _instance.Infinite;
    public double SpendRate => (double)Capacity / Seconds;

    public bool CanSpend(int pixels)
    {
        _instance.Update();
        return Infinite || pixels <= Allowance;
    }

    public TimeSpan GetTimeToRestore()
        => Infinite || Allowance >= Capacity
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((Capacity - Allowance) / SpendRate);

    public async Task DelayUntilRestore()
        => await (Infinite ? Task.CompletedTask : Task.Delay(GetTimeToRestore()));

    public static implicit operator Bucket(BucketData data) => data.Bucket;
}
