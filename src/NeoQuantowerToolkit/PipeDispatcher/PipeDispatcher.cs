// Neo.Quantower.Toolkit.PipeDispatcher
// PipeDispatcher - Core for NamedPipe-based messaging

using Neo.Quantower.Abstractions;
using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Neo.Quantower.Toolkit.PipeDispatcher.DispatcherRegistry;

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
        private Action<string> Logger { get; set; }

        private PipeServer _server;
        private PipeClient _client;
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
        public List<Subscription> Subscriptions => _registry.Subscriptions;
        public string PipeName => _pipeName;
        public bool IsInitialized => _inizialized;
        public bool IsServer => _isServer;
        public bool IsRegistred => _registry.Subscriptions.Any();
        public bool GetConneccionStatus() => _isServer ? _server.IsConnected : _client.IsConnected;


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
        public async Task Initialize(string pipeName = "NeoQuantowerDispatcher", Action<string> logger = null)
        {
            _pipeName = pipeName;
            Logger = logger;
            _registry = new DispatcherRegistry(logger);

            try
            {
                _server = new PipeServer(_pipeName, logger);
                await _server.StartAsync();
                _isServer = true;
                _inizialized = true;
            }
            catch
            {
                _client = new PipeClient(_pipeName, logger);
                await _client.ConnectAsync();
                _isServer = false;
                _inizialized = true;
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
                Logger?.Invoke($"PipeDispatcher: Publish: Dispatcher not initialized");
                throw new Exception(message: "PipeDispatcher: Publish: Dispatcher not initialized");
            }
            if (message == null)
            {
                if (Logger != null)
                {
                    Logger.Invoke($"PipeDispatcher: PublishAsync: message is null");
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
                await _client.SendAsync(serializedEnvelope);
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
                Logger?.Invoke($"[PipeDispatcher] Dispatched envelope: {json}");
            }
            catch (Exception ex)
            {
                Logger?.Invoke($"[PipeDispatcher] Dispatch failed: {ex.Message}");
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
                Logger?.Invoke($"PipeDispatcher: Subscribe: Dispatcher not initialized");
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
            Logger?.Invoke($"PipeDispatcher: Refresh: Dispatcher refreshed");
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
                _client?.Dispose();
                _disposed = true;
            }
            catch (Exception ex)
            {
                Logger?.Invoke($"PipeDispatcher: Dispose: {ex.Message}");
            }

        }

        public void DumpStatus()
        {
            Logger?.Invoke(" --- PipeDispatcher Status ---");
            Logger?.Invoke($"Initialized: {IsInitialized}");
            Logger?.Invoke($"Mode: {(IsServer ? "Server" : "Client")}");
            Logger?.Invoke($"Pipe: {PipeName}");
            Logger?.Invoke($"Clients Connected: {_client?.ToString()}");
            Logger?.Invoke($"Clients Connected: {_server?.ToString()}");
            Logger?.Invoke($"Subscription Count: {_registry?.SubsCount}");
            foreach (var kvp in _registry?.SubscriptionsDictionary)
                Logger?.Invoke($" -{kvp.Key} with Guid: {kvp.Key.Guid}: {kvp.Value} subscribers");
        }

    }
}
