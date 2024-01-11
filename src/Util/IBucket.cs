namespace Owop.Util;

/// <summary>Represents a refilling container with some spendable interaction <see cref="Allowance"/>.</summary>
/// <param name="data">The internal bucket data.</param>
public interface IBucket
{
    /// <summary>An empty bucket instance.</summary>
    static Bucket Empty => BucketData.Empty;

    /// <summary>The bucket's total capacity to refill towards.</summary>
    int Capacity { get; }

    /// <summary>How long, in seconds, it takes to refill an empty bucket to the <see cref="Capacity">.</summary>
    int FillTime { get; }

    /// <summary>The bucket's current spendable allowance.</summary>
    double Allowance { get; }

    /// <summary>Whether the bucket is infinite (has no cooldowns).</summary>
    bool Infinite { get; }

    /// <summary>The rate, in allowance/s, at which the bucket refills to capacity.</summary>
    double FillRate { get; }

    /// <summary>The duration, in seconds, it takes the bucket to refill one allowance.</summary>
    TimeSpan FillInterval { get; }

    /// <summary>Whether the bucket is full.</summary>
    bool IsFull { get; }

    /// <summary>Whether the bucket is empty.</summary>
    bool IsEmpty { get; }

    /// <summary>Returns whether the bucket can spend the specified amount from its allowance.</summary>
    /// <param name="amount">The amount to spend.</param>
    bool CanSpend(int amount);

    /// <summary>
    /// Returns a <see cref="TimeSpan"/> of how long the bucket will take to refill
    /// the specified amount of allowance.
    /// </summary>
    /// <param name="amount">The amount to refill.</param>
    TimeSpan GetTimeToFill(int amount);

    /// <summary>
    /// Creates a task that completes after the bucket has refilled 
    /// the specified amount of allowance.
    /// </summary>
    /// <param name="amount">The amount to refill.</param>
    /// <returns>A task that represents the time delay.</returns>
    Task DelayUntilFill(int amount);

    /// <summary>
    /// Creates a task that completes after the bucket has refilled <b>to</b>
    /// the specified amount of allowance.
    /// </summary>
    /// <param name="amount">The amount to refill to.</param>
    /// <returns>A task that represents the time delay.</returns>
    Task DelayUntilHas(int allowance);

    /// <summary>Creates a task that completes after the bucket has completely refilled.</summary>
    /// <returns>A task that represents the time delay.</returns>
    Task DelayUntilRestore();

    /// <summary>Creates a task that completes after the bucket has refilled exactly one allowance point.</summary>
    /// <returns>A task that represents the time delay.</returns>
    Task DelayOne();

    /// <summary>Creates a task that completes after the bucket has refilled <b>to</b> exactly one allowance point.</summary>
    /// <returns>A task that represents the time delay.</returns>
    Task DelayAny();
}
