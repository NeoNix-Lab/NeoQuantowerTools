using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using Moq;

namespace Neo.Quantower.Abstractions.Tests
{
    public class AsyncTaskQueueTests
    {
        [Fact]
        public void Enqueue_Always_LogsQueued()
        {
            // Arrange: creo il mock del logger
            var mockLogger = new Mock<ICustomLogger<TaskResoult>>();

            // Inietto il mock nel mio queue
            var queue = new AsyncTaskQueue(logger: mockLogger.Object);

            // Act: chiamo Enqueue (void)
            queue.Enqueue(ct => Task.CompletedTask, TaskPriority.High);

            // Assert: verifico che Log sia stato chiamato esattamente una volta
            mockLogger.Verify(
                log => log.Log(
                    TaskResoult.Queued,
                    It.Is<string>(s => s.Contains("enqueued"))),
                Times.Once   // ecco Times
            );

            queue.Dispose();
        }

        [Fact]
        public async Task Enqueue_TaskFunction_IsExecuted()
        {
            // Arrange
            var queue = new AsyncTaskQueue(logger: null);
            var tcs = new TaskCompletionSource<bool>();

            // Il delegate che segnala il completamento
            Func<CancellationToken, Task> work = async ct =>
            {
                await Task.Yield();
                tcs.SetResult(true);
            };

            // Act
            queue.Enqueue(work, TaskPriority.Normal);

            // Assert: aspetta fino a 1 secondo che il delegate venga eseguito
            var finished = await Task.WhenAny(tcs.Task, Task.Delay(1000));
            Assert.True(tcs.Task.IsCompletedSuccessfully, "La funzione non è mai stata eseguita");

            queue.Dispose();
        }

        [Fact]
        public void Enqueue_NullTaskFunc_ThrowsArgumentNullException()
        {
            // Arrange
            var queue = new AsyncTaskQueue(logger: null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => queue.Enqueue(null!, TaskPriority.Normal));
            queue.Dispose();
        }
    }
}
