using Neo.Quantower.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeoQuantowerToolkit.Abstractions
{
    public enum PipeDispatcherStatus
    {
        Requested,
        Finded,
        Lost,
        Error
    }
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
        public static PipeDispatcherStatus Status ;
        private static Action<string> Logger { get; set; }


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
                    Logger?.Invoke("PipeDispatcher.Instance not found");
                }
                else
                {
                    var instance = instanceProp.GetValue(null) as IPipeDispatcher;
                    if (instance != null)
                    {
                        Status = PipeDispatcherStatus.Finded;
                        RegisterDispatcher(instance);
                        Logger?.Invoke("Dispatcher registered successfully.");
                    }
                    else
                        Status = PipeDispatcherStatus.Lost;
                }
            }
            catch (Exception ex)
            {
                Status = PipeDispatcherStatus.Lost;
                Logger?.Invoke($"[PipeFactory] Initialization failed: {ex.Message}");
            }

        }
    }
}
