using System;
using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Comparers;

/// <summary>
/// Generic equality comparer
/// </summary>
/// <typeparam name="T">Type that this equality comparer can be used with.</typeparam>
public class GenericEqualityComparer<T> : EqualityComparer<T>
{
    private readonly Func<T, T, bool> _equalsFunc;
    private readonly Func<T, int> _hashFunc;

    /// <summary>
    /// Builds a new <see cref="GenericEqualityComparer{T}"/> instance.
    /// </summary>
    /// <param name="equalsFunc">The function to use as the <see cref="EqualityComparer{T}.Equals(T,T)"/> implementation.</param>
    /// <param name="getHashCodeFunc">The function to use as the <see cref="EqualityComparer{T}.GetHashCode(T)"/> implementation.</param>
    /// <exception cref="ArgumentNullException">equals</exception>
    public GenericEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> getHashCodeFunc = null)
    {
        _equalsFunc = equalsFunc;
        _hashFunc = getHashCodeFunc ?? (obj => obj.GetHashCode());
    }

    /// <inheritdoc />
    public override bool Equals(T x, T y) => _equalsFunc(x, y);

    /// <inheritdoc />
    public override int GetHashCode(T obj) => _hashFunc(obj);
}