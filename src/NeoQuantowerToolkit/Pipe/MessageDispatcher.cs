using Neo.Quantower.Toolkit.PipeDispatcher;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Pipe
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IPipeTransport _transport;
        private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _handlers = new();
        private readonly JsonSerializerOptions _jsonOptions;

        public MessageDispatcher(IPipeTransport transport)
        {
            _transport = transport;
            _transport.MessageReceived += OnTransportMessageReceived;
            _jsonOptions = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                WriteIndented = false
            };
        }

        public Task InitializeAsServerAsync(string pipeName, CancellationToken token = default)
            => _transport.StartServerAsync(pipeName, token);

        public Task InitializeAsClientAsync(string pipeName, CancellationToken token = default)
            => _transport.ConnectAsync(pipeName, token);

        public async Task PublishAsync<T>(T message)
        {
            var envelope = new MessageEnvelope
            {
                TypeName = typeof(T).AssemblyQualifiedName!,
                JsonPayload = JsonSerializer.Serialize(message, _jsonOptions)
            };
            string json = JsonSerializer.Serialize(envelope, _jsonOptions);
            await _transport.SendAsync(json);
        }

        public IDisposable Subscribe<T>(Func<T, Task> handler)
        {
            string typeName = typeof(T).AssemblyQualifiedName!;
            Func<object, Task> wrapper = obj => handler((T)obj);
            _handlers.AddOrUpdate(typeName,
                _ => new List<Func<object, Task>> { wrapper },
                (_, list) => { list.Add(wrapper); return list; });
            return new Unsubscriber(_handlers, typeName, wrapper);
        }

        private void OnTransportMessageReceived(object? sender, string json)
        {
            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(json, _jsonOptions);
            if (envelope == null) return;
            if (!_handlers.TryGetValue(envelope.TypeName, out var list)) return;
            object? payload = JsonSerializer.Deserialize(envelope.JsonPayload, Type.GetType(envelope.TypeName!)!, _jsonOptions);
            if (payload == null) return;
            foreach (var h in list)
            {
                _ = h(payload);
            }
        }

        public void Dispose() => _transport.Dispose();

        private class Unsubscriber : IDisposable
        {
            private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _dict;
            private readonly string _key;
            private readonly Func<object, Task> _handler;

            public Unsubscriber(ConcurrentDictionary<string, List<Func<object, Task>>> dict, string key, Func<object, Task> handler)
            {
                _dict = dict;
                _key = key;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_dict.TryGetValue(_key, out var list))
                    list.Remove(_handler);
            }
        }
    }
}
