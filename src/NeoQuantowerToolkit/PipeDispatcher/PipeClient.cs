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
        /// <summary>
        /// The name of the pipe used for communication.
        /// </summary>
        private readonly string _pipeName;
        /// <summary>
        /// The NamedPipeClientStream used for communication with the server.
        /// </summary>
        private NamedPipeClientStream _clientStream;
        /// <summary>
        /// Flag indicating whether the client has been disposed.
        /// </summary>
        private bool _disposed;
        /// <summary>
        /// Custom logger injected for logging messages.
        /// </summary>
        public ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; private set; }
        /// <summary>
        /// Returns true if the client is connected to the server.
        /// </summary>
        public bool IsConnected => _clientStream != null && _clientStream.IsConnected;
        /// <summary>
        /// Returns the name of the pipe used for communication.
        /// </summary>
        public string PipeName => _pipeName;
        /// <summary>
        /// Returns the unique identifier for the client.
        /// </summary>
        public Guid Id { get; } = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pipeName">Pipe name</param>
        /// <param name="logger">Injeted logger</param>
        public PipeClient(string pipeName, ICustomLogger<PipeDispatcherLoggingLevels> logger)
        {
            _pipeName = pipeName;
            Logger = logger;
        }
        /// <summary>
        /// Entry point for the client to connect to the server.
        /// </summary>
        /// <returns>task resoult</returns>
        internal async Task ConnectAsync()
        {
            _clientStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _clientStream.ConnectAsync();
            _ = Task.Run(ReadLoopAsync);
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Client Connected");
        }
        /// <summary>
        /// Entry point for sending messages to the server.
        /// </summary>
        /// <param name="message">json</param>
        /// <returns>task resoult</returns>
        internal async Task SendAsync(string message)
        {
            if (_clientStream == null || !_clientStream.IsConnected)
                return;

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _clientStream.WriteAsync(buffer, 0, buffer.Length);
            await _clientStream.FlushAsync();
            Logger?.Log(PipeDispatcherLoggingLevels.Success, $"Client Send");
        }
        /// <summary>
        /// Entry point for reading messages from the server.
        /// </summary>
        /// <returns>task resoult</returns>
        internal async Task ReadLoopAsync()
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