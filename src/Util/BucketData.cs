using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Util;

// TODO: multiplier
public class BucketData
{
    public static BucketData Empty => new(0, 0, false);

    public int Capacity;
    public int FillTime; // in seconds
    public int Allowance;
    public bool Infinite;

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

    //public TimeSpan GetTimeUntilNextFill() => Bucket.FillInterval - (DateTime.Now - _lastFilled);

    public void Update()
    {
        double value = Bucket.FillRate * (DateTime.Now - _lastUpdate).TotalSeconds;
        Allowance += (int)value;
        Allowance = Math.Min(Capacity, Allowance);
        _lastUpdate = DateTime.Now;
    }


    public bool TrySpend(int pixels)
    {
        if (Infinite)
        {
            return true;
        }
        if (!Bucket.CanSpend(pixels))
        {
            return false;
        }
        Allowance -= pixels;
        return true;
    }

    public override string ToString() => $"[{Allowance}/{Capacity}] @{Bucket.FillRate}a/s";
}
