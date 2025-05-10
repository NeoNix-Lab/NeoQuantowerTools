using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Neo.Quantower.Abstractions.Factories
{
    public class AsyncTaskQueueFactory : IAsyncTaskQueueFactory
    {
        private readonly ICustomLogger<TaskResoult>? _logger;
        private readonly int _maxLength;
        private readonly int _maxRetries;
        private readonly TimeSpan _timeout;


        public AsyncTaskQueueFactory(
            Action<string>? logger = null,
            int maxLength = 100,
            int maxRetries = 3,
            TimeSpan? timeout = null)
        {
            _logger = new InternalLogger(logger);
            _maxLength = maxLength;
            _maxRetries = maxRetries;
            _timeout = timeout ?? TimeSpan.FromSeconds(10);
        }
        /// <summary>
        /// Returns a new instance of AsyncTaskQueue with the specified Name base for his Guid.
        /// </summary>
        /// <param name="name">String for Guid</param>
        /// <returns></returns>
        public AsyncTaskQueue Create(string name)
        {
            return new AsyncTaskQueue(_logger, GetGuidFromName(name))
            {
                MaxQueueLength = _maxLength,
                MaxRetryAttempts = _maxRetries,
                TaskTimeout = _timeout
            };
        }
        /// <summary>
        /// Returns a new instance of AsyncTaskQueue with the specified Guid.
        /// </summary>
        /// <param name="name">Guid Id</param>
        /// <returns></returns>
        public AsyncTaskQueue Create(Guid name)
        {
            return new AsyncTaskQueue(_logger, name)
            {
                MaxQueueLength = _maxLength,
                MaxRetryAttempts = _maxRetries,
                TaskTimeout = _timeout
            };
        }

        private static Guid GetGuidFromName(string input)
        {
            using var md5 = MD5.Create();
            return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
        /// <summary>
        /// Default logger for AsyncTaskQueue
        /// </summary>
        internal struct InternalLogger : ICustomLogger<TaskResoult>
        {
            public Action<string> Logger { get; }

            public bool IsNull => Logger.Equals(null);

            public InternalLogger(Action<string>? logger )
            {
                Logger = logger ?? this.InternalLogAction;
            }

            private void InternalLogAction(string message)
            {
                Console.WriteLine(message);
            }

            public void Log(TaskResoult result, string message)
            {
                Logger?.Invoke($"[{result}] {message}");
                
            }
        }
    }

    public static class AsyncTaskQueueDefaultFactories
    {
        /// <summary>
        /// Creates a default async task queue factory for server-side streaming environments
        /// (e.g., NamedPipeServerStream clients).
        /// </summary>
        public static AsyncTaskQueueFactory ForServerStreams(Action<string>? logger = null)
            => new(logger, maxLength: 100, maxRetries: 1, timeout: TimeSpan.FromSeconds(30));
        /// <summary>
        /// Returns a default async task queue factory for Dispatcher streams.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static AsyncTaskQueueFactory ForDispatcherStreams(Action<string>? logger = null)
            => new(logger, maxLength: 256, maxRetries: 3, timeout: TimeSpan.FromSeconds(10));
    }
}
