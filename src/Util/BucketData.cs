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

    public event EventHandler<int>? Tick;
    public event EventHandler<int>? Fill;

    private DateTime _lastFilled;
    private Task? _fillTask;

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
    }

    public void StartFill()
    //public TimeSpan GetTimeUntilNextFill() => Bucket.FillInterval - (DateTime.Now - _lastFilled);

    {
        if (_fillTask is not null)
        {
            return;
        }
        _fillTask = Task.Run(async () =>
        {
            while (!Bucket.IsFull)
            {
                await Task.Delay(Bucket.FillInterval + TimeSpan.FromMilliseconds(100));
                _lastFilled = DateTime.Now;
                Allowance++;
                Fill?.Invoke(this, Allowance);
            }
            _fillTask = null;
        });

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
        StartFill();
        return true;
    }

    public override string ToString() => $"[{Allowance}/{Capacity}] @{Bucket.FillRate}a/s";
}
