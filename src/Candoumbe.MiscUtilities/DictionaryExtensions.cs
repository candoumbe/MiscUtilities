using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Collections.Generic
{
    /// <summary>
    /// Exntesions for <see cref="KeyValuePair{TKey, TValue}"/> and <see cref="IDictionary{TKey, TValue}"/> types.
    /// </summary>
    public static class DictionaryExtensions
    {
        private const char Ampersand = '&';

        /// <summary>
        /// List of all types that can be directly converted to their string representation
        /// </summary>
        public static readonly IEnumerable<Type> PrimitiveTypes = new[]
        {
            typeof(string),

            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(float), typeof(float?),

            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
#if NET6_0_OR_GREATER
            typeof(DateOnly), typeof(DateOnly?),
	#endif
            typeof(Guid), typeof(Guid?),
            typeof(bool), typeof(bool?)
        };

        /// <summary>
        /// Numeric types.
        /// </summary>
        public static readonly IEnumerable<Type> NumericTypes = new[]
        {
            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(float), typeof(float?),
        };

        /// <summary>
        /// Converts a collection of key/value pairs to a "URL" friendly representation.
        /// </summary>
        /// <param name="keyValues">the dictionary to convert</param>
        /// <param name="transform">A delegate that can be used to customize the value associated with a field name.</param>
        /// <returns></returns>
        public static string ToQueryString(this IEnumerable<KeyValuePair<string, object>> keyValues, Func<string, object, object> transform)
        {
            StringBuilder sb = new();

            IEnumerable<KeyValuePair<string, object>> localKeyValues = keyValues is null
                ? Enumerable.Empty<KeyValuePair<string, object>>()
                : keyValues.Where(kv => kv.Value != null)
                           .OrderBy(kv => kv.Key)
                           .ThenBy(kv => kv.Value);

            foreach (KeyValuePair<string, object> kv in localKeyValues)
            {
                object value = transform is not null
                    ? transform.Invoke(kv.Key, kv.Value)
                    : kv.Value;
                string key = kv.Key;
                if (value is not null)
                {
                    Type valueType = value.GetType();
                    TypeInfo valueTypeInfo = valueType.GetTypeInfo();
                    //The type of the value is a "simple" object
                    if (valueTypeInfo.IsPrimitive
                        || valueTypeInfo.IsEnum
                        || PrimitiveTypes.Any(x => x == valueType))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(Ampersand);
                        }

                        sb.Append(Uri.EscapeDataString(key))
                          .Append('=')
                          .Append(ConvertValueToString(value));
                    }
                    else if (value is IEnumerable<KeyValuePair<string, object>> subDictionary)
                    {
                        subDictionary = subDictionary
#if !NETSTANDARD1_0 && !NETSTANDARD1_3
                                    .AsParallel()
#endif
                                    .ToDictionary(x => $"{key}[{x.Key}]", x => x.Value);

                        if (sb.Length > 0)
                        {
                            sb.Append(Ampersand);
                        }
                        sb.Append(ToQueryString(subDictionary, transform));
                    }
                    else if (value is IEnumerable enumerable)
                    {
                        int itemPosition = 0;
                        Type elementType;
                        TypeInfo elementTypeInfo;

                        foreach (object item in enumerable)
                        {
                            if (item != null)
                            {
                                elementType = item.GetType();
                                elementTypeInfo = elementType.GetTypeInfo();
                                if (elementTypeInfo.IsPrimitive || PrimitiveTypes.Any(x => x == elementType))
                                {
                                    if (sb.Length > 0)
                                    {
                                        sb.Append(Ampersand);
                                    }

                                    sb.Append(Uri.EscapeDataString($"{key}[{itemPosition}]"))
                                       .Append('=')
                                       .Append(ConvertValueToString(item));

                                    itemPosition++;
                                }
                            }
                        }
                    }
                    else
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(valueType);
                        if (tc.CanConvertTo(typeof(string)))
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(Ampersand);
                            }

                            sb.Append(Uri.EscapeDataString(key))
                              .Append('=')
                              .Append(tc.ConvertTo(value, typeof(string)));
                        }
                    }
                }
            }

            return sb.ToString();

            static string ConvertValueToString(in object value) => value switch
            {
                DateTime dateTime when dateTime.Kind == DateTimeKind.Utc => dateTime.ToString("u").Replace(' ', 'T'),
                DateTime dateTime => dateTime.ToString("s").Replace(' ', 'T'),
                DateTimeOffset dateTimeOffset => $"{ dateTimeOffset:yyyy-MM-ddTHH:mm:ss}{(dateTimeOffset.Offset < TimeSpan.Zero ? "-" : "+")}{dateTimeOffset.Offset.Hours:00}:{dateTimeOffset.Offset.Minutes:00}",
#if NET6_0_OR_GREATER
                DateOnly date => date.ToString("yyyy-MM-dd"),
#endif
                int intValue => Convert.ToString(intValue),
                long longValue => Convert.ToString(longValue),
                _ => Uri.EscapeDataString(value.ToString())
            };
        }
    }
}