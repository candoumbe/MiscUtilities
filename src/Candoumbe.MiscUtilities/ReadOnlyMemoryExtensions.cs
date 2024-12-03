// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if NET8_0_OR_GREATER
using System.Linq;
#endif

namespace Microsoft.Extensions.Primitives;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides extension methods for ReadOnlyMemory&lt;char&gt; type to perform string operations like finding occurrences,
/// checking start patterns and searching for substrings. Includes methods for finding first/last occurrences
/// and all occurrences of characters or character sequences.
/// </summary>
/// <remarks>
/// The class contains optimized methods for string searching and pattern matching operations
/// while working with <see cref="ReadOnlyMemory{T}"/> instances.
/// </remarks>
public static class ReadOnlyMemoryExtensions
{
    /// <summary>
    /// Reports all zero-based indexes of all occurrences of <paramref name="search"/> in the <paramref name="input"/>
    /// </summary>
    /// <param name="input">The <see cref="ReadOnlyMemory{T}"/> where searching occurrences will be performed</param>
    /// <param name="search">The searched element</param>
    /// <param name="comparer">An optional comparer to use during lookup.</param>
    /// <typeparam name="T">Type of items</typeparam>
    /// <returns>
    /// A collection of all indexes in <paramref name="input"/> where <paramref name="search"/> is present.
    /// </returns>
    /// <remarks>
    ///     The returned collection should not be expected to be in any particular order.
    /// </remarks>
    public static IEnumerable<int> Occurrences<T>(this ReadOnlyMemory<T> input, T search, IEqualityComparer<T> comparer = null)
    {
        int i = 0;

        if (input.IsEmpty)
        {
            yield break;
        }

        while (i < input.Length)
        {
            if (comparer?.Equals(input.Span[i], search) is true || Equals(input.Span[i],search))
            {
                yield return i;
            }

            i++;
        }
    }

    /// <summary>
    /// Lazily reports zero-based indexes of all occurrences of <see langword="char"/> that matches the specified <paramref name="predicate"/>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="predicate"></param>
    /// <returns>
    /// Collection of zero-based indexes of matching <typeparamref name="T"/> elements.
    /// The collection will be empty if no matching <typeparamref name="T"/> found.
    /// </returns>
    /// <exception cref="ArgumentNullException">if <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static IEnumerable<int> Occurrences<T>(this ReadOnlyMemory<T> input, Func<T, bool> predicate)
    {
        int i = 0;

        if (input.IsEmpty)
        {
            yield break;
        }

        while (i < input.Length)
        {
            if (predicate(input.Span[i]))
            {
                yield return i;
            }

            i++;
        }
    }

    /// <summary>
    /// Reports a zero-based index of the last occurrence of <paramref name="search"/> span within <paramref name="source"/> span.
    /// </summary>
    /// <param name="source">The source to search within</param>
    /// <param name="search">The item span to search for</param>
    /// <param name="comparer">The comparer</param>
    /// <returns>
    /// the index where <paramref name="search"/>
    /// was found in <paramref name="source"/> or <c>-1</c> if no occurrence found
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="source"/> or <paramref name="search"/> is <see langword="null"/>.</exception>
    public static int LastOccurrence<T>(this ReadOnlyMemory<T> source, ReadOnlyMemory<T> search, IEqualityComparer<T> comparer = null)
        where T : IEquatable<T>
    {
        bool found = false;
        int index = -1;

        if (source.IsEmpty)
        {
            if (search.IsEmpty)
            {
                index = 0;
            }
        }
        else if (search.IsEmpty)
        {
            index = source is { Length: > 0 } memory ? memory.Length - 1 : 0;
        }
        else if (source.Length >= search.Length)
        {
            int currentPos = source.LastIndexOf(search, comparer);
            int remainingCharactersInSource = source.Length - currentPos;

            if(remainingCharactersInSource >= search.Length)
            {
                int offset = 0;
                while (!found && currentPos >= 0)
                {
                    ReadOnlyMemory<T> subSegment = source.Slice(currentPos, search.Length);
                    found = subSegment.StartsWith(search, comparer);
                    if (found)
                    {
                        index = currentPos;
                    }

                    offset++;
                    currentPos = source.Length - (search.Length + offset);
                }
            }
        }

        return index;
    }

    /// <summary>
    /// Reports a zero-based index of the first occurrence of <paramref name="search"/> within <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source to search within</param>
    /// <param name="search">The item span to search for</param>
    /// <param name="comparer">The comparer</param>
    /// <returns>
    /// the index where <paramref name="search"/>
    /// was first found in <paramref name="source"/> or <c>-1</c> if no occurrence found
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="source"/> or <paramref name="search"/> is <see langword="null"/>.</exception>
    public static int FirstOccurrence<T>(this ReadOnlyMemory<T> source, ReadOnlyMemory<T> search, IEqualityComparer<T> comparer = null)
        where T : IEquatable<T>
    {
        bool found = false;
        int index = -1;

        if (source.IsEmpty)
        {
            if (search.IsEmpty)
            {
                index = 0;
            }
        }
        else if (search.IsEmpty)
        {
            index = source is { Length: > 0 } memory ? memory.Length - 1 : 0;
        }
        else if (source.Length >= search.Length)
        {
            int currentPos = source.IndexOf(search, comparer);
            int remainingCharactersInSource = source.Length - currentPos;

            if(remainingCharactersInSource >= search.Length)
            {
                int offset = 0;
                while (!found && currentPos >= 0)
                {
                    ReadOnlyMemory<T> subSegment = source.Slice(currentPos, search.Length);
                    found = subSegment.StartsWith(search, comparer);
                    if (found)
                    {
                        index = currentPos;
                    }

                    offset++;
                    currentPos = source.Length - (search.Length + offset);
                }
            }
        }

        return index;
    }

