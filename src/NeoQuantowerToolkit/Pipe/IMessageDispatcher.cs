using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Pipe
{
    public interface IMessageDispatcher
    {
        void Dispose();
        Task InitializeAsClientAsync(string pipeName, CancellationToken token = default);
        Task InitializeAsServerAsync(string pipeName, CancellationToken token = default);
        Task PublishAsync<T>(T message);
        IDisposable Subscribe<T>(Func<T, Task> handler);
    }
}