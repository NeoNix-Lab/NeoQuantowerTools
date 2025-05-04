// Neo.Quantower.Toolkit.PipeDispatcher
// PipeDispatcher - Core for NamedPipe-based messaging

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
        private static Lazy<PipeDispatcher> _instance = new(() => new PipeDispatcher());
        public static PipeDispatcher Instance => _instance.Value;
        public ICustomLogger<PipeDispatcherLoggingLevels> Logger { get; private set; }

        private PipeServer _server;
        private List<PipeClient> _clients = new();
        private DispatcherRegistry _registry;
        private string _pipeName;
        private bool _isServer;
        private bool _disposed;
        private bool _inizialized;
        private bool _isConnected;

        public event EventHandler<string> MessageSent;
        public event EventHandler<string> MessageRecived;

        /// <summary>
        /// TODO:Gets a read-only view of all subscribed handler counts by message type and more.
        /// </summary>
        public int SubscribedHandlerCounts => _registry.SubsCount;
        public int MaxClients { get; protected set; }
        public List<Subscription> Subscriptions => _registry.Subscriptions;
        public string PipeName => _pipeName;
        public bool IsInitialized => _inizialized;
        public bool IsServer => _isServer;
        public bool IsRegistred => _registry.Subscriptions.Any();
        public bool GetConneccionStatus() => _isServer 
            ? _server.IsConnected 
            : _clients.Any(x => x.IsConnected);


        private PipeDispatcher()
        {
            _inizialized = false;
            _disposed = false;
        }

        public void UnscribeSubscription(Guid subscription) => _registry.Unsubscribe(subscription);

        /// <summary>
        /// Initializes the dispatcher, setting up the NamedPipe server or client.
        /// TODO : Add error handling for connection issues.
        /// TODO : Define Logging Levels And Verbose.
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="logger"></param>

        public async Task Initialize(string pipeName = "NeoQuantowerDispatcher", ICustomLogger<PipeDispatcherLoggingLevels> logger = null, int maxClients = 10)
        {
            this.MaxClients = maxClients;
            _pipeName = pipeName;
            Logger = logger;
            _registry = new DispatcherRegistry(logger);

            if (_clients.Count < maxClients)
            {
                try
                {
                    _server = new PipeServer(_pipeName, logger);
                    await _server.StartAsync();
                    _isServer = true;
                    _inizialized = true;
                }
                catch
                {
                    if (!_clients.Any(x => x.PipeName == pipeName))
                    {
                        PipeClient _client = new PipeClient(_pipeName, logger);
                        _clients.Add(_client);
                        await _client.ConnectAsync();
                        _isServer = false;
                        _inizialized = true;
                    }
                    else
                    {
                        Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Initialize: Server not found, trying to connect as client with exisisting name");
                        throw new Exception(message: "PipeDispatcher: Initialize: Server not found, trying to connect as client with exisisting name");
                    }

                }
            }
            else
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Initialize: Max clients reached");
                throw new Exception(message: "PipeDispatcher: Initialize: Max clients reached");
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
        public async Task PublishAsync<TMessage>(TMessage message)
        {

            if (!_inizialized)
            {
                Logger?.Log( PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Publish: Dispatcher not initialized");
                throw new Exception(message: "PipeDispatcher: Publish: Dispatcher not initialized");
            }
            if (message == null)
            {
                if (Logger != null)
                {
                    Logger.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: PublishAsync: message is null");
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
                await _server.SendAsync(serializedEnvelope);
            else
                //TODO: Execute for all clients
                await _clients.First().SendAsync(serializedEnvelope);
        }

        /// <summary>
        /// Dispatches an envelope to the appropriate handler.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        internal async Task DispatchEnvelopeAsync(string json)
        {
            try
            {
                if (json != string.Empty)
                    this.onMessageRecived(json);

                await _registry.DispatchEnvelopeAsync(json);
                Logger?.Log(PipeDispatcherLoggingLevels.Success, $"[PipeDispatcher] Dispatched envelope: {json}");
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"[PipeDispatcher] Dispatch failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Subscribes a handler for incoming messages of type TMessage.
        /// HINT : EntryPoint for Subscribtion.
        /// </summary>
        public IDisposable Subscribe<TMessage>(Func<TMessage, Task> handler, Guid tag)
        {
            if (!_inizialized)
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

        public void Refresh()
        {
            this.Dispose();
            _instance = new Lazy<PipeDispatcher>(() => new PipeDispatcher());
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
                _server?.Dispose();
                foreach (var client in _clients)
                {
                    client?.Dispose();
                }
                _disposed = true;
            }
            catch (Exception ex)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"PipeDispatcher: Dispose: {ex.Message}");
            }

        }

        public void DumpStatus()
        {
            Logger?.Log(PipeDispatcherLoggingLevels.System, " --- PipeDispatcher Status ---");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Initialized: {IsInitialized}");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Mode: {(IsServer ? "Server" : "Client")}");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Pipe: {PipeName}");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Clients Count: {_clients?.Count}");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Server Connected: {_server?.ToString()}");
            Logger?.Log(PipeDispatcherLoggingLevels.System, $"Subscription Count: {_registry?.SubsCount}");
            foreach (var kvp in _registry?.SubscriptionsDictionary)
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $" -{kvp.Key} with Guid: {kvp.Key.Guid}: {kvp.Value} subscribers");
        }

    }
}
