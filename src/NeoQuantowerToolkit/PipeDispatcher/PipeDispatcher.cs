// Neo.Quantower.Toolkit.PipeDispatcher
// PipeDispatcher - Core for NamedPipe-based messaging

using Neo.Quantower.Abstractions.Factories;
using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.PipeDispatcher
{


    /// <summary>
    /// Core dispatcher using NamedPipe for cross-module messaging.
    /// Handles publishing and subscribing messages through a centralized system.
    /// </summary>
    public class PipeDispatcher : IDisposable, IPipeDispatcher
    {
        /// <summary>
        /// Singleton instance of the PipeDispatcher.
        /// </summary>
        private static Lazy<PipeDispatcher> _instance = new(() => new PipeDispatcher());
        public static PipeDispatcher Instance => _instance.Value;
        public ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; private set; }

        private KeyValuePair<PipeServer, AsyncTaskQueue> _server;
        private Dictionary<PipeClient, AsyncTaskQueue> _clients = new();
        private DispatcherRegistry _registry;
        private string _pipeName;
        private bool _isServer;
        private bool _disposed;
        private bool _initialized;
        private bool _isConnected;
        private AsyncTaskQueue _asyncDispatcher;

        public event EventHandler<string> MessageSent;
        public event EventHandler<string> MessageRecived;
        /// <summary>
        /// Count of subscribed handlers.
        /// </summary>
        public int SubscribedHandlerCounts => _registry.SubsCount;
        /// <summary>
        /// Server instance of the dispatcher.
        /// </summary>
        public IPipeClient Server => _server.Key;
        /// <summary>
        /// Clients connected to the dispatcher.
        /// </summary>
        public List<IPipeClient> Clients
        {
            get
            {
                var clients = new List<IPipeClient>();
                foreach (var client in _clients)
                {
                    clients.Add(client.Key);
                }
                return clients;
            }
        }

        public int MaxClients { get; protected set; }
        public List<Subscription> Subscriptions => _registry.Subscriptions;
        public string PipeName => _pipeName;
        /// <summary>
        /// Dispatcher status
        /// </summary>
        public bool IsInitialized => _initialized;
        public bool IsServer => _isServer;
        public bool IsRegistred => _registry.Subscriptions.Any();
        /// <summary>
        /// Returns the connection status of the dispatcher.
        /// </summary>
        /// <returns></returns>
        public bool GetConneccionStatus() => _isServer
            ? _server.Key.IsConnected
            : _clients.Keys.Any(x => x.IsConnected);


        private PipeDispatcher()
        {
            _initialized = false;
            _disposed = false;
        }

        /// <summary>
        /// Unscribes a subscription using its Guid.
        /// </summary>
        /// <param name="subscription">Subscription Guid</param>
        public void UnscribeSubscription(Guid subscription) => _registry.Unsubscribe(subscription);

        /// TODO : Add error handling for connection issues.
        /// TODO : Define Logging Levels And Verbose.
        /// TODO : Handle cts.
        /// <summary>
        /// Initializes the dispatcher as either a server or fallback client.
        /// </summary>
        /// <param name="pipeName">The name of the pipe to use.</param>
        /// <param name="logger">Optional logger instance.</param>
        /// <param name="maxClients">Maximum clients allowed when acting as server.</param>

        public async Task Initialize(string pipeName = "NeoQuantowerDispatcher", ICustomLogger<PipeDispatcherLoggingLevels> logger = null, int maxClients = 10)
        {
            if (_initialized)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.System, $"PipeDispatcher: Inizialize: Dispatcher already initialized");
                return;
            }

            _asyncDispatcher = AsyncTaskQueueDefaultFactories.ForDispatcherStreams(logger?.Logger).Create("PipeDispatcher");

            this.MaxClients = maxClients;
            _pipeName = pipeName;
            Logger = logger;
            _registry = new DispatcherRegistry(logger);

            try
            {
                PipeServer pipe_server = new PipeServer(pipeName, logger);
                _server = new KeyValuePair<PipeServer, AsyncTaskQueue>
                    (
                        key: pipe_server,
                        value: AsyncTaskQueueDefaultFactories.ForServerStreams(Logger?.Logger)
                            .Create(_server.Key.Id)
                    );
                await _server.Key.StartAsync();

                _isServer = true;
                _initialized = _server.Key.IsConnected;
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Inizialize: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        ///  Entry point for adding a client to the dispatcher.
        /// </summary>
        /// <param name="pipeName">Client Name have to match with a connected server name</param>
        /// <param name="logger"></param>
        public void AddClient(string pipeName, ICustomLogger<PipeDispatcherLoggingLevels> logger)
        {
            try
            {
                if (_clients.Count < MaxClients)
                {
                    var client = new PipeClient(pipeName, logger);
                    var queue = AsyncTaskQueueDefaultFactories.ForServerStreams(logger?.Logger).Create(client.Id);
                    _clients.Add(client, queue);
                    queue.Enqueue(_ => client.ConnectAsync(), TaskPriority.High);
                }
                else
                {
                    Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: AddClient: Max clients reached");
                    throw new Exception(message: "PipeDispatcher: AddClient: Max clients reached");
                }
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: AddClient: {ex.Message}");
                throw;
            }

        }

        public virtual void onMessageSent(string message)
        {
            MessageSent?.Invoke(this, message);
        }

        public virtual void onMessageRecived(string message)
        {
            MessageRecived?.Invoke(this, message);
        }

        /// <summary>
        /// Publishes a message to all connected clients.
        /// HINT : EntryPoint for the dispatcher.
        /// </summary>
        public void Publish<TMessage>(TMessage message, TaskPriority priority = TaskPriority.Normal, List<IPipeClient> pipeClients = null)
        {
            try
            {
                if (!_initialized)
                {
                    Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Publish: Dispatcher not initialized");
                    throw new Exception(message: "PipeDispatcher: Publish: Dispatcher not initialized");
                }
                if (message == null)
                {
                    if (Logger != null)
                    {
                        Logger.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Publish: message is null");
                        throw new ArgumentNullException(nameof(message));

                    }
                    else
                        throw new ArgumentNullException(nameof(message));
                }

                var envelope = new MessageEnvelope
                {
                    TypeName = typeof(TMessage).AssemblyQualifiedName,
                    JsonPayload = System.Text.Json.JsonSerializer.Serialize(message)
                };

                if (envelope.JsonPayload != string.Empty)
                    this.onMessageSent(envelope.JsonPayload);

                string serializedEnvelope = System.Text.Json.JsonSerializer.Serialize(envelope);

                if (_isServer)
                    _server.Value.Enqueue(_ => _server.Key.SendAsync(serializedEnvelope), priority);
                else
                {
                    //TODO: Execute for all clients
                    if (pipeClients == null)
                    {
                        var client = _clients.FirstOrDefault();
                        client.Value.Enqueue(_ => client.Key.SendAsync(serializedEnvelope), priority);

                    }
                    else
                    {
                        var clients = _clients.Where(x => pipeClients.Any(z => z.Id == x.Key.Id)).ToList();

                        foreach (var client in clients)
                        {
                            client.Value.Enqueue(_ => client.Key.SendAsync(serializedEnvelope), priority);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: AddClient: {ex.Message}");
                throw;
            }

        }
        /// <summary>
        /// Dispatches an envelope to the appropriate handler.
        /// </summary>
        internal void DispatchEnvelope(string json)
        {
            try
            {
                if (json != string.Empty)
                    this.onMessageRecived(json);

                _asyncDispatcher.Enqueue(_ => _registry.DispatchEnvelopeAsync(json), TaskPriority.High);
                Logger?.Log(PipeDispatcherLoggingLevels.Success, $"[PipeDispatcher] Dispatched envelope: {json}");
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"[PipeDispatcher] Dispatch failed: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Subscribes a handler for incoming messages of type TMessage.
        /// HINT : EntryPoint for Subscribtion.
        /// </summary>
        public IDisposable Subscribe<TMessage>(Func<TMessage, Task> handler, Guid tag)
        {
            if (!_initialized)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Subscribe: Dispatcher not initialized");
                return null;
            }
            var x = _registry.Subscribe(handler, tag);

            if (x == null)
                throw new ArgumentNullException(nameof(handler));
            else
                return x;

        }
        /// <summary>
        /// Refreshes the dispatcher, disposing of the current instance and creating a new one.
        /// </summary>
        public void Refresh()
        {
            this.Dispose();
            _instance = new Lazy<PipeDispatcher>(() => new PipeDispatcher());
            _disposed = false;
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"PipeDispatcher: Refresh: Dispatcher refreshed");
        }
        /// <summary>
        /// Cleans up resources and unsubscribes all handlers.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_disposed) return;

                _registry.Dispose();
                if (_isServer)
                {
                    _server.Key.Dispose();
                    _server.Value.Dispose();
                }
                foreach (var client in _clients)
                {
                    client.Key.Dispose();
                    client.Value.Dispose();
                }

                _clients.Clear();

                _asyncDispatcher.Dispose();

                _disposed = true;
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Dispose: {ex.Message}");
            }

        }

        /// <summary>
        /// Logs the connection status of the dispatcher and connected clients.
        /// </summary>
        public void DumpStatus()
        {
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"PipeDispatcher: Status Dump");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Server: {_server.Key?.Id} Connected: {_server.Key?.IsConnected}");

            foreach (var kvp in _clients)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.System, $"Client: {kvp.Key.Id} Connected: {kvp.Key.IsConnected}");
            }
        }
    }
}
