// Neo.Quantower.Toolkit.PipeDispatcher
// PipeServer - NamedPipeServerStream handling incoming and outgoing messages

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.PipeDispatcher
{
	internal class PipeServer : IDisposable
	{
		private readonly string _pipeName;
		private NamedPipeServerStream _serverStream;
		private bool _disposed;
		public static Action<string> Logger { get; private set; }
		public bool IsConnected => _serverStream != null && _serverStream.IsConnected;

		public PipeServer(string pipeName, Action<string> logger)
		{
			_pipeName = pipeName;
			Logger = logger;
		}

		public async Task StartAsync()
		{
			_serverStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
			await _serverStream.WaitForConnectionAsync();
			_ = Task.Run(ReadLoopAsync);
			Logger?.Invoke($"Server Connected");
		}

		public async Task SendAsync(string message)
		{
			if (_serverStream == null || !_serverStream.IsConnected)
				return;

			byte[] buffer = Encoding.UTF8.GetBytes(message);
			await _serverStream.WriteAsync(buffer, 0, buffer.Length);
			await _serverStream.FlushAsync();
			Logger?.Invoke($"Server Send");
		}

		private async Task ReadLoopAsync()
		{
			var buffer = new byte[8192];
			while (_serverStream != null && _serverStream.IsConnected)
			{
				int bytesRead = await _serverStream.ReadAsync(buffer, 0, buffer.Length);
				if (bytesRead > 0)
				{
					string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					await PipeDispatcher.Instance.DispatchEnvelopeAsync(json);
					Logger?.Invoke($"Server Read");
				}
			}
		}

		public void Dispose()
		{
			if (_disposed) return;

			try
			{
				_serverStream?.Dispose();
				Logger?.Invoke($"Server Disposed");

			}
			catch
			{
				// swallow any dispose errors
			}

			_disposed = true;
		}
	}
}
