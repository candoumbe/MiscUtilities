using System.Collections.Generic;
using System.Linq;

namespace Candoumbe.MiscUtilities.Comparers;

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
    private static readonly EqualityComparer<T> s_elementComparer = EqualityComparer<T>.Default;

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
                            equals = s_elementComparer.Equals(x[i], y[i]);
                            i++;
                        } while (i < x.Length && equals);

                        break;
                }
            }
        }

        return equals;
    }

    ///<inheritdoc/>
    public int GetHashCode(T[] obj) => obj.Aggregate(17, (current, item) => (current * 31) + s_elementComparer.GetHashCode(item));
}