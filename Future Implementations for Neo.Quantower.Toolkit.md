# Future Implementations for Neo.Quantower.Toolkit

This document outlines planned and proposed future features for:
- `Neo.Quantower.Toolkit.PipeDispatcher`
- `Neo.Quantower.Toolkit.AsyncTaskQueue`

---

## 🧠 PipeDispatcher - Planned Extensions

### 🔁 Multi-client Pipe Server
- Support concurrent connections from multiple processes.
- Queue and dispatch messages per client.

### 🔐 Message Authentication (optional)
- Add support for signing messages with a shared secret or certificate.
- Validate incoming messages for integrity.

### 🧩 Message Topic Channels
- Enable topic-based routing (string topic names).
- `Subscribe<T>(string topic, ...)` and `PublishAsync(topic, payload)`.

### 💾 Logging/Telemetry
- Built-in tracing hooks for diagnostics.
- Allow optional injection of `ILogger`.

### ♻️ Automatic reconnects
- Smarter reconnection/backoff logic if a client is disconnected unexpectedly.

### 🧪 Testing framework
- Integration test harness to simulate multiple clients.

---

## ⚙️ AsyncTaskQueue - Planned Features

### 🎯 Result-based Task completion
- Allow tasks to return results and expose them via event or `Task<T>` wrappers.

### 🗓 Delayed and Scheduled Execution
- Enqueue task with a scheduled execution time.

### 📊 Queue Introspection APIs
- Expose read-only access to queued tasks count, by priority.
- `GetPendingCount(TaskPriority priority)`

### 🧵 Per-Priority Worker Threads
- Run separate worker loops per priority for guaranteed isolation.

### 📈 Metrics
- Track execution time, failures, retries, per-priority stats.

### 🧪 Built-in Test Harness
- Simulate enqueue bursts and controlled failure scenarios.

---

## 📅 Development Strategy
- New features will remain backward compatible.
- Separate branches for experimental additions.
- PRs and versioning will follow SemVer.

## 📬 Suggestions
Please open issues or discussions for any community-driven ideas.

