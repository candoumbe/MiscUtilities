using System.Collections.Generic;

namespace Utilities
{
    /// <summary>
    /// Generic implementation of <see cref="IEqualityComparer{T}"/> suitable for comparing two arrays of <typeparamref name="T"/>
    ///
    /// <para>
    /// This implementation is based on <see href="https://exceptionshub.com/gethashcode-on-byte-array.html"/> and relies internally
    /// on <see cref="EqualityComparer{T}.Default"/> to compare items of each array
    /// </para>
    /// </summary>
    /// <typeparam name="T">Type of the collection</typeparam>
    public class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        // You could make this a per-instance field with a constructor parameter
        private static readonly EqualityComparer<T> _elementComparer = EqualityComparer<T>.Default;

        ///<inheritdoc/>
        public bool Equals(T[] x, T[] y)
        {
            bool equals = false;
            if (x != null && y != null)
            {
                if (x == y)
                {
                    equals = true;
                }
                else if (x.Length == y.Length)
                {
                    switch (x.Length)
                    {
                        case 0 when y.Length == 0:
                            equals = true;
                            break;
                        default:
                            int i = 0;

                            do
                            {
                                equals = _elementComparer.Equals(x[i], y[i]);
                                i++;
                            } while (i < x.Length && equals);

                            break;
                    }
                }
            }

            return equals;
        }

        ///<inheritdoc/>
        public int GetHashCode(T[] obj)
        {
            int hash = 0;
            if (obj != null)
            {
                hash = 17;
                foreach (T item in obj)
                {
                    hash = (hash * 31) + _elementComparer.GetHashCode(item);
                }
            }

            return hash;
        }
    }
}
