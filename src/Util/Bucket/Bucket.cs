﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Util;

/// <summary>An <see cref="IBucket"/> implementation.</summary>
public class Bucket : IBucket
{
    /// <summary>An empty bucket instance.</summary>
    public static Bucket Empty => new(0, 0, false);

    /// <summary>
    /// A delay to add to every <see cref="GetTimeToFill"/>call.
    /// Due to how OWOP handles bucket values, this is necessary for continuous allowance spending.
    /// </summary>
    public static readonly TimeSpan SafetyDelay = TimeSpan.FromMilliseconds(100);

    /// <summary>The internal allowance value.</summary>
    private double _allowance;

    /// <summary>The last time <see cref="Update"/> was called.</summary>
    private DateTime _lastUpdate;

    /// <inheritdoc/>
    public int Capacity { get; set; }

    /// <inheritdoc/>
    public int FillTime { get; set; }

    /// <inheritdoc/>
    public bool Infinite { get; set; }

    /// <inheritdoc/>
    public double FillRate => (double)Capacity / FillTime;

    /// <inheritdoc/>
    public TimeSpan FillInterval => TimeSpan.FromSeconds(1.0 / FillRate);

    /// <inheritdoc/>
    public bool IsFull => Allowance >= Capacity;

    /// <inheritdoc/>
    public bool IsEmpty => Allowance <= 0.0;

    /// <inheritdoc/>
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
    public Bucket(int capacity, int fillTime, bool infinite, bool fill = true)
    {
        SetValues(capacity, fillTime, fill);
        Infinite = infinite;
    }

    /// <summary>
    /// Sets the bucket's values.
    /// See <see cref="Bucket(int, int, bool, bool)"/> for more detail.
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

    /// <inheritdoc/>
    public bool CanSpend(double amount) => Infinite || amount <= Allowance;

    /// <summary>Tries to spend the specified amount from the bucket's allowance.</summary>
    /// <param name="amount">The amount to try to spend.</param>
    /// <returns>Whether the amount was spent.</returns>
    public bool TrySpend(double amount)
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

    /// <inheritdoc/>
    public TimeSpan GetTimeToFill(double amount)
    {
        if (Infinite || amount <= 0.0)
        {
            return TimeSpan.Zero;
        }
        Update();
        return FillInterval * Math.Min(amount, Capacity)
            //+ GetNextTimeToFill() TODO: (never) fix
            + SafetyDelay;
    }

    /// <inheritdoc/>
    public async Task DelayUntilFill(double amount) => await Task.Delay(GetTimeToFill(amount));

    /// <inheritdoc/>
    public async Task DelayUntilHas(double allowance) => await DelayUntilFill(allowance - _allowance);

    /// <inheritdoc/>
    public async Task DelayUntilRestore() => await DelayUntilFill(Capacity - _allowance);

    /// <inheritdoc/>
    public async Task DelayOne() => await DelayUntilFill(1.0);

    /// <inheritdoc/>
    public async Task DelayAny() => await DelayUntilHas(1.0);

    /// <inheritdoc/>
    public override string ToString() => $"[{Allowance:0.00}/{Capacity}] @{FillRate:0.00}a/s";
}
