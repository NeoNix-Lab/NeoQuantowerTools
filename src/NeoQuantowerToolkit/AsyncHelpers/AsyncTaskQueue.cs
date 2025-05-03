// AsyncTaskQueue.cs
using Neo.Quantower.Abstractions;
using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.AsyncHelpers
{
    public enum TaskPriority { High, Normal, Low }

    public enum TaskResoult { Succes, Failed }

    public class AsyncTaskQueue : IDisposable
    {
        private readonly SortedDictionary<TaskPriority, Queue<Func<CancellationToken, Task>>> priorityQueues;
        private readonly object locker = new();
        private readonly ManualResetEventSlim queueEvent = new(false);
        private readonly CancellationTokenSource cts = new();
        private Queue<Func<CancellationToken, Task>> _delayedTasks = new Queue<Func<CancellationToken, Task>>();
        private int _queueCount
        {
            get
            {
                lock (locker)
                {
                    int count = 0;
                    foreach (var queue in priorityQueues)
                    {
                        if (queue.Value.Count > 0)
                            count += queue.Value.Count;
                    }

                    return count;
                }
            }
        }


        public ICustomLogger<TaskResoult> Logger { get; private set; }
        public int MaxQuequeLenght { get; set; } = 100;
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan TaskTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public event Action<Task> OnTaskCompleted;
        public event Action<Exception> OnTaskFailed;

        public AsyncTaskQueue(ICustomLogger<TaskResoult> logger = null)
        {
            Logger = logger;

            priorityQueues = new SortedDictionary<TaskPriority, Queue<Func<CancellationToken, Task>>>
            {
                { TaskPriority.High, new Queue<Func<CancellationToken, Task>>() },
                { TaskPriority.Normal, new Queue<Func<CancellationToken, Task>>() },
                { TaskPriority.Low, new Queue<Func<CancellationToken, Task>>() }
            };
            Task.Factory.StartNew(ProcessQueue, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Enqueue(Func<CancellationToken, Task> taskFactory, TaskPriority priority = TaskPriority.Normal)
        {
            lock (locker)
            {
                priorityQueues[priority].Enqueue(taskFactory);

                try
                {
                    if (_queueCount > MaxQuequeLenght)
                    {
                        var task = priorityQueues[TaskPriority.Low].Any()
                            ? priorityQueues[TaskPriority.Low].Dequeue()
                            : priorityQueues[TaskPriority.Normal].Any()
                            ? priorityQueues[TaskPriority.Normal].Dequeue()
                            : priorityQueues[TaskPriority.High].Dequeue();

                        _delayedTasks.Enqueue(task);
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Log(TaskResoult.Failed, $"Error in Enqueue: {ex.Message}");
                    throw;
                }
                
            }
            queueEvent.Set();
        }

        private async void ProcessQueue()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                queueEvent.Wait(cts.Token);
                queueEvent.Reset();

                Func<CancellationToken, Task> taskFactory = null;
                lock (locker)
                {
                   
                    foreach (var queue in priorityQueues)
                    {
                        if (queue.Value.Count > 0)
                        {
                            taskFactory = queue.Value.Dequeue();
                            break;
                        }
                    }
                }

                if (taskFactory != null)
                {
                    int attempt = 0;

                    while (attempt < MaxRetryAttempts && !cts.Token.IsCancellationRequested)
                    {
                        attempt++;
                        try
                        {
                            var task = taskFactory(cts.Token);
                            if (await Task.WhenAny(task, Task.Delay(TaskTimeout, cts.Token)) == task)
                            {
                                OnTaskCompleted?.Invoke(task);

                                Logger?.Log(TaskResoult.Succes, $"Task completed successfully after {attempt} attempts.");

                                if (_queueCount < MaxQuequeLenght && _delayedTasks.Any())
                                {
                                    var paused_task = _delayedTasks.Dequeue();
                                    priorityQueues[TaskPriority.Low].Enqueue(paused_task);
                                    
                                }

                                break;
                            }
                            else
                            {
                                throw new TimeoutException("Task timed out.");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (attempt >= MaxRetryAttempts || ex is OperationCanceledException)
                            {
                                OnTaskFailed?.Invoke(ex);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            queueEvent.Set();
        }
    }
}
