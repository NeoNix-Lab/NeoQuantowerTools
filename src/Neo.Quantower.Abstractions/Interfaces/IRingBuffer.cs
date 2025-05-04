using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Interfaces
{
    /// <summary>
    /// Interface for a generic circular buffer with logical indexing and directional iteration support.
    /// </summary>
    /// <typeparam name="T">The value type stored in the buffer.</typeparam>
    internal interface IRingBuffer<T> where T : struct
    {
        /// <summary>
        /// Adds a new item to the buffer, possibly overwriting the oldest entry.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void Add(T item);

        /// <summary>
        /// Gets the item at the specified logical index.
        /// </summary>
        /// <param name="index">The logical index of the item.</param>
        /// <returns>The element at the given logical index.</returns>
        T this[int index] { get; }

        /// <summary>
        /// Gets the number of valid items currently stored in the buffer.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Clears the buffer and resets all internal counters.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns all items in the buffer in logical order, according to the current direction setting.
        /// </summary>
        IEnumerable<T> GetItems();

        /// <summary>
        /// Returns the items in logical order, sorted by the given key selector.
        /// </summary>
        /// <typeparam name="TKey">The type of the sorting key.</typeparam>
        /// <param name="selector">A function to extract the sorting key.</param>
        /// <returns>An ordered sequence of elements.</returns>
        IEnumerable<T> GetOrderBy<TKey>(Func<T, TKey> selector) where TKey : IComparable<TKey>;
    }
}
