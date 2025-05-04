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
        /// <summary>
        /// Returns the list of clients connected to the dispatcher.
        /// </summary>
        List<IPipeClient> Clients { get; }
        /// <summary>
        /// returns the initialization status for the dispatcher.
        /// </summary>
        bool IsInitialized { get; }
        /// <summary>
        /// Returns the registration status for the dispatcher.
        /// </summary>
        bool IsRegistred { get; }
        /// <summary>
        /// Returns true if this static instance is the server.
        /// </summary>
        bool IsServer { get; }
        /// <summary>
        /// Rappresent the current logger of the dispatcher.
        /// </summary>
        ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; }
        /// <summary>
        /// Returns the maximum number of clients that can be connected to the dispatcher.
        /// </summary>
        int MaxClients { get; }
        /// <summary>
        /// Dispatcher name, if server => server name.
        /// </summary>
        string PipeName { get; }
        /// <summary>
        /// Returns the server instance of the dispatcher.
        /// </summary>
        IPipeClient Server { get; }
        /// <summary>
        /// returns the number of subscribed handlers.
        /// </summary>
        int SubscribedHandlerCounts { get; }
        /// <summary>
        /// returns the list of subscriptions.
        /// </summary>
        List<Subscription> Subscriptions { get; }
        /// <summary>
        /// Event triggered when a message is received.
        /// </summary>
        event EventHandler<string> MessageRecived;
        /// <summary>
        /// Event triggered when a message is sent.
        /// </summary>
        event EventHandler<string> MessageSent;
        /// <summary>
        /// Entry point for add new clients.
        /// </summary>
        /// <param name="pipeName">Client name</param>
        /// <param name="logger"></param>
        void AddClient(string pipeName, ICustomLogger<PipeDispatcherLoggingLevels> logger);
        void Dispose();
        /// <summary>
        /// Returns a snapshot of the current status of the dispatcher.
        /// </summary>
        void DumpStatus();
        /// <summary>
        /// Get connection status for the dispatcher.
        /// </summary>
        /// <returns>True if connected</returns>
        bool GetConneccionStatus();
        /// <summary>
        /// Entry point for initialize the dispatcher.
        /// </summary>
        /// <param name="pipeName">Dispatcher, server? name</param>
        /// <param name="logger"></param>
        /// <param name="maxClients"></param>
        /// <returns></returns>
        Task Initialize(string pipeName = "NeoQuantowerDispatcher", ICustomLogger<PipeDispatcherLoggingLevels> logger = null, int maxClients = 10);
        void onMessageRecived(string message);
        void onMessageSent(string message);
        /// <summary>
        /// Publish a message to all clients. will be automatically serialized to JSON. and dispatched to all subscribed Handlers.
        /// </summary>
        /// <typeparam name="TMessage">message of a custom type</typeparam>
        /// <param name="message">name , ll be automaticaly serializated and deserializate</param>
        /// <param name="priority"></param>
        /// <param name="pipeClients">if null will be dispatch to the first client</param>
        void Publish<TMessage>(TMessage message, TaskPriority priority = TaskPriority.Normal, List<IPipeClient> pipeClients = null);
        /// <summary>
        /// Avoid this method, attempt to refresh the static Instance
        /// </summary>
        void Refresh();
        /// <summary>
        /// EntryPoint for new handlers subscription.
        /// </summary>
        /// <typeparam name="TMessage">Any type</typeparam>
        /// <param name="handler">any desired Task</param>
        /// <param name="tag">Handler id</param>
        /// <returns></returns>
        IDisposable Subscribe<TMessage>(Func<TMessage, Task> handler, Guid tag);
        /// <summary>
        /// Remove a specific Handler
        /// </summary>
        /// <param name="subscription">Handler id</param>
        void UnscribeSubscription(Guid subscription);
    }
}