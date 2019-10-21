#if !(NETSTANDARD1_0 || NETSTANDARD1_1)
using System.Collections.Generic;

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
    }
}
#endif