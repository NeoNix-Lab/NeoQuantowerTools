# Neo.Quantower.Toolkit.PipeDispatcher

A modular and asynchronous message dispatch system using .NET Named Pipes, designed for intra-process or inter-process communication within Quantower modules or external tools.

---

## ✅ Features

- Supports publish/subscribe messaging pattern
- Message type-safe dispatching using generics
- Full client/server fallback logic
- Clean serialization via `System.Text.Json`
- Simple registration of handlers with automatic unsubscription (`IDisposable`)
- Optional logging action
- **Built on `AsyncTaskQueue` for asynchronous, prioritized message handling**

---

## 🧱 Components

- `PipeDispatcher`: Singleton coordinator. Handles subscriptions, publishing and fallback to client/server roles.
- `PipeServer` / `PipeClient`: Manages bidirectional communication via `NamedPipeServerStream` and `NamedPipeClientStream`.
- `DispatcherRegistry`: Internal handler container per message type.
- `AsyncTaskQueue`: Powers each client and server connection with a prioritized, retryable async queue.
- `MessageEnvelope`: Typed message container serialized in JSON.

---

## 🚀 Example usage

```csharp
// At application startup:
PipeDispatcher.Initialize();

// Subscribe to a message type
var subscription = PipeDispatcher.Subscribe<MyMessage>(msg =>
{
    Console.WriteLine($"Received: {msg.Text}");
});

// Publish a message (async fire-and-forget)
await PipeDispatcher.PublishAsync(new MyMessage { Text = "Hello Neo!" });

// When done
subscription.Dispose();
```

---

## 🔄 How It Works

When the dispatcher is initialized:

- It attempts to create a `PipeServer`, and if successful, it acts as the server.
- Each client and server uses an instance of `AsyncTaskQueue` to manage task execution asynchronously and in priority order.
- Messages are serialized, wrapped in `MessageEnvelope`, and routed to subscribers via the internal `DispatcherRegistry`.

---

## 🔒 Thread Safety

All core components are thread-safe via `ConcurrentDictionary`, `ImmutableHashSet`, and async-safe dispatching via `AsyncTaskQueue`.

---

## 🧪 Recommended Tests

- Message flow and handler invocation
- Connection interruption and reconnection
- Dispose of subscriptions under load
- Serialize/deserialize nested objects

---

## 📦 Ideal for

- Event-driven messaging between Quantower scripts
- Decoupled module communication
- Logging or debugging event routing
- Asynchronous dispatch handling with automatic retry and timeouts (via `AsyncTaskQueue`)

---

## 📘 Related

See also: [AsyncTaskQueue README](./README_AsyncTaskQueue.md)
