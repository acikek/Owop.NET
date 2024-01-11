using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Util;

// TODO: multiplier
public class BucketData : IBucket
{
    public static BucketData Empty => new(0, 0, false);

    public int Capacity { get; set; }
    public int FillTime { get; set; }
    public double Allowance { get; set; }
    public bool Infinite { get; set; }

    private DateTime _lastUpdate;

    public Bucket Bucket;

    public BucketData(int capacity, int fillTime, bool infinite, bool fill = true)
    {
        SetValues(capacity, fillTime, fill);
        Infinite = infinite;
        Bucket = new Bucket(this);
    }

    public void SetValues(int capacity, int fillTime, bool fill = true)
    {
        Capacity = capacity;
        FillTime = fillTime;
        if (fill)
        {
            Allowance = capacity;
        }
        _lastUpdate = DateTime.Now;
    }

    public void Update()
    {
        Allowance += Bucket.FillRate * (DateTime.Now - _lastUpdate).TotalSeconds;
        Allowance = Math.Min(Capacity, Allowance);
        _lastUpdate = DateTime.Now;
    }

    public TimeSpan GetNextTimeToFill() => Bucket.FillInterval * (1.0 - (Allowance % 1.0));

    public TimeSpan GetTimeToFill(int amount)
    {
        if (Infinite || amount <= 0)
        {
            return TimeSpan.Zero;
        }
        Update();
        return TimeSpan.Zero; /*FillInterval * Math.Min(amount - 1, Capacity)
            + GetNextTimeToFill()
            + SafetyDelay;*/
    }

    public bool TrySpend(int amount)
    {
        if (Infinite)
        {
            return true;
        }
        if (!Bucket.CanSpend(amount))
        {
            return false;
        }
        Allowance -= amount;
        return true;
    }

    public override string ToString() => $"[{Allowance:0.00}/{Capacity}] @{Bucket.FillRate}a/s";
}
