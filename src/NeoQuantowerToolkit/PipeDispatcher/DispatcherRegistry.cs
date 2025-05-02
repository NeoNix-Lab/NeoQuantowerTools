// Neo.Quantower.Toolkit.PipeDispatcher
// DispatcherRegistry - Handles subscription and dynamic dispatching of incoming messages

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
        private readonly ConcurrentDictionary<string, ImmutableHashSet<Func<object, Task>>> _handlers = new();
        private readonly ConcurrentDictionary<Subscription, (string typeName, Func<object, Task>)> _subscriptions = new();
        private bool _disposed;

        private Action<string> Logger;

        public ConcurrentDictionary<Subscription, (string typeName, Func<object, Task>)> SubscriptionsDictionary => this._subscriptions;
        public int SubsCount => this._subscriptions.Keys.Count;
        public List<Subscription> Subscriptions => this.SubscriptionsDictionary.Keys.ToList();

        public DispatcherRegistry(Action<string> Logger)
        {
            this.Logger = Logger;
        }

        public IDisposable Subscribe<TMessage>(Func<TMessage, Task> handler, Guid tag)
        {
            if (handler == null)
                return null;


            string typeName = typeof(TMessage).AssemblyQualifiedName;
            var wrapper = new Func<object, Task>(obj => handler((TMessage)obj));

            var subscription = new Subscription(() => RemoveHandler(typeName, wrapper), tag);
            if (_subscriptions.ContainsKey(subscription))
            {
                Logger?.Invoke($"Subscription already exists for {typeName} and Guid {tag}");
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

        private void RemoveHandler(string typeName, Func<object, Task> handler)
        {
            _handlers.AddOrUpdate(
                typeName,
                ImmutableHashSet<Func<object, Task>>.Empty,
                (_, existing) => existing.Remove(handler)
            );
        }

        public void Unsubscribe(Guid tag)
        {
            var subscription = _subscriptions.Keys.FirstOrDefault(s => s.Guid == tag);

            if (subscription == null)
            {
                Logger?.Invoke($"No subscription found for Guid {tag}");
                throw new ArgumentNullException(nameof(subscription));
            }

            if (_subscriptions.TryRemove(subscription, out var kvp))
            {
                var (typeName, handler) = kvp;
                RemoveHandler(typeName, handler);
            }
        }


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

    public class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private readonly Guid _guid;
        private bool _disposed;

        public bool Disposed => this._disposed;
        public Guid Guid => this._guid;

        public Subscription(Action unsubscribe, Guid guid)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _unsubscribe();
                _disposed = true;
            }
        }
    }
}