    /// <summary>
    /// Reports a zero-based index of the first occurrence of <typeparamref name="T"/> within <paramref name="source"/>
    /// that matches the specified <paramref name="predicate"/>.
    /// </summary>
    /// <param name="source">The source to search within</param>
    /// <param name="predicate">The predicate used to search for a matching <typeparamref name="T"/> element within <paramref name="source"/>.</param>
    /// <returns>
    /// the index of the first <typeparamref name="T"/> element which matches <paramref name="predicate"/>
    /// was found in <paramref name="source"/> or <c>-1</c> if no occurrence found.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static int FirstOccurrence<T>(this ReadOnlyMemory<T> source, Func<T, bool> predicate)
#if NET8_0_OR_GREATER
        => source.Occurrences(predicate)
                 .FirstOrDefault(-1);
    #else
    {
            using IEnumerator<int> enumerator = source.Occurrences(predicate).GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : -1;
    }
#endif

    /// <summary>
    /// Reports all zero-based indexes of all occurrences of <paramref name="search"/> in the <paramref name="input"/>
    /// </summary>
    /// <param name="input">The <see cref="StringSegment"/> where searching occurrences will be performed</param>
    /// <param name="search">The searched element</param>
    /// <returns>
    /// A collection of all indexes in <paramref name="input"/> where <paramref name="search"/> is present.
    /// </returns>
    public static IEnumerable<int> Occurrences<T>(this ReadOnlyMemory<T> input, ReadOnlyMemory<T> search) where T : IEquatable<T>
    {
        int currentPos = 0;

        if (search.IsEmpty)
        {
            yield return 0;
        }

        if (input.IsEmpty || input.Length < search.Length)
        {
            yield break;
        }

        int inputLength = input.Length;
        int index;
        do
        {
            index = input.Slice(currentPos).Span.IndexOf(search.Span);
            if (index != -1)
            {
                yield return index + currentPos;
            }

            int newPos = index + currentPos + search.Length;
            currentPos = newPos + 1;
        } while (currentPos < inputLength && index != -1);
    }

    /// <summary>
    /// Determines if <paramref name="input"/> starts with <paramref name="search"/>.
    /// </summary>
    /// <param name="input">The <see cref="ReadOnlyMemory{T}"/> to test</param>
    /// <param name="search">The value to </param>
    /// <param name="comparer">The comparer to use when looking for a match.</param>
    /// <returns><see langword="true"/> when <paramref name="input"/> starts with the specified <paramref name="search"/> value of <see langword="false"/> otherwise.</returns>
    public static bool StartsWith<T>(this ReadOnlyMemory<T> input, ReadOnlyMemory<T> search, IEqualityComparer<T> comparer = null)
    {
        bool startsWith = false;

        if (search.Length == 0)
        {
            startsWith = true;
        }
        else if(search.Length <= input.Length)
        {
            int i = 0;
            bool mismatchFound;
            Func<T, T, bool> predicate = comparer switch
            {
                null => (left, right) => left.Equals(right),
                _ => comparer.Equals
            };

            do
            {
                mismatchFound =  !predicate.Invoke(input.Span[i], search.Span[i]);
                i++;
            } while (!mismatchFound && i < search.Length);

            startsWith = !mismatchFound;
        }
        return startsWith;
    }

    private static int LastIndexOf<T>(this ReadOnlyMemory<T> input, ReadOnlyMemory<T> search, IEqualityComparer<T> comparer = null)
    {
        int lastIndex = -1;

        if (search.IsEmpty)
        {
            lastIndex = input.Length;
        }
        else if (search.Length <= input.Length)
        {
            int i = input.Length - 1;
            bool firstItemFound = false;
            T firstSearchItem = search.Span[0];

            Func<T, T, bool> predicate = comparer switch
            {
                null => (left, right) => left.Equals(right),
                _ => comparer.Equals
            };

            do
            {
                T currentItem = input.Span[i];
                firstItemFound =  predicate.Invoke(currentItem, firstSearchItem);
                if (!firstItemFound)
                {
                    i--;
                }
            } while (!firstItemFound && i >= 0);

#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
            if (firstItemFound && input[i..].StartsWith(search, comparer))
#else
            if (firstItemFound && input.Slice(i).StartsWith(search, comparer))
#endif
            {
                lastIndex = i;
            }
        }

        return lastIndex;
    }
    
    private static int IndexOf<T>(this ReadOnlyMemory<T> input, ReadOnlyMemory<T> search, IEqualityComparer<T> comparer = null)
    {
        int firstIndex = -1;

        if (search.IsEmpty)
        {
            firstIndex = 0;
        }
        else if (search.Length <= input.Length)
        {
            int i = 0;
            bool firstItemFound = false;
            T firstSearchItem = search.Span[0];

            Func<T, T, bool> predicate = comparer switch
            {
                null => (left, right) => left.Equals(right),
                _ => comparer.Equals
            };

            do
            {
                T currentItem = input.Span[i];
                firstItemFound =  predicate.Invoke(currentItem, firstSearchItem);
                if (!firstItemFound)
                {
                    i++;
                }
            } while (!firstItemFound && i < input.Length);

#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
            if (firstItemFound && input[i..].StartsWith(search, comparer))
#else
            if (firstItemFound && input.Slice(i).StartsWith(search, comparer))
#endif
            {
                firstIndex = i;
            }
        }

        return firstIndex;
    }
}