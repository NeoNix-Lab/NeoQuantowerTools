// Neo.Quantower.Toolkit.PipeDispatcher
// DispatcherRegistry - Handles subscription and dynamic dispatching of incoming messages

using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.PipeDispatcher
{
    internal sealed class DispatcherRegistry : IDisposable
    {
        /// <summary>
        /// Retains the handlers for each message type.
        /// </summary>
        private readonly ConcurrentDictionary<string, ImmutableHashSet<Func<object, Task>>> _handlers = new();
        /// <summary>
        /// Retains the subscriptions for each message type.
        /// </summary>
        private readonly ConcurrentDictionary<Subscription, (string typeName, Func<object, Task>)> _subscriptions = new();
        private bool _disposed;
        /// <summary>
        /// Custom logger for logging messages.
        /// </summary>
        private ICustomLogger<PipeDispatcherLoggingLevels> Logger;
        /// <summary>
        /// Returns the handlers for each message type.
        /// </summary>
        public ConcurrentDictionary<Subscription, (string typeName, Func<object, Task>)> SubscriptionsDictionary => this._subscriptions;
        /// <summary>
        /// Returns the number of subscribed handlers.
        /// </summary>
        public int SubsCount => this._subscriptions.Keys.Count;
        /// <summary>
        /// Returns the list of subscribed handlers.
        /// </summary>
        public List<Subscription> Subscriptions => this.SubscriptionsDictionary.Keys.ToList();
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Logger"></param>
        public DispatcherRegistry(ICustomLogger<PipeDispatcherLoggingLevels> Logger)
        {
            this.Logger = Logger;
        }
        /// <summary>
        /// Subscribes a handler for a specific message type.
        /// </summary>
        /// <typeparam name="TMessage">Type of Message</typeparam>
        /// <param name="handler">Action</param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public IDisposable Subscribe<TMessage>(Func<TMessage, Task> handler, Guid tag)
        {
            if (handler == null)
                return null;


            string typeName = typeof(TMessage).AssemblyQualifiedName;
            var wrapper = new Func<object, Task>(obj => handler((TMessage)obj));

            var subscription = new Subscription(() => RemoveHandler(typeName, wrapper), tag);
            if (_subscriptions.ContainsKey(subscription))
            {
                Logger?.Log(PipeDispatcherLoggingLevels.System,$"Subscription already exists for {typeName} and Guid {tag}");
               return null;
            }
            _handlers.AddOrUpdate(
                typeName,
                ImmutableHashSet.Create(wrapper),
                (_, existing) => existing.Add(wrapper)
            );

            _subscriptions[subscription] = (typeName, wrapper);
            return subscription;
        }
        /// <summary>
        /// Dispatches the envelope to the appropriate handlers.
        /// </summary>
        /// <param name="envelopeJson"></param>
        /// <returns></returns>
        public async Task DispatchEnvelopeAsync(string envelopeJson)
        {
            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(envelopeJson);
            if (envelope == null || string.IsNullOrEmpty(envelope.TypeName))
                return;

            if (!_handlers.TryGetValue(envelope.TypeName, out var bag))
                return;

            var messageType = Type.GetType(envelope.TypeName);
            if (messageType == null)
                return;

            var payload = JsonSerializer.Deserialize(envelope.JsonPayload, messageType);

            foreach (var handler in bag)
            {
                await handler(payload);
            }
        }
        /// <summary>
        /// Remove a specific handler from the list of handlers.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="handler"></param>
        private void RemoveHandler(string typeName, Func<object, Task> handler)
        {
            _handlers.AddOrUpdate(
                typeName,
                ImmutableHashSet<Func<object, Task>>.Empty,
                (_, existing) => existing.Remove(handler)
            );
        }
        /// <summary>
        /// Remove a specific set of handlers from the list of handlers.
        /// </summary>
        /// <param name="tag"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Unsubscribe(Guid tag)
        {
            var subscription = _subscriptions.Keys.FirstOrDefault(s => s.Guid == tag);

            if (subscription == null)
            {
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"No subscription found for Guid {tag}");
                throw new ArgumentNullException(nameof(subscription));
            }

            if (_subscriptions.TryRemove(subscription, out var kvp))
            {
                var (typeName, handler) = kvp;
                RemoveHandler(typeName, handler);
            }
        }
        /// <summary>
        /// Unsubscribes all subsriptions.
        /// </summary>
        public void UnsubscribeAll()
        {
            foreach (var kvp in _subscriptions)
            {
                var (typeName, handler) = kvp.Value;
                RemoveHandler(typeName, handler);
            }
            _subscriptions.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            UnsubscribeAll();
            _disposed = true;
        }
    }

    
}
