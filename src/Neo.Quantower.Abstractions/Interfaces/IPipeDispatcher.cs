// Neo.Quantower.Toolkit.PipeDispatcher
// PipeDispatcher - Core for NamedPipe-based messaging

using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Interfaces
{
    public interface IPipeDispatcher
    {
        bool IsInitialized { get; }
        bool IsRegistred { get; }
        bool IsServer { get; }
        string PipeName { get; }
        int SubscribedHandlerCounts { get; }
        List<Subscription> Subscriptions { get; }

        event EventHandler<string> MessageRecived;
        event EventHandler<string> MessageSent;

        void Dispose();
        void DumpStatus();
        bool GetConneccionStatus();
        Task Initialize(string pipeName, ICustomLogger<PipeDispatcherLoggingLevels> logger, int maxClients);
        void onMessageRecived(string message);
        void onMessageSent(string message);
        Task PublishAsync<TMessage>(TMessage message);
        void Refresh();
        IDisposable Subscribe<TMessage>(Func<TMessage, Task> handler, Guid tag);
        void UnscribeSubscription(Guid subscription);
    }
}