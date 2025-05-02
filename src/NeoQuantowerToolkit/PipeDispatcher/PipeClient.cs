// Neo.Quantower.Toolkit.PipeDispatcher
// PipeClient - NamedPipeClientStream handling connection to the server and message sending/receiving

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.PipeDispatcher
{
    internal class PipeClient : IDisposable
    {
        private readonly string _pipeName;
        private NamedPipeClientStream _clientStream;
        private bool _disposed;
        public static Action<string> Logger { get; private set; }
        public bool IsConnected => _clientStream != null && _clientStream.IsConnected;


        public PipeClient(string pipeName, Action<string> logger)
    {
            _pipeName = pipeName;
            Logger = logger;
        }

        public async Task ConnectAsync()
        {
            _clientStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _clientStream.ConnectAsync();
            _ = Task.Run(ReadLoopAsync);
            Logger?.Invoke($"Client Connected");
        }

        public async Task SendAsync(string message)
        {
            if (_clientStream == null || !_clientStream.IsConnected)
                return;

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _clientStream.WriteAsync(buffer, 0, buffer.Length);
            await _clientStream.FlushAsync();
            Logger?.Invoke($"Client Send");
        }

        private async Task ReadLoopAsync()
        {
            var buffer = new byte[8192];
            while (_clientStream != null && _clientStream.IsConnected)
            {
                int bytesRead = await _clientStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await PipeDispatcher.Instance.DispatchEnvelopeAsync(json);
                    Logger?.Invoke($"Client Read");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _clientStream?.Dispose();
                Logger?.Invoke($"Client Disposed");
            }
            catch
            {
                // swallow any dispose errors
            }

            _disposed = true;
        }
    }
}