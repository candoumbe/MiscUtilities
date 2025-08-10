using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace System.Collections.Generic;

/// <summary>
/// Extensions for <see cref="KeyValuePair{TKey, TValue}"/> and <see cref="IDictionary{TKey, TValue}"/> types.
/// </summary>
public static class DictionaryExtensions
{
    private const char Ampersand = '&';

    /// <summary>
    /// List of all types that can be directly converted to their string representation
    /// </summary>
    public static readonly IEnumerable<Type> PrimitiveTypes =
    [
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
        typeof(TimeOnly), typeof(TimeOnly?),
#endif
        typeof(Guid), typeof(Guid?),
        typeof(bool), typeof(bool?)
    ];

    /// <summary>
    /// Converts a collection of key/value pairs to a "URL" friendly representation.
    /// </summary>
    /// <param name="keyValues">the dictionary to convert</param>
    /// <param name="transform">A delegate that can be used to customize the value associated with a field name.</param>
    /// <returns>a string that can be directly and safely appended to a query string either after a <c>?</c> or <c>&amp;</c> character.</returns>
    public static string ToQueryString(this IEnumerable<KeyValuePair<string, object>> keyValues, Func<string, object, object> transform)
    {
        StringBuilder sb = new();

        IEnumerable<KeyValuePair<string, object>> localKeyValues = keyValues is null
                                                                       ? []
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
                if (valueTypeInfo.IsPrimitive || valueTypeInfo.IsEnum
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
                        .AsParallel()
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

                    foreach (object item in enumerable)
                    {
                        if (item is not null)
                        {
                            elementType = item.GetType();
                            TypeInfo elementTypeInfo = elementType.GetTypeInfo();
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
            DateTime { Kind: DateTimeKind.Utc } dateTime => dateTime.ToString("u").Replace(' ', 'T'),
            DateTime dateTime                            => dateTime.ToString("s").Replace(' ', 'T'),
            DateTimeOffset dateTimeOffset                => $"{dateTimeOffset:yyyy-MM-ddTHH:mm:ss}{(dateTimeOffset.Offset < TimeSpan.Zero ? "-" : "+")}{dateTimeOffset.Offset.Hours:00}:{dateTimeOffset.Offset.Minutes:00}",
#if NET6_0_OR_GREATER
            TimeOnly time => (time.Hour, time.Minute, time.Second, time.Millisecond) switch
            {
                (_, _, _, > 0) => time.ToString("hh:mm:ss.fffffff"),
                _              => time.ToString("hh:mm:ss"),
            },
            DateOnly date => date.ToString("O"),
#endif
            int intValue   => Convert.ToString(intValue),
            long longValue => Convert.ToString(longValue),
            _              => Uri.EscapeDataString(value.ToString())
        };
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Retrieves the value associated with the specified key from the dictionary.
    /// If the key does not exist, adds the key with the specified default value
    /// and returns the default value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search for the key.</param>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="defaultValue">The value to add if the key does not exist.</param>
    /// <returns>The value associated with the specified key, or the default value if the key does not exist.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        TValue value = defaultValue;

        ref TValue val = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out bool exists);
        if (!exists)
        {
            val = defaultValue;
        }
        else
        {
            value = val;
        }

        return value;
    }
#endif
}