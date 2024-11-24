// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

namespace Microsoft.Extensions.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;

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
        /// <param name="input">The <see cref="StringSegment"/> where searching occurrences will be performed</param>
        /// <param name="search">The searched element</param>
        /// <returns>
        /// A collection of all indexes in <paramref name="input"/> where <paramref name="search"/> is present.
        /// </returns>
        /// <remarks>
        ///     The returned collection should not be expected to be in any particular order.
        /// </remarks>
        public static IEnumerable<int> Occurrences(this ReadOnlyMemory<char> input, char search)
        {
            int i = 0;

            if (input.IsEmpty)
            {
                yield break;
            }

            while (i < input.Length)
            {
                if (input.Span[i] == search)
                {
                    yield return i;
                }

                i++;
            }
        }
                
        /// <summary>
        /// Reports the zero-based index of the first occurrence of a character span within the source span.
        /// </summary>
        /// <param name="source">The source to search within</param>
        /// <param name="search">The character span to search for</param>
        /// <param name="comparison">The type of string comparison to use. Defaults to StringComparison.InvariantCultureIgnoreCase</param>
        /// <returns>
        /// The index where the search span was first found in the source, or -1 if no occurrence was found.
        /// If the search span is empty, returns 0.
        /// </returns>
        /// </summary>
        public static int FirstOccurrence(this ReadOnlyMemory<char> source, ReadOnlySpan<char> search, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            int index;
            if (search.IsEmpty)
            {
                index = 0;
            }
            else
            {
                using IEnumerator<int> enumerator = source.Occurrences(search.ToArray(), comparison)
                    .GetEnumerator();

                index =  enumerator.MoveNext()
                    ? enumerator.Current
                    : -1;
            }

            return index;
        }

        /// <summary>
        /// Report a zero-based index of the last occurrence of <paramref name="search"/> span within <paramref name="source"/> span.
        /// </summary>
        /// <param name="source">The source to search within</param>
        /// <param name="search">The character span to search for</param>
        /// <returns>
        /// the index where <paramref name="search"/>
        /// was found in <paramref name="source"/> or <c>-1</c> if no occurrence found
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
        public static int LastOccurrence(this ReadOnlyMemory<char> source, ReadOnlySpan<char> search, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            bool found = false;
            int index = -1,
                offset = 0;

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
                int currentPos = source.Span.LastIndexOf(search[0]);
                int remainingCharactersInSource = source.Length - currentPos;

                if(remainingCharactersInSource >= search.Length)
                {
                    while (!found && currentPos >= 0)
                    {
                        ReadOnlySpan<char> subSegment = source.Slice(currentPos, search.Length).Span;
                        found = subSegment.Equals(search, stringComparison);
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
        /// Reports all zero-based indexes of all occurrences of <paramref name="search"/> in the <paramref name="input"/>
        /// </summary>
        /// <param name="input">The <see cref="StringSegment"/> where searching occurrences will be performed</param>
        /// <param name="search">The searched element</param>
        /// <param name="stringComparison"></param>
        /// <returns>
        /// A collection of all indexes in <paramref name="input"/> where <paramref name="search"/> is present.
        /// </returns>
        public static IEnumerable<int> Occurrences(this ReadOnlyMemory<char> input, ReadOnlyMemory<char> search, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
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
                index = input.Slice(currentPos).Span.IndexOf(search.Span, stringComparison);
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
        /// <returns><see langword="true"/> when <paramref name="input"/> starts with the specified <paramref name="search"/> value of <see langword="false"/> otherwise.</returns>
        public static bool StartsWith<T>(this ReadOnlyMemory<T> input, ReadOnlySpan<T> search)
        {
            bool startsWith = false;

            if (search.Length == 0)
            {
                startsWith = true;
            }
            else if(search.Length < input.Length)
            {
                int i = 0;
                bool mismatchFound;
                do
                {
                    mismatchFound =  !input.Span[i].Equals(search[i]);
                    i++;
                } while (!mismatchFound && i < search.Length);

                startsWith = !mismatchFound;
            }
            return startsWith;
        }
    }
}