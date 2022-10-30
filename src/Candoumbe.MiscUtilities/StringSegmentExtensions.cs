// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if STRING_SEGMENT

namespace Microsoft.Extensions.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension methods for <see cref="StringSegment"/> types
    /// </summary>
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
        public static IEnumerable<int> Occurrences(this StringSegment input, char search)
        {
            int i = 0;

            if (!StringSegment.IsNullOrEmpty(input))
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
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="search"/> is <see langword="null"/></exception>
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
        /// <exception cref="ArgumentException">if <paramref name="search"/> is <c>empty</c></exception>
        public static int LastOccurrence(this StringSegment source, StringSegment search, StringComparison stringComparison = default)
        {
            if (StringSegment.IsNullOrEmpty(search))
            {
                throw new ArgumentException($"{nameof(search)} cannot be null or empty", nameof(search));
            }

            int index = -1,
                offset = 0,
                currentPos = source.LastIndexOf(search[0]);

            bool found = false;

            while (!found && currentPos >= 0)
            {
                StringSegment subSegment = source.Subsegment(currentPos, search.Length);
                found = subSegment.Equals(search, stringComparison);
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
        public static IEnumerable<int> Occurrences(this StringSegment input, StringSegment search, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            int index,
                newPos,
                currentPos = 0;

            if (!StringSegment.IsNullOrEmpty(input))
            {
                int inputLength = input.Length;
                do
                {
                    index = input.IndexOf(search[0], currentPos);

                    if (index != -1 && input.Subsegment(index, search.Length).Equals(search, stringComparison))
                    {
                        yield return index;
                    }

                    newPos = index + search.Length;
                    currentPos = newPos + 1;
                }
                while (currentPos <= inputLength && index != -1);
            }
        }

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the <see cref="StringSegment"/> to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <param name="ignoreCase"><see langword="true"/> to ignore case</param>
        /// <returns><see langword="true"/> when <paramref name="input"/> is like the specified <paramref name="pattern"/> and <see langword="false"/>
        /// otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
        public static bool Like(this StringSegment input, string pattern, bool ignoreCase) => input.Value.Like(pattern, ignoreCase);

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the <see cref="StringSegment"/> to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <returns><see langword="true"/> if input is like <paramref name="pattern"/> and <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
        public static bool Like(this StringSegment input, string pattern) => input.Like(pattern, ignoreCase: true);

        /// <summary>
        /// Converts <paramref name="source"/> to its <see cref="LambdaExpression"/> equivalent
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns><see cref="LambdaExpression"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static LambdaExpression ToLambda<TSource>(this StringSegment source) => source.Value.ToLambda<TSource>();


        /// <summary>
        /// Converts the <paramref name="input"/> to its Title Case equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use when performing the casing conversion</param>
        /// <returns>the string converted to Title case</returns>
        /// <example>
        /// <c>"cyrille-alexandre".ToTitleCase(); // "Cyrille-Alexandre" </c>
        /// </example>
        public static string ToTitleCase(this StringSegment input, CultureInfo cultureInfo = null)
        {
            TextInfo textInfo = cultureInfo?.TextInfo ?? CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.Value);
        }
    }
}
#endif