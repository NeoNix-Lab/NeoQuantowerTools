using Neo.Quantower.Abstractions.Interfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Models
{
    /// <summary>
    /// A fixed-size circular buffer (ring buffer) with logical indexing and directional access.
    /// </summary>
    /// <typeparam name="T">The value type stored in the buffer.</typeparam>
    public class RingBuffer<T> : IRingBuffer<T> where T : struct
    {
        private readonly int _size;
        private readonly ArrayPool<T> _pool = ArrayPool<T>.Shared;
        private readonly T[] _buffer;
        private int _head;
        private int _count;

        /// <summary>
        /// Determines whether the buffer should be read from oldest to newest (true) or from newest to oldest (false).
        /// </summary>
        public bool FromBeginning { get; private set; }

        /// <inheritdoc/>
        public int Count => _count;

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");

                int realIndex = FromBeginning
                    ? (_head - _count + index + _size) % _size
                    : (_head - 1 - index + _size) % _size;

                return _buffer[realIndex];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBuffer{T}"/> class with a fixed capacity.
        /// </summary>
        /// <param name="length">The maximum number of items the buffer can store.</param>
        /// <param name="fromBeginning">Whether to iterate from the oldest (true) or most recent (false).</param>
        public RingBuffer(int length, bool fromBeginning = true)
        {
            _size = length;
            _buffer = _pool.Rent(length);
            _head = 0;
            _count = 0;
            FromBeginning = fromBeginning;
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _size;
            if (_count < _size)
                _count++;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _head = 0;
            _count = 0;
        }

        /// <summary>
        /// Sets the logical read direction of the buffer.
        /// </summary>
        /// <param name="fromBeginning">
        /// If true, logical indexing will start from the oldest item.  
        /// If false, logical indexing will start from the most recent item.
        /// </param>
        public void SetFromBeginning(bool fromBeginning) => FromBeginning = fromBeginning;

        /// <summary>
        /// Gets an item by logical index, supporting negative values.
        /// </summary>
        /// <param name="index">
        /// The logical index: 0 means oldest if <see cref="FromBeginning"/> is true, or newest if false.
        /// </param>
        /// <returns>The value at the specified logical index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the index is out of bounds.</exception>
        public T GetWithReversal(int index)
        {
            if (index < -_count || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");

            int realIndex = FromBeginning
                ? (_head - _count + index + _size) % _size
                : (_head - 1 - index + _size) % _size;

            return _buffer[realIndex];
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetItems()
        {
            if (_count == 0)
                throw new ArgumentOutOfRangeException(nameof(_buffer), "The buffer is empty.");

            if (FromBeginning)
            {
                for (int i = 0; i < _count; i++)
                    yield return this[i];
            }
            else
            {
                for (int i = _count - 1; i >= 0; i--)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Returns a logical range of items between two indices.
        /// </summary>
        /// <param name="from">The start index (inclusive).</param>
        /// <param name="to">The end index (exclusive).</param>
        /// <returns>A sequence of elements in the specified logical range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the range is invalid or the buffer is empty.</exception>
        public IEnumerable<T> GetRange(int from, int to)
        {
            if (_count <= 0 || to > _count || from < 0 || from > to)
                throw new ArgumentOutOfRangeException(nameof(_buffer), "Invalid range or empty buffer.");

            if (FromBeginning)
            {
                for (int i = from; i < to; i++)
                    yield return this[i];
            }
            else
            {
                int delta = to - from;
                for (int i = _count - 1; i >= delta; i--)
                    yield return this[i];
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetOrderBy<TKey>(Func<T, TKey> selector) where TKey : IComparable<TKey>
        {
            return GetItems().OrderBy(selector);
        }
    }
}
