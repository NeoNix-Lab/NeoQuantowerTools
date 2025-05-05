# Neo.Quantower.Toolkit.Abstractions

`AsyncTaskQueue` is a lightweight asynchronous prioritized task processor.
It supports:

- Prioritized execution (`High`, `Normal`, `Low`)
- Task retry logic
- Timeout per task
- Soft backpressure with delayed task queue

Designed for high-frequency, multi-client, low-latency scenarios such as server-stream dispatchers.

---

## Features

- **Thread-safe** internal queue
- **Non-blocking** background worker using `Task.Run`
- **Timeout** and **retry** support for fault isolation
- **Backpressure handling**: when queue overflows, low-priority tasks are delayed
- **Custom logger** support via `ICustomLogger<T>`
- **Factory support** for consistent and parameterized instantiation

---

## Usage Example

```csharp
// 1. Create the logger (optional)
ICustomLogger<TaskResoult> logger = new ConsoleTaskLogger();

// 2. Instantiate the queue
var queue = new AsyncTaskQueue(logger)
{
    MaxQueueLength = 256,
    MaxRetryAttempts = 2,
    TaskTimeout = TimeSpan.FromSeconds(5)
};

// 3. Enqueue tasks with different priorities
queue.Enqueue(async ct =>
{
    await Task.Delay(100, ct);
    Console.WriteLine("High priority task completed");
}, TaskPriority.High);

queue.Enqueue(async ct =>
{
    Console.WriteLine("Normal task executed");
    return;
}, TaskPriority.Normal);

queue.Enqueue(async ct =>
{
    throw new InvalidOperationException("Simulated failure");
}, TaskPriority.Low);
```

---

## Using the Factory

To centralize configuration and avoid repeated setup logic, use `AsyncTaskQueueFactory`:

```csharp
// Create a factory with shared defaults
var factory = new AsyncTaskQueueFactory(
    logger: new ConsoleTaskLogger(),
    maxQueueLength: 256,
    maxRetryAttempts: 2,
    timeout: TimeSpan.FromSeconds(5)
);

// Generate a queue for a specific client/module
var queue = factory.Create("client-42");

// Enqueue task
queue.Enqueue(async ct =>
{
    await Task.Delay(100, ct);
    Console.WriteLine("Client task completed");
}, TaskPriority.Normal);
```

You can also expose static presets via `AsyncTaskQueueDefaultFactories.ForServerStreams()`.

---

## Enum: `TaskPriority`

| Value | Description |
|-------|-------------|
| `High` | Critical operations (e.g., initialization, client connections) |
| `Normal` | Standard tasks |
| `Low` | Delayed or background operations |

---

## Enum: `TaskResoult`

| Value | Description |
|--------|-------------|
| `Completed` | Task finished successfully |
| `Failed` | Task threw exception (last retry) |
| `Delayed` | Task was temporarily deferred (backpressure) |
| `Reenqueued` | Delayed task moved back into active queue |
| `Queued` | Task added to queue successfully |

---

## Integration Tips

- You can create one `AsyncTaskQueue` per client / component
- Use it behind a factory for DI or naming-based routing


---

## License

MIT License © 2025


