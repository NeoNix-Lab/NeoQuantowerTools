// Neo.Quantower.Toolkit.PipeDispatcher
// PipeClient - NamedPipeClientStream handling connection to the server and message sending/receiving

using Neo.Quantower.Abstractions.Models;

namespace Neo.Quantower.Abstractions.Interfaces
{
    public interface IPipeClient
    {
        Guid Id { get; }
        bool IsConnected { get; }
        bool IsServer { get; }
        ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; }
        string PipeName { get; }
    }
}