namespace Owop;

public class PixelBucket(PixelBucketData data)
{
    public static PixelBucket Empty => PixelBucketData.Empty;

    private readonly PixelBucketData _instance = data;

    public int Capacity => _instance.Capacity;
    public int Seconds => _instance.Seconds;
    public int Allowance => _instance.Allowance;
    public double SpendRate => (double) Capacity / Seconds;

    public bool CanSpend(int pixels)
    {
        _instance.Update();
        Console.WriteLine($"CanSpend pixels: {pixels} Allowance: {Allowance}");
        return pixels <= Allowance;
    }

    public TimeSpan GetTimeToRestore()
        => Allowance >= Capacity
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((Capacity - Allowance) / SpendRate);

    public async Task DelayUntilRestore()
        => await Task.Delay(GetTimeToRestore());

    public static implicit operator PixelBucket(PixelBucketData data) => data.Bucket;
}
