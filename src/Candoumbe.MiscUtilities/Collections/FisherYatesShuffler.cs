// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;
using System.Linq;
#if NET5_0_OR_GREATER
using System.Security.Cryptography;
#endif

namespace Candoumbe.MiscUtilities.Collections
{
    /// <summary>
    /// Shuffler based on the <see href="https://exceptionnotfound.net/understanding-the-fisher-yates-card-shuffling-algorithm/">"improved" Fisher-Yates algorithm</see>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FisherYatesShuffler<T> : IShuffler<T>
    {
#if !NET5_0_OR_GREATER
        private readonly Random _random = new();
#endif

        ///<inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Simplification", "RCS1179:Unnecessary assignment.")]
        public IEnumerable<T> Shuffle(IEnumerable<T> original)
        {
            IEnumerable<T> shuffled = null;
#if NET6_0_OR_GREATER
            if(!original.TryGetNonEnumeratedCount(out int count))
            {
                count = original.Count();
            }
#else
            int count = original.Count();
#endif
            switch (count)
            {
                case 0:
                    shuffled = Enumerable.Empty<T>();
                    break;
                case 1:
                    shuffled = original;
                    break;
                case 2:
                    shuffled = new[] { original.Last(), original.First() };
                    break;

                default:
                    if (original is not T[] unshuffled)
                    {
                        unshuffled = original.ToArray();
                    }

                    for (int i = unshuffled.Length - 1; i > 0; --i)
                    {
                        //Step 2: Randomly pick an item which has not been shuffled yet
#if !NET5_0_OR_GREATER
                        int k = _random.Next(i + 1);
#else
                        int k = RandomNumberGenerator.GetInt32(0, i + 1);
#endif

                        //Step 3: Swap the selected item with the last "unstruck" letter in the collection
                        (unshuffled[k], unshuffled[i]) = (unshuffled[i], unshuffled[k]);
                    }
                    shuffled = unshuffled;
                    break;
            }

            return shuffled;
        }
    }
}
