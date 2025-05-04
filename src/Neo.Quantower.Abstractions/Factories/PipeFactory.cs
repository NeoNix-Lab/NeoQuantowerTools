using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neo.Quantower.Abstractions.Factories
{

    /// <summary>
    /// Factory centralizzata per la gestione dell'istanza singleton di IPipeDispatcher.
    /// Fornisce inizializzazione lazy e registrazione dinamica tramite runtime reflection.
    /// </summary>
    public static class PipeFactory
    {
        const string targetAssemblyName = "Neo.Quantower.Toolkit";
        const string dispatcherTypeName = "Neo.Quantower.Toolkit.PipeDispatcher";

        private static IPipeDispatcher? _dispatcher;
        private static bool _initialized = false;

        public static bool IsInitialized => _initialized;
        public static PipeDispatcherStatus Status;
        private static ICustomLogger<PipeDispatcherLoggingLevels>? Logger { get; set; }


        /// <summary>
        /// Ottiene l'istanza registrata di IPipeDispatcher, inizializzandola se necessario.
        /// </summary>
        public static IPipeDispatcher? Dispatcher
        {
            get
            {
                EnsureInitialized();
                return _dispatcher;
            }
        }

        /// <summary>
        /// Registra l'istanza concreta del dispatcher.
        /// </summary>
        public static void RegisterDispatcher(IPipeDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _initialized = dispatcher.IsInitialized;
        }

        private static void EnsureInitialized()
        {
            Status = PipeDispatcherStatus.Requested;
            try
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == targetAssemblyName);

                if (asm == null)
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"bin/{targetAssemblyName}.dll");
                    if (File.Exists(path))
                        asm = Assembly.LoadFrom(path);
                }

                // Trova il tipo e la proprietà statica
                var type = asm?.GetType(dispatcherTypeName);
                var instanceProp = type?.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);

                if (instanceProp == null)
                {
                    Logger?.Log(PipeDispatcherLoggingLevels.Error, "PipeDispatcher.Instance not found");
                }
                else
                {
                    var instance = instanceProp.GetValue(null) as IPipeDispatcher;
                    if (instance != null)
                    {
                        Status = PipeDispatcherStatus.Finded;
                        RegisterDispatcher(instance);
                        Logger?.Log(PipeDispatcherLoggingLevels.System, "Dispatcher registered successfully.");
                    }
                    else
                        Status = PipeDispatcherStatus.Lost;
                }
            }
            catch (Exception ex)
            {
                Status = PipeDispatcherStatus.Lost;
                Logger?.Log(PipeDispatcherLoggingLevels.Error, $"[PipeFactory] Initialization failed: {ex.Message}");
            }

        }
    }
}
