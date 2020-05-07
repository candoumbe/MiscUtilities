#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Extensions.Primitives
{
    public static class StringSegmentExtensions
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
        /// 
        /// </remarks>
        public static IEnumerable<int> Occurrences(this StringSegment input, char search)
        {
            int i = 0;

            if (input.Length == 0)
            {
                yield break;
            }
            else
            {
                while (i < input.Length)
                {
                    if (input[i] == search)
                    {
                        yield return i;
                    }

                    i++;
                }
            }
        }

        /// <summary>
        /// Report a zero-based index of the first occurrence of <paramref name="search"/> in <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="search"></param>
        /// <param name="stringComparison"></param>
        /// <returns>
        /// the index where <paramref name="search"/> 
        /// was found in <paramref name="source"/> or <c>-1</c> if no occurrence found
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref="source"> or <paramref name="search"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="search"/> is <c>empty</c></exception>
        public static int FirstOccurrence(this StringSegment source, StringSegment search, StringComparison stringComparison = default)
        {
            using IEnumerator<int> enumerator = source.Occurrences(search, stringComparison)
                                                 .GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : -1;
        }

        /// <summary>
        /// Report a zero-based index of the last occurrence of <paramref name="search"/> in <paramref name="source"/>
        /// </summary>
        /// <param name="source">Where to perform to search</param>
        /// <param name="search">The pattern to lookup for</param>
        /// <param name="stringComparison"></param>
        /// <returns>
        /// the index where <paramref name="search"/> 
        /// was found in <paramref name="source"/> or <c>-1</c> if no occurrence found
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref="source"> or <paramref name="search"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="search"/> is <c>empty</c></exception>
        public static int LastOccurrence(this StringSegment source, StringSegment search, StringComparison stringComparison = default)
        {
            int index =-1,
                offset = 0,
                currentPos = source.Length - search.Length;

            bool found = false;

            while(!found && currentPos > 0)
            {
                found = source.Subsegment(currentPos).Equals(search, stringComparison);
                if (found)
                {
                    index = currentPos;
                }
                offset++;
                currentPos = source.Length - (search.Length + offset);
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
        /// <remarks>
        /// 
        /// </remarks>
        public static IEnumerable<int> Occurrences(this StringSegment input, StringSegment search, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            int index,
                newPos,
                currentPos = 0;

            if (input.Length == 0)
            {
                yield break;
            }
            else
            {
                int inputLength = input.Length;
                do
                {
                    index = input.IndexOf(search[0], currentPos);

                    if (index != -1 && input.Subsegment(index, search.Length).Equals(stringComparison))
                    {
                        yield return index;
                        newPos = index + search.Length;
                        currentPos = newPos + 1;
                    }
                }
                while (currentPos <= inputLength && index != -1);
            }
        }

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the <see cref="StringSegment"/> to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this StringSegment input, string pattern, bool ignoreCase) => input.Value.Like(pattern, ignoreCase);

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the <see cref="StringSegment"/> to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this StringSegment input, string pattern) => input.Like(pattern, ignoreCase: true);

        /// <summary>
        /// Converts <paramref name="source"/> to its <see cref="LambdaExpression"/> equivalent
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <c>null</c>.</exception>
        public static LambdaExpression ToLambda<TSource>(this StringSegment source) => source.Value.ToLambda<TSource>();
    }
}
#endif