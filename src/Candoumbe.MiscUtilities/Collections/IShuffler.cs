// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Collections
{
    /// <summary>
    /// Contrat of algorithm that can shuffle a collection
    /// </summary>
    /// <typeparam name="T">Type of items that the collection to shuffle contains.</typeparam>
    public interface IShuffler<T>
    {
        /// <summary>
        /// Creates a new collection which content is a shuffled representation of <paramref name="original"/>'s content
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        IEnumerable<T> Shuffle(IEnumerable<T> original);
    }
}
