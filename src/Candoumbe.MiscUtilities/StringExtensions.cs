
namespace System
{
    using Globalization;

    #if STRING_SEGMENT
    using Microsoft.Extensions.Primitives;
    #endif
    using Collections.Generic;
    using Linq;
    using Linq.Expressions;
    using Text;
    using Text.RegularExpressions;

    using static Linq.Expressions.Expression;

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
        /// <example>
        /// <c>"cyrille-alexandre".ToTitleCase(); // "Cyrille-Alexandre" </c>
        /// </example>
        public static string ToTitleCase(this string input)
        {
#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
            CultureInfo culture = CultureInfo.CurrentCulture;
            TextInfo textInfo = culture.TextInfo;

            return textInfo.ToTitleCase(input);

#else
            StringBuilder sbResult = null;
            if ((input?.Length ?? 0 ) > 0)
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
#endif
        }

        /// <summary>
        /// Converts the <paramref name="input"/> to its camelCase equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <returns>the string converted to Title case</returns>
        /// <example><c>"cyrille-alexandre".ToTitleCase()" // "cyrilleAlexandre"</c></example>
        public static string ToCamelCase(this string input)
        {
            StringBuilder sbResult = null;
            input ??= string.Empty;
            string sanitizedInput
#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
             = new(input
#else
            = new(input.ToCharArray()
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
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
        public static bool Like(this string input, string pattern) => input.Like(pattern, ignoreCase: true);

        /// <summary>
        /// Perfoms a VB "Like" comparison.
        /// <para>
        /// The <paramref name="pattern"/> can be
        /// </para>
        /// </summary>
        /// <param name="input">the string to test</param>
        /// <param name="pattern">the pattern to test <paramref name="input"/> against</param>
        /// <param name="ignoreCase"><see langword="true"/> to ignore case</param>
        /// <returns><see langword="true"/> when <paramref name="input"/> matches <paramref name="pattern"/> and <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="input"/> or <paramref name="pattern"/> is <see langword="null"/>.</exception>
        public static bool Like(this string input, string pattern, bool ignoreCase)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            StringBuilder sbPattern = new(pattern.Length * 2);
            RegexOptions regexOptions = RegexOptions.Singleline;

            if (ignoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            foreach (char chr in pattern)
            {
                sbPattern.Append(chr switch
                {
                    '+' => @"\+",
                    '*' => ".*",
                    '.' => @"\.",
                    '?' => '.',
                    '|' => @"\|",
                    '^' => @"\^",
                    _ => chr
                });
            }

            return Regex.IsMatch(input, $"{sbPattern}$", regexOptions, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Converts <paramref name="source"/> to its <see cref="LambdaExpression"/> equivalent
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static LambdaExpression ToLambda<TSource>(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ParameterExpression pe = Parameter(typeof(TSource), "x");

            IEnumerable<string> fields = source.Replace(@"[""", ".")
                                               .Replace(@"""]", string.Empty)
                                               .Split(new[] { '.' })
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
        /// Decodes a <see cref="string"/> converted using <see cref="GuidExtensions.Encode(Guid)"/> back to <see cref="Guid"/>.
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
        /// Converts <paramref name="input"/> to its lower kebab representation
        ///
        /// </summary>
        /// <param name="input">The string to transform</param>
        /// <returns>The lower-kebab-cased string</returns>
        /// <example>
        ///
        /// "JusticeLeague".ToLowerKebabCase() // "justice-league"
        ///
        /// </example>
        public static string Slugify(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), $"{nameof(input)} cannot be null");
            }

            StringBuilder sb = new(input.Length * 2);
            input = input.Trim()
                         .Replace("  ", " ");

            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];

                switch (character)
                {
                    case char c when char.IsWhiteSpace(c) || char.IsPunctuation(c):
                        if (i > 0 && !char.IsWhiteSpace(input[i - 1]))
                        {
                            sb.Append('-');
                        }
                        break;
                    case char c when char.IsUpper(c):
                        if (i == 0)
                        {
                            sb.Append(char.ToLowerInvariant(c));
                        }
                        else if (i > 0 && !char.IsWhiteSpace(input[i - 1]))
                        {
                            sb.Append('-');
                            sb.Append(char.ToLowerInvariant(c));
                        }
                        break;
                    default:
                        sb.Append(char.ToLowerInvariant(character));
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts <param ref="input"/> to its snake_case equivalent
        ///
        /// </summary>
        /// <param name="input">The string to transform</param>
        /// <returns>The snake-cased string</returns>
        /// <example>
        ///
        /// "JusticeLeague".ToSnakeCase() // "Justice_League"
        ///
        /// </example>
        public static string ToSnakeCase(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), $"{nameof(input)} cannot be null");
            }

#if !NET5_0_OR_GREATER
            StringBuilder sb = new(input.Length * 2);
            input = input.Trim()
                         .Replace("  ", " ");

            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];

                switch (character)
                {
                    case char c when char.IsWhiteSpace(c) || char.IsPunctuation(c):
                        if (i > 0 && !char.IsWhiteSpace(input[i - 1]))
                        {
                            sb.Append('_');
                        }
                        break;
                    case char c when char.IsUpper(c):
                        if (i == 0)
                        {
                            sb.Append(char.ToLowerInvariant(c));
                        }
                        else if (i > 0 && !char.IsWhiteSpace(input[i - 1]))
                        {
                            sb.Append('_');
                            sb.Append(char.ToLowerInvariant(c));
                        }
                        break;
                    default:
                        sb.Append(char.ToLowerInvariant(character));
                        break;
                }
            }
#else
            Rune[] runes = input.EnumerateRunes().ToArray();
            StringBuilder sb = new(runes.Length * 2);
            input = input.Trim()
                         .Replace("  ", " ");

            for (int i = 0; i < runes.Length; i++)
            {
                Rune rune = runes[i];

                switch (rune)
                {
                    case Rune c when Rune.IsWhiteSpace(c) || Rune.IsPunctuation(c):
                        if (i > 0 && !char.IsWhiteSpace(input[i - 1]))
                        {
                            sb.Append('_');
                        }
                        break;
                    case Rune c when Rune.IsUpper(c):
                        if (i == 0)
                        {
                            sb.Append(Rune.ToLowerInvariant(c));
                        }
                        else if (i > 0 && !char.IsWhiteSpace(input[i - 1]))
                        {
                            sb.Append('_');
                            sb.Append(Rune.ToLowerInvariant(c));
                        }
                        break;
                    default:
                        sb.Append(Rune.ToLowerInvariant(rune));
                        break;
                }
            }
#endif
            return sb.ToString();
        }

#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
        /// <summary>
        /// Removes diacritics from <paramref name="input"/>
        /// </summary>
        /// <param name="input">where to remove diacritics</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new(input.Length);

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
#endif
#if STRING_SEGMENT
        /// <summary>
        /// Removes diacritics from <paramref name="input"/>
        /// </summary>
        /// <param name="input">where to remove diacritics</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this StringSegment input) => input.Value.RemoveDiacritics();
#endif

        /// <summary>
        /// Reports all zero-based indexes of all occurrences of <paramref name="search"/> in the <paramref name="input"/>
        /// </summary>
        /// <param name="input">The <see cref="string"/> onto which searching occurrences will be performed</param>
        /// <param name="search">The searched element</param>
        /// <param name="stringComparison"></param>
        /// <returns>
        /// A collection of all indexes in <paramref name="input"/> where <paramref name="search"/> is present.
        /// </returns>
        /// <remarks>
        ///
        /// </remarks>
        public static IEnumerable<int> Occurrences(this string input, string search, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            int index,
                newPos,
                currentPos = 0;

            if (input.Length > 0)
            {
                int inputLength = input.Length;

                do
                {
                    index = input.IndexOf(search, currentPos, stringComparison);

                    if (index != -1)
                    {
                        newPos = index + search.Length;
                        yield return index;
                        currentPos = newPos + 1;
                    }
                }
                while (currentPos <= inputLength && index != -1);
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
        public static int FirstOccurrence(this string source, string search, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

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
        /// Report a zero-based index of the last occurrence of <paramref name="search"/> in <paramref name="source"/>
        /// </summary>
        /// <param name="source">string where to search for occurrence</param>
        /// <param name="search">The text to search</param>
        /// <param name="stringComparison"></param>
        /// <returns>the index where <paramref name="search"/> was found in <paramref name="source"/> or <c>-1</c> if no occurrence found</returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="search"/> is <see cref="string.Empty"/></exception>
        /// <exception cref="ArgumentNullException">if either <paramref name="source"/> or <paramref name="search"/> is <c>nuull</c></exception>
        public static int LastOccurrence(this string source, string search, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

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

        /// <summary>
        /// Converts the <paramref name="input"/> to its PascalCase equivalent
        /// </summary>
        /// <param name="input">the string to convert</param>
        /// <returns>the string converted to PascalCasecase</returns>
        /// <example>
        /// <code>
        /// "cyrille-alexandre".ToPascalCase() // CyrilleAlexandre
        /// </code>
        /// </example>
        public static string ToPascalCase(this string input)
        {
            StringBuilder sbResult = null;
            input ??= string.Empty;

            if (input != string.Empty)
            {
                sbResult = new StringBuilder(input);
                if (char.IsLetter(sbResult[0]))
                {
                    sbResult[0] = char.ToUpperInvariant(sbResult[0]);
                }

                for (int i = 1; i < input.Length; i++)
                {
                    if (!char.IsLetterOrDigit(sbResult[i - 1]))
                    {
                        sbResult[i] = char.ToUpperInvariant(sbResult[i]);
                    }
                }
            }

            return (sbResult?.ToString() ?? string.Empty)
                        .Replace(" ", string.Empty)
                        .Replace("-", string.Empty)
                        .Replace("_", string.Empty);
        }
    }
}
