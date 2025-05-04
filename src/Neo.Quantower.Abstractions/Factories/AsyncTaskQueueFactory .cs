using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
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
            ICustomLogger<TaskResoult>? logger = null,
            int maxLength = 100,
            int maxRetries = 3,
            TimeSpan? timeout = null)
        {
            _logger = logger;
            _maxLength = maxLength;
            _maxRetries = maxRetries;
            _timeout = timeout ?? TimeSpan.FromSeconds(30);
        }

        public AsyncTaskQueue Create(string name)
        {
            return new AsyncTaskQueue(_logger, GetGuidFromName(name))
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
    }

    public static class AsyncTaskQueueFactories
    {
        /// <summary>
        /// Creates a default async task queue factory for server-side streaming environments
        /// (e.g., NamedPipeServerStream clients).
        /// </summary>
        public static AsyncTaskQueueFactory ForServerStreams(ICustomLogger<TaskResoult>? logger = null)
            => new(logger, maxLength: 256, maxRetries: 1, timeout: TimeSpan.FromSeconds(10));
    }
}
