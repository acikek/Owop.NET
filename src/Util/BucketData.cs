using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Util;

/// <summary>An <see cref="IBucket"/> implementation.</summary>
public class BucketData : IBucket
{
    /// <summary>An empty bucket instance.</summary>
    public static BucketData Empty => new(0, 0, false);

    /// <summary>
    /// A delay to add to every <see cref="GetTimeToFill"/>call.
    /// Due to how OWOP handles bucket values, this is necessary for continuous allowance spending.
    /// </summary>
    public static readonly TimeSpan SafetyDelay = TimeSpan.FromMilliseconds(100);

    /// <summary>The internal allowance value.</summary>
    private double _allowance;

    /// <summary>The last time <see cref="Update"/> was called.</summary>
    private DateTime _lastUpdate;

    public int Capacity { get; set; }
    public int FillTime { get; set; }
    public bool Infinite { get; set; }

    public double FillRate => (double)Capacity / FillTime;
    public TimeSpan FillInterval => TimeSpan.FromSeconds(1.0 / FillRate);
    public bool IsFull => Allowance >= Capacity;
    public bool IsEmpty => Allowance <= 0;

    public double Allowance
    {
        get
        {
            Update();
            return _allowance;
        }
    }

    /// <summary>Constructs a bucket.</summary>
    /// <param name="capacity">The bucket's total capacity to refill towards.</param>
    /// <param name="fillTime">How long, in seconds, it takes to refill the bucket to capacity. </param>
    /// <param name="infinite">Whether the bucket is infinite (has no cooldown).</param>
    /// <param name="fill">Whether the allowance value should begin at capacity.</param>
    public BucketData(int capacity, int fillTime, bool infinite, bool fill = true)
    {
        SetValues(capacity, fillTime, fill);
        Infinite = infinite;
    }

    /// <summary>
    /// Sets the bucket's values.
    /// See <see cref="BucketData(int, int, bool, bool)"/> for more detail.
    /// </summary>
    public void SetValues(int capacity, int fillTime, bool fill = true)
    {
        Capacity = capacity;
        FillTime = fillTime;
        if (fill)
        {
            _allowance = capacity;
        }
        _lastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Updates the <see cref="Allowance"/> value based on the <see cref="_lastUpdate"/> 
    /// and <see cref="FillRate"/>.
    /// </summary>
    public void Update()
    {
        _allowance += FillRate * (DateTime.Now - _lastUpdate).TotalSeconds;
        _allowance = Math.Min(Capacity, _allowance);
        _lastUpdate = DateTime.Now;
    }

    public bool CanSpend(int amount) => Infinite || amount <= Allowance;

    /// <summary>Tries to spend the specified amount from the bucket's allowance.</summary>
    /// <param name="amount">The amount to try to spend.</param>
    /// <returns>Whether the amount was spent.</returns>
    public bool TrySpend(int amount)
    {
        if (Infinite)
        {
            return true;
        }
        if (!CanSpend(amount))
        {
            return false;
        }
        _allowance -= amount;
        return true;
    }

    /// <summary>
    /// Returns a <see cref="TimeSpan"/> of how long the bucket will take to refill
    /// the next point of allowance.
    /// </summary>
    private TimeSpan GetNextTimeToFill() => FillInterval * (1.0 - (_allowance % 1.0));

    public TimeSpan GetTimeToFill(int amount)
    {
        if (Infinite || amount <= 0)
        {
            return TimeSpan.Zero;
        }
        Update();
        return FillInterval * Math.Min(amount - 1, Capacity)
            + GetNextTimeToFill()
            + SafetyDelay;
    }

    public async Task DelayUntilFill(int amount) => await (Infinite || amount <= 0 ? Task.CompletedTask : Task.Delay(GetTimeToFill(amount)));

    public async Task DelayUntilHas(int allowance) => await DelayUntilFill(allowance - (int)_allowance);

    public async Task DelayUntilRestore() => await DelayUntilFill(Capacity - (int)_allowance);

    public async Task DelayOne() => await DelayUntilFill(1);

    public async Task DelayAny() => await DelayUntilHas(1);

    public override string ToString() => $"[{Allowance:0.00}/{Capacity}] @{FillRate}a/s";
}
