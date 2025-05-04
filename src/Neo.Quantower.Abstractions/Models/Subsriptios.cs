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

        public bool Disposed => this._disposed;
        public Guid Guid => this._guid;

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
