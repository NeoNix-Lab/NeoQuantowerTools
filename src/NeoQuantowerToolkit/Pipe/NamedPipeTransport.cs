using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Pipe
{
    public class NamedPipeTransport : IPipeTransport
    {
        private NamedPipeServerStream? _server;
        private NamedPipeClientStream? _client;
        private CancellationTokenSource? _cts;

        public event EventHandler<string>? MessageReceived;

        public async Task StartServerAsync(string pipeName, CancellationToken cancellationToken = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = Task.Run(() => ServerAcceptLoop(pipeName, _cts.Token));
        }

        private async Task ServerAcceptLoop(string pipeName, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                await server.WaitForConnectionAsync(token);
                _ = Task.Run(() => HandleServerClientAsync(server, token));
            }
        }

        private async Task HandleServerClientAsync(NamedPipeServerStream server, CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                while (server.IsConnected && !token.IsCancellationRequested)
                {
                    int read = await server.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read > 0)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, read);
                        MessageReceived?.Invoke(this, json);
                    }
                }
            }
            finally
            {
                server.Disconnect();
                server.Dispose();
            }
        }

        public Task StopServerAsync()
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(string pipeName, CancellationToken cancellationToken = default)
        {
            _client = new NamedPipeClientStream(
                ".",
                pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous,
                TokenImpersonationLevel.Impersonation);

            await _client.ConnectAsync(cancellationToken);
            _ = Task.Run(() => ClientListenLoop(cancellationToken));
        }

        private async Task ClientListenLoop(CancellationToken token)
        {
            var buffer = new byte[4096];
            while (_client!.IsConnected && !token.IsCancellationRequested)
            {
                int read = await _client.ReadAsync(buffer, 0, buffer.Length, token);
                if (read > 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, read);
                    MessageReceived?.Invoke(this, json);
                }
            }
        }

        public Task DisconnectAsync()
        {
            _client?.Dispose();
            _client = null;
            return Task.CompletedTask;
        }

        public async Task SendAsync(string jsonPayload)
        {
            byte[] data = Encoding.UTF8.GetBytes(jsonPayload);
            if (_server != null && _server.IsConnected)
                await _server.WriteAsync(data, 0, data.Length);
            if (_client != null && _client.IsConnected)
                await _client.WriteAsync(data, 0, data.Length);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _server?.Dispose();
            _client?.Dispose();
            _cts?.Dispose();
        }
    }
}
