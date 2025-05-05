// Neo.Quantower.Toolkit.PipeDispatcher
// PipeClient - NamedPipeClientStream handling connection to the server and message sending/receiving

using Neo.Quantower.Abstractions.Models;

namespace Neo.Quantower.Abstractions.Interfaces
{
    public interface IPipeClient
    {
        /// <summary>
        /// Returns the unique identifier for the client.
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// Returns true if the client is connected to the server.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Returns true if this static instance is the server.
        /// </summary>
        ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; }
        /// <summary>
        /// Returns the name of the pipe used for communication.
        /// </summary>
        string PipeName { get; }
    }
}