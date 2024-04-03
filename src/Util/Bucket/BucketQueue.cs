using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Owop.Game;

namespace Owop.Util;

public record BucketQueue<T>(Bucket Bucket, string Name, World World, Func<T, Task> Processor)
{
    private readonly ConcurrentQueue<(T, TaskCompletionSource)> _buffer = [];
    private Task? _task = null;

    public TaskCompletionSource Add(T obj)
    {
        World.Logger.LogDebug($"Queuing {Name} object: '{obj}'");
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _buffer.Enqueue((obj, source));
        Process();
        return source;
    }

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
                (T obj, var task) = pair;
                await Bucket.DelayAny();
                if (!Bucket.TrySpend(1))
                {
                    World.Logger.LogError($"Failed to process {Name} '{obj}'; no allowance left!");
                    continue;
                }
                World.Logger.LogDebug($"Processing {Name} object: '{obj}'");
                await Processor(obj);
                count++;
                task.SetResult();
            }
            _task = null;
            World.Logger.LogDebug($"Completed {Name} task with {count} object(s) sent.");
        });
    }
}
