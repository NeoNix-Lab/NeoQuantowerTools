# Neo.Quantower.Toolkit

[![CI Status](https://github.com/NeoNix-Lab/NeoQuantowerTools/actions/workflows/ci.yml/badge.svg)](https://github.com/NeoQuantowerTools/actions/workflows/ci.yml)
[![Issues](https://img.shields.io/github/issues/NeoNix-Lab/NeoQuantowerTools.svg?style=flat-square)](https://github.com/NeoNix-Lab/NeoQuantowerTools/issues)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=flat-square)
 
An advanced, modular toolkit designed to extend Quantower indicators and strategies .NET 8.0 environments.

---

## 📦 Content

- **AsyncHelpers/AsyncTaskQueue.cs**: Asynchronous FIFO task management with priorities, retries, and timeouts.
- **Core/LoggerHelper.cs**: Simple wrapper over Quantower Core.Loggers for consistent logging.
---

## 🚀 Usage

1. Run `build.bat` to build the solution and create `.nupkg` NuGet package.
2. Install the package into your Visual Studio projects:
   - Open Visual Studio.
   - Manage NuGet Packages > Browse > Add Local Package.
3. Reference `Neo.Quantower.Toolkit` in your indicator or strategy projects.

✅ The toolkit supports automatic compatibility based on your project:
- For projects targeting `.NET Framework 4.7.2` ➔ Loads `net472` build.
- For projects targeting `.NET 8.0` ➔ Loads `net8.0` build.

---

## ⚙️ Quantower Compatibility

| Quantower Version | Compatibility |
|:------------------|:---------------|
| 1.142 and newer (uses .NET 8 templates) | ✅ Fully compatible |
| Versions below 1.140 (uses .NET Framework 4.7.2) | ✅ Fully compatible |

The toolkit can be safely used across both older and newer Quantower builds without modification.

---

## 📜 License

The toolkit is distributed under a simple attribution license:

- Free to use, modify, and distribute (including commercial use).
- Attribution to **"Neo"** must be maintained in any derived work.

> Licensed under Neo's Attribution License - Version 1.0.0.

---

## 🛡️ Notes

- Ensure `TradingPlatform.BusinessLayer.dll` from Quantower is properly referenced when working under `.NET Framework 4.7.2`.
- Under `.NET 8.0`, no manual referencing is needed for TradingPlatform.BusinessLayer unless Quantower changes the loading behavior in future updates.

---

**Neo.Quantower.Toolkit** — Built for performance, extensibility, and future-proof Quantower development.
