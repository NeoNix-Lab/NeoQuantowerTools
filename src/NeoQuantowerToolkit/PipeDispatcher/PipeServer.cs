// Neo.Quantower.Toolkit.PipeDispatcher
// PipeServer - NamedPipeServerStream handling incoming and outgoing messages

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Neo.Quantower.Abstractions.Models;
using Neo.Quantower.Abstractions.Interfaces;



namespace Neo.Quantower.Toolkit.PipeDispatcher
{
	internal class PipeServer : IDisposable, IPipeClient
	{
		/// <summary>
		/// The name of the pipe used for communication.
		/// </summary>
		private readonly string _pipeName;
		/// <summary>
		/// The NamedPipeServerStream used for communication with the server.
		/// </summary>
		private NamedPipeServerStream _serverStream;
		/// <summary>
		/// Flag indicating whether the server has been disposed.
		/// </summary>
		private bool _disposed;
		/// <summary>
		/// Injected logger for logging messages.
		/// </summary>
		public ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; private set; }
		/// <summary>
		/// returns true if the server is connected to the client.
		/// </summary>
		public bool IsConnected => _serverStream != null && _serverStream.IsConnected;
		/// <summary>
		/// Returns the name of the pipe used for communication.
		/// </summary>
		public string PipeName => _pipeName;
		/// <summary>
		/// Returns the unique identifier for the server.
		/// </summary>
		public Guid Id { get; } = new();
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pipeName">Pipe name</param>
		/// <param name="logger">Injeted logger</param>
		public PipeServer(string pipeName, ICustomLogger<PipeDispatcherLoggingLevels> logger)
		{
			_pipeName = pipeName;
			Logger = logger;
		}
		/// <summary>
		/// entry point for the server to start listening for incoming connections.
		/// </summary>
		/// <returns>task resoult</returns>
		internal async Task StartAsync()
		{
			_serverStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
				PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

			await _serverStream.WaitForConnectionAsync();
			_ = Task.Run(ReadLoopAsync);
			Logger?.Log(PipeDispatcherLoggingLevels.System, $"Server Connected");
		}
		/// <summary>
		/// Entry point for sending messages to the client.
		/// </summary>
		/// <param name="message"></param>
		/// <returns>task resoult</returns>
		internal async Task SendAsync(string message)
		{
			if (_serverStream == null || !_serverStream.IsConnected)
				return;

			byte[] buffer = Encoding.UTF8.GetBytes(message);
			await _serverStream.WriteAsync(buffer, 0, buffer.Length);
			await _serverStream.FlushAsync();
			Logger?.Log(PipeDispatcherLoggingLevels.Success, $"Server Send");
		}
		/// <summary>
		/// Entry point for reading messages from the client.
		/// </summary>
		/// <returns></returns>
		private async Task ReadLoopAsync()
		{
			var buffer = new byte[8192];
			while (_serverStream != null && _serverStream.IsConnected)
			{
				int bytesRead = await _serverStream.ReadAsync(buffer, 0, buffer.Length);
				if (bytesRead > 0)
				{
					string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					PipeDispatcher.Instance.DispatchEnvelope(json);
					Logger?.Log(PipeDispatcherLoggingLevels.Success, $"Server Read");
				}
			}
		}

		public void Dispose()
		{
			if (_disposed) return;

			try
			{
				_serverStream?.Dispose();
				Logger?.Log(PipeDispatcherLoggingLevels.System, $"Server Disposed");

			}
			catch (Exception ex)
			{
				Logger?.Log(PipeDispatcherLoggingLevels.Error, $"Error disposing server: {ex.Message}");
			}

			_disposed = true;
		}
	}
}
