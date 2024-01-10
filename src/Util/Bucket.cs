namespace Owop.Util;

/// <summary>Represents a refilling container with some spendable interaction <see cref="Allowance"/>.</summary>
/// <param name="data">The internal bucket data.</param>
public class Bucket(BucketData data)
{
    /// <summary>An empty bucket instance.</summary>
    public static Bucket Empty => BucketData.Empty;

    /// <summary>
    /// A delay to add to every <see cref="GetTimeToFill"/>call.
    /// Due to how OWOP handles bucket values, this is necessary for continuous allowance spending.
    /// </summary>
    public static readonly TimeSpan SafetyDelay = TimeSpan.FromMicroseconds(10);

    /// <summary>The internal bucket data.</summary>
    private readonly BucketData _instance = data;

    /// <summary>The bucket's total capacity to refill towards.</summary>
    public int Capacity => _instance.Capacity;

    /// <summary>How long, in seconds, it takes to refill an empty bucket to the <see cref="Capacity">.</summary>
    public int FillTime => _instance.FillTime;

    /// <summary>The bucket's current spendable allowance.</summary>
    public int Allowance
    {
        get
        {
            _instance.Update();
            return _instance.Allowance;
        }
    }

    /// <summary>Whether the bucket is infinite (has no cooldowns).</summary>
    public bool Infinite => _instance.Infinite;

    /// <summary>The rate, in allowance/s, at which the bucket refills to capacity.</summary>
    public double FillRate => (double)Capacity / FillTime;

    /// <summary>The duration, in seconds, it takes the bucket to refill one allowance.</summary>
    public TimeSpan FillInterval => TimeSpan.FromSeconds(1.0 / FillRate);

    /// <summary>Whether the bucket is full.</summary>
    public bool IsFull => Allowance >= Capacity;

    /// <summary>Whether the bucket is empty.</summary>
    public bool IsEmpty => Allowance <= 0;

    /// <summary>Returns whether the bucket can spend the specified amount from its allowance.</summary>
    /// <param name="amount">The amount to spend.</param>
    public bool CanSpend(int amount) => Infinite || amount <= Allowance;

    /// <summary>
    /// Returns a <see cref="TimeSpan"/> of how long the bucket will take to refill
    /// the specified amount of allowance.
    /// </summary>
    /// <param name="amount">The amount to refill.</param>
    public TimeSpan GetTimeToFill(int amount)
    {
        _instance.Update();
        return Infinite || amount <= 0
            ? TimeSpan.Zero
            : FillInterval * Math.Min(amount, Capacity) + TimeSpan.FromMilliseconds(10);
    }

    /// <summary>
    /// Creates a task that completes after the bucket has refilled 
    /// the specified amount of allowance.
    /// </summary>
    /// <param name="amount">The amount to refill.</param>
    /// <returns>A task that represents the time delay.</returns>
    public async Task DelayUntilFill(int amount) => await (Infinite || amount <= 0 ? Task.CompletedTask : Task.Delay(GetTimeToFill(amount)));

    /// <summary>
    /// Creates a task that completes after the bucket has refilled <b>to</b>
    /// the specified amount of allowance.
    /// </summary>
    /// <param name="amount">The amount to refill to.</param>
    /// <returns>A task that represents the time delay.</returns>
    public async Task DelayUntilHas(int allowance) => await DelayUntilFill(allowance - Allowance);

    /// <summary>Creates a task that completes after the bucket has completely refilled.</summary>
    /// <returns>A task that represents the time delay.</returns>
    public async Task DelayUntilRestore() => await DelayUntilFill(Capacity - Allowance);

    /// <summary>Creates a task that completes after the bucket has refilled exactly one allowance point.</summary>
    /// <returns>A task that represents the time delay.</returns>
    public async Task DelayOne() => await DelayUntilFill(1);

    /// <summary>Creates a task that completes after the bucket has refilled <b>to</b> exactly one allowance point.</summary>
    /// <returns>A task that represents the time delay.</returns>
    public async Task DelayAny() => await DelayUntilHas(1);

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString() => _instance.ToString();

    /// <summary>Returns <see cref="BucketData.Bucket"/>.</summary>
    /// <param name="data">The bucket data instance.</param>
    public static implicit operator Bucket(BucketData data) => data.Bucket;
}
