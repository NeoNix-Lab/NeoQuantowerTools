# Future Implementations for Neo.Quantower.Toolkit

This document outlines planned and proposed future features for:
- `Neo.Quantower.Toolkit.PipeDispatcher`
- `Neo.Quantower.Toolkit.AsyncTaskQueue`

---

## 🧠 PipeDispatcher - Planned Extensions

### ♻️ Automatic reconnects
- Smarter reconnection/backoff logic if a client is disconnected unexpectedly.

### 🧪 Testing framework
- Integration test harness to simulate multiple clients.

---

## ⚙️ AsyncTaskQueue - Planned Features

### 🎯 Result-based Task completion
- Allow tasks to return results and expose them via event or `Task<T>` wrappers.

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

