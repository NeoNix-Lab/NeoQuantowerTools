using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Pipe
{
    public interface IPipeTransport
    {
        event EventHandler<string> MessageReceived;

        Task ConnectAsync(string pipeName, CancellationToken cancellationToken = default);
        Task DisconnectAsync();
        void Dispose();
        Task SendAsync(string jsonPayload);
        Task StartServerAsync(string pipeName, CancellationToken cancellationToken = default);
        Task StopServerAsync();
    }
}