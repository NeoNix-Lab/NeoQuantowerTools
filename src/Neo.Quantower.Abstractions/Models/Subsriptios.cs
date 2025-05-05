using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Models
{
    public class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private readonly Guid _guid;
        private bool _disposed;
        /// <summary>
        /// Dispose flag
        /// </summary>
        public bool Disposed => this._disposed;
        /// <summary>
        /// Subscription Guid
        /// </summary>
        public Guid Guid => this._guid;
        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="unsubscribe"></param>
        /// <param name="guid"></param>
        public Subscription(Action unsubscribe, Guid guid)
        {
            _guid = guid;
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _unsubscribe();
                _disposed = true;
            }
        }
    }
}
