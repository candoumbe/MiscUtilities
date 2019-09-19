﻿#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
using Microsoft.Extensions.Primitives;
using System.Globalization;
# endif
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using static System.Collections.Generic.EnumerableExtensions;
using static System.Linq.Expressions.Expression;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="string"/> type
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the <paramref name="input"/> to its Title Case equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <returns>the string converted to Title case</returns>
        /// <example><c>"cyrille-alexandre".<see cref="ToTitleCase()"/></c> returns <c>"Cyrille-Alexandre"</c></example>
        public static string ToTitleCase(this string input)
        {
            StringBuilder sbResult = null;
            if (input?.ToCharArray()?.AtLeastOnce() ?? false)
            {
                sbResult = new StringBuilder(input);
                if (char.IsLetter(sbResult[0]))
                {
                    sbResult[0] = char.ToUpper(sbResult[0]);
                }

                for (int i = 1; i < sbResult.Length; i++)
                {
                    if (char.IsWhiteSpace(sbResult[i - 1]) || sbResult[i - 1] == '-')
                    {
                        sbResult[i] = char.ToUpper(sbResult[i]);
                    }
                }
            }

            return sbResult?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Converts the <paramref name="input"/> to its camelCase equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <returns>the string converted to Title case</returns>
        /// <example><c>"cyrille-alexandre".<see cref="ToTitleCase()"/></c> returns <c>"cyrilleAlexandre"</c></example>
        public static string ToCamelCase(this string input)
        {
            StringBuilder sbResult = null;
            input = input ?? string.Empty;
            string sanitizedInput
#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
             = new string(input
#else
            = new string(input.ToCharArray()
#endif
                .Where(character => char.IsLetterOrDigit(character) || character == '-' || char.IsWhiteSpace(character))
                    .ToArray());

            if (sanitizedInput != string.Empty)
            {
                sbResult = new StringBuilder(sanitizedInput);
                if (char.IsLetter(sbResult[0]))
                {
                    sbResult[0] = char.ToLower(sbResult[0]);
                }

                for (int i = 1; i < sbResult.Length; i++)
                {
                    if (char.IsWhiteSpace(sbResult[i - 1]) || sbResult[i - 1] == '-')
                    {
                        sbResult[i] = char.ToUpper(sbResult[i]);
                    }
                }
            }

            return (sbResult?.ToString() ?? string.Empty)
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);
        }

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the string to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this string input, string pattern) => input.Like(pattern, ignoreCase: true);

        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the string to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this string input, string pattern, bool ignoreCase)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            RegexOptions regexOptions = RegexOptions.Singleline;
            if (ignoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            pattern = pattern.Replace("?", ".")
                .Replace("|", @"\|")
                .Replace("*", ".*");

            return Regex.IsMatch(input, $"{pattern}$", regexOptions);
        }

#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
        /// <summary>
        /// Perfoms a VB "Like" comparison
        /// </summary>
        /// <param name="input">the <see cref="StringSegment"/> to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <c>null</c>.</exception>
        public static bool Like(this StringSegment input, string pattern, bool ignoreCase) => Like(input.Value, pattern, ignoreCase);

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
#endif

        /// <summary>
        /// Converts <paramref name="source"/> to its <see cref="LambdaExpression"/> equivalent
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <c>null</c>.</exception>
        public static LambdaExpression ToLambda<TSource>(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ParameterExpression pe = Parameter(typeof(TSource), "x");
            IEnumerable<string> fields = source.Split(new[] { '.' })
                .Select(item => item.Trim());
            MemberExpression property = null;
            foreach (string field in fields)
            {
                property = property == null
                    ? Property(pe, field)
                    : Property(property, field);
            }

            return Lambda(property, pe);
        }

        /// <summary>
        /// Decodes a <see cref="string"/> converted using <see cref="GuidExtensions.Encode{string}"/> back to <see cref="Guid"/>. 
        /// </summary>
        /// <remarks>
        /// See http://madskristensen.net/post/A-shorter-and-URL-friendly-GUID for more details.
        /// </remarks>
        /// <param name="encoded">the encoded <see cref="Guid"/> string</param>
        /// <returns>The original <see cref="Guid"/></returns>
        public static Guid Decode(this string encoded)
        {
            encoded = encoded.Replace("_", "/");
            encoded = encoded.Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(encoded + "==");
            return new Guid(buffer);
        }

        /// <summary>
        /// Converts <see cref="input"/> to its lower kebab representation
        /// 
        /// </summary>
        /// <param name="input">The string to transform</param>
        /// <returns>The lower-kebab-cased string</returns>
        /// <example>
        /// 
        /// "JusticeLeague".ToLowerKebabCase() // "justice-league"
        /// 
        /// </example>
        public static string ToLowerKebabCase(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), $"{nameof(input)} cannot be null");
            }

            StringBuilder sb = new StringBuilder(input.Length * 2);
            foreach (char character in input)
            {
                if (char.IsUpper(character) && sb.Length > 0)
                {
                    sb.Append("-");
                }
                sb.Append(char.ToLower(character));
            }

            return sb.ToString();
        }

#if !NETSTANDARD1_0 && !NETSTANDARD1_1
        /// <summary>
        /// Removes diacritics from <paramref name="input"/>
        /// </summary>
        /// <param name="input">where to remove diacritics</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder(input.Length);

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Removes diacritics from <paramref name="input"/>
        /// </summary>
        /// <param name="input">where to remove diacritics</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this StringSegment input) => input.Value.RemoveDiacritics();


#if !NETSTANDARD2_0
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
        public static IEnumerable<int> Occurrences(this StringSegment input, StringSegment search, StringComparison stringComparison = StringComparison.InvariantCulture)
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
                string inputValue = input.Value;
                int inputLength = input.Length;
                string searchValue = search.Value;
                int searchLength = searchValue.Length;
                do
                {
                    index = inputValue.IndexOf(searchValue, currentPos, stringComparison);

                    if (index != -1)
                    {
                        newPos = index + searchLength;
                        yield return index;
                        currentPos = newPos + 1;
                    }
                }
                while (currentPos <= inputLength && index != -1);
            }

        }

        /// <summary>
        /// Gets a 0-based index of the first occurrence of <paramref name="search"/> in <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="search"></param>
        /// <param name="stringComparison"></param>
        /// <returns>
        /// the index where <paramref name="search"/> 
        /// was found in <paramref name="source"/> or <c>-1</c> if no occurrence found
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="search"/> is <c>default(StringSegment)</c></exception>
        public static int FirstOccurrence(this StringSegment source, StringSegment search, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            if (search.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(search), "Search cannot be empty");
            }

            using IEnumerator<int> enumerator = Occurrences(source, search, stringComparison).GetEnumerator();

            return enumerator.MoveNext()
                ? enumerator.Current
                : -1;
        }

        /// <summary>
        /// Gets a 0-based index of the first occurrence of <paramref name="search"/> in <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="search"></param>
        /// <param name="stringComparison"></param>
        /// <returns>the index where <paramref name="search"/> was found in <paramref name="source"/> or <c>-1</c> if no occurrence found</returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="search"/> is <c>default(StringSegment)</c></exception>
        public static int LastOccurrence(this StringSegment source, StringSegment search, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            if (search.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(search), "Search cannot be empty");
            }

            using IEnumerator<int> enumerator = Occurrences(source, search, stringComparison).GetEnumerator();

            int index = -1;
            while (enumerator.MoveNext())
            {
                index = enumerator.Current;
            }

            return index;
        }
#endif

#endif

    }
}
