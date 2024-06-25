using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Owop.Game;

namespace Owop.Util;

/// <summary>Represents a queue processed according to a <see cref="Util.Bucket"/>.</summary>
/// <typeparam name="Obj">The queue object type.</typeparam>
/// <param name="Bucket">The bucket to wait for.</param>
/// <param name="Name">The debug name of this queue.</param>
/// <param name="World">The attached world (for debug logging).</param>
/// <param name="Processor">The processor function for queue objects.</param>
public record BucketQueue<Obj, Res>(Bucket Bucket, string Name, World World, Func<Obj, Task<Res>> Processor)
{
    /// <summary>The internal queue buffer.</summary>
    private readonly ConcurrentQueue<(Obj, TaskCompletionSource<Res>)> _buffer = [];

    /// <summary>The queue processing task, or <c>null</c> if none is running.</summary>
    private Task? _task = null;

    /// <summary>Adds an object to the queue.</summary>
    /// <param name="obj">The object to add.</param>
    /// <returns>A task source that completes when the object has been processed.</returns>
    public TaskCompletionSource<Res> Add(Obj obj)
    {
        World.Logger.LogDebug($"Queuing {Name} object: '{obj}'");
        var source = new TaskCompletionSource<Res>(TaskCreationOptions.RunContinuationsAsynchronously);
        _buffer.Enqueue((obj, source));
        Process();
        return source;
    }

    /// <summary>Processes the queue.</summary>
    public void Process()
    {
        if (_task is not null)
        {
            return;
        }
        _task = Task.Run(async () =>
        {
            World.Logger.LogDebug($"Starting {Name} task...");
            int count = 0;
            while (!_buffer.IsEmpty)
            {
                if (!_buffer.TryDequeue(out var pair))
                {
                    continue;
                }
                (Obj obj, var task) = pair;
                await Bucket.DelayAny();
                if (!Bucket.TrySpend(1))
                {
                    World.Logger.LogError($"Failed to process {Name} '{obj}'; no allowance left!");
                    continue;
                }
                World.Logger.LogDebug($"Processing {Name} object: '{obj}'");
                task.SetResult(await Processor(obj));
                count++;
            }
            _task = null;
            World.Logger.LogDebug($"Completed {Name} task with {count} object(s) sent.");
        });
    }
}
