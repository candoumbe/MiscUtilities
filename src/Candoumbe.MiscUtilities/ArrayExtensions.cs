// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

namespace System;

using Collections.Generic;
using Linq;

/// <summary>
/// Sets of extension methods for <see cref="Array"/> type.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Converts an <see cref="Array"/>
    /// </summary>
    /// <typeparam name="T">Type of items in the array</typeparam>
    /// <param name="array">The array to convert</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T[] ToArray<T>(this Array array)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(array);
#else
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }
#endif

        IList<T> values = new List<T>(array.Length);
        foreach (T value in array)
        {
            values.Add(value);
        }

        return [.. values];
    }
}