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
    public int Interval; // in seconds
    public int Allowance;
    public bool Infinite;
    private DateTime _lastUpdate;

    public Bucket Bucket;

    public BucketData(int capacity, int interval, bool infinite, bool fill = true)
    {
        SetValues(capacity, interval, fill);
        Infinite = infinite;
        Bucket = new Bucket(this);
    }

    public void SetValues(int capacity, int interval, bool fill = true)
    {
        Capacity = capacity;
        Interval = interval;
        if (fill)
        {
            Allowance = capacity;
        }
        _lastUpdate = DateTime.Now;
    }

    public void Update()
    {
        double value = Bucket.SpendRate * (DateTime.Now - _lastUpdate).TotalSeconds;
        Allowance += Math.Min((int)value, Capacity);
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
}
