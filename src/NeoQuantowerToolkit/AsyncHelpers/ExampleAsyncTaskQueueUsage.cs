using Neo.Quantower.Toolkit.AsyncHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class ExampleAsyncTaskQueueUsage
{
    public static async Task Run()
    {
        var queue = new AsyncTaskQueue
        {
            MaxRetryAttempts = 2,
            TaskTimeout = TimeSpan.FromSeconds(5)
        };

        queue.OnTaskCompleted += task => Console.WriteLine("[Queue] Task completed.");
        queue.OnTaskFailed += ex => Console.WriteLine($"[Queue] Task failed: {ex.Message}");

        queue.Enqueue(async ct =>
        {
            Console.WriteLine("Started task 1.");
            await Task.Delay(1000, ct);
            Console.WriteLine("Finished task 1.");
        }, TaskPriority.Normal);

        queue.Enqueue(async ct =>
        {
            Console.WriteLine("Started task 2.");
            await Task.Delay(10000, ct); // Will timeout
        }, TaskPriority.Low);

        // Keep main thread alive to see output
        Console.WriteLine("Tasks queued. Press Enter to exit.");
        Console.ReadLine();

        queue.Dispose();
    }
}
