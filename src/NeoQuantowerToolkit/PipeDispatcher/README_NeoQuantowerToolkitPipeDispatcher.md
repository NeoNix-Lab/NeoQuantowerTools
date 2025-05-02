# Neo.Quantower.Toolkit.PipeDispatcher

A modular and asynchronous message dispatch system using .NET Named Pipes, designed for intra-process or inter-process communication within Quantower modules or external tools.

## ✅ Features

- Supports publish/subscribe messaging pattern
- Message type-safe dispatching using generics
- Full client/server fallback logic
- Clean serialization via `System.Text.Json`
- Simple registration of handlers with automatic unsubscription (`IDisposable`)
- Optional logging action

## 🧱 Components

- `PipeDispatcher`: Singleton coordinator. Handles subscriptions, publishing and fallback to client/server roles.
- `PipeServer` / `PipeClient`: Manages bidirectional communication via `NamedPipeServerStream` and `NamedPipeClientStream`.
- `DispatcherRegistry`: Internal handler container per message type.
- `MessageEnvelope`: Typed message container serialized in JSON.

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

## 🔒 Thread Safety

All core components are thread-safe via `ConcurrentDictionary`, `ImmutableHashSet`, and async-safe dispatching.

## 🔄 Lifecycle

- Automatically attempts to become a server if possible.
- Falls back to a client role if the pipe name is already occupied.
- Pipe name defaults to `NeoDispatcherPipe`.

## 🧪 Recommended Tests

- Message flow and handler invocation
- Connection interruption and reconnection
- Dispose of subscriptions under load
- Serialize/deserialize nested objects

## 📦 Ideal for

- Event-driven messaging between Quantower scripts
- Decoupled module communication
- Logging or debugging event routing
