// Neo.Quantower.Toolkit.PipeDispatcher
// PipeClient - NamedPipeClientStream handling connection to the server and message sending/receiving

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Neo.Quantower.Abstractions.Models;
using Neo.Quantower.Abstractions.Interfaces;

namespace Neo.Quantower.Toolkit.PipeDispatcher
{
    internal class PipeClient : IDisposable, IPipeClient
    {
        private const bool _IsServer = false;

        private readonly string _pipeName;
        private NamedPipeClientStream _clientStream;
        private bool _disposed;
        public ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; private set; }
        public bool IsConnected => _clientStream != null && _clientStream.IsConnected;
        public bool IsServer => _IsServer;
        public string PipeName => _pipeName;
        public Guid Id { get; } = new();



        public PipeClient(string pipeName, ICustomLogger<PipeDispatcherLoggingLevels> logger)
        {
            _pipeName = pipeName;
            Logger = logger;
        }

        public async Task ConnectAsync()
        {
            _clientStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _clientStream.ConnectAsync();
            _ = Task.Run(ReadLoopAsync);
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Client Connected");
        }

        public async Task SendAsync(string message)
        {
            if (_clientStream == null || !_clientStream.IsConnected)
                return;

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _clientStream.WriteAsync(buffer, 0, buffer.Length);
            await _clientStream.FlushAsync();
            Logger?.Log(PipeDispatcherLoggingLevels.Success, $"Client Send");
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
                    PipeDispatcher.Instance.DispatchEnvelope(json);
                    Logger?.Log(PipeDispatcherLoggingLevels.Success, $"Client Read");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _clientStream?.Dispose();
                Logger?.Log(PipeDispatcherLoggingLevels.System, $"Client Disposed");
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"Client Dispose Error {ex.Message}");
            }

            _disposed = true;
        }
    }
}