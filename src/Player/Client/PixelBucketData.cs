using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop;

public class PixelBucketData
{
    public static PixelBucketData Empty => new(0, 0);

    public int Capacity;
    public int Seconds;
    public int Allowance;
    private DateTime _lastUpdate;

    public PixelBucket Bucket;

    public PixelBucketData(int capacity, int seconds)
    {
        SetValues(capacity, seconds);
        Bucket = new PixelBucket(this);
    }

    public void SetValues(int capacity, int seconds)
    {
        Console.WriteLine($"Received PQuota: {capacity} {seconds}");
        Capacity = capacity;
        Seconds = seconds;
        Allowance = capacity;
        _lastUpdate = DateTime.Now;
    }

    public void Update()
    {
        Console.WriteLine(Bucket.SpendRate);
        Console.WriteLine((DateTime.Now - _lastUpdate).TotalSeconds);
        double value = Bucket.SpendRate * (DateTime.Now - _lastUpdate).TotalSeconds;
        Allowance += Math.Min((int) value, Capacity);
        _lastUpdate = DateTime.Now;
    }

    public bool TrySpend(int pixels)
    {
        if (!Bucket.CanSpend(pixels))
        {
            return false;
        }
        Allowance -= pixels;
        return true;
    }
}
