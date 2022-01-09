// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"
#if !NET5_0_OR_GREATER

namespace System
{
    using Collections.Generic;

    /// <summary>
    /// Provides a set of methods to deal with enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the values of the underlying 
        /// </summary>
        /// <typeparam name="TEnum">Type of the enum</typeparam>
        /// <returns></returns>
        public static IEnumerable<TEnum> GetValues<TEnum>()
        {
            Array array = Enum.GetValues(typeof(TEnum));
            List<TEnum> values = new (array.Length);

            foreach (TEnum @enum in array)
            {
                values.Add(@enum);
            }

            return values.ToArray();
        }
    }
}

#endif