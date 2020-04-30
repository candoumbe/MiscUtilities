﻿using System.Linq;
using System.Text;
using System.Reflection;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// List of all types that can be directly converted to their string representation
        /// </summary>
        public static IEnumerable<Type> PrimitiveTypes = new[]
        {
            typeof(string),

            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(float), typeof(float?),

            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
            typeof(Guid), typeof(Guid?),
            typeof(bool), typeof(bool?)
        };

        public static IEnumerable<Type> NumericTypes = new[]
        {
            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(float), typeof(float?),
        };

        /// <summary>
        /// Converts a dictionary to a "URL" friendly representation
        /// </summary>
        /// <param name="keyValues">the dictionary to convert</param>
        /// <returns></returns>
        public static string ToQueryString(this IEnumerable<KeyValuePair<string, object>> keyValues)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<KeyValuePair<string, object>> localKeyValues = keyValues.Where(kv => kv.Value != null)
                                                                                .OrderBy(kv => kv.Key)
                                                                                .ThenBy(kv => kv.Value);
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD2_0
            foreach (KeyValuePair<string, object> kv in localKeyValues)
#else
            foreach ((string key, object value) in localKeyValues)
#endif
            {
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD2_0
                object value = kv.Value;
                string key = kv.Key;
#endif
                Type valueType = value.GetType();
                TypeInfo valueTypeInfo = valueType.GetTypeInfo();
                //The type of the value is a "simple" object
                if (valueTypeInfo.IsPrimitive || valueTypeInfo.IsEnum || PrimitiveTypes.Any(x => x == valueType))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }

                    sb
                        .Append(Uri.EscapeDataString(key))
                        .Append("=");

                    // DateTime/DateTimeOffset should be encoded in ISO format
                    switch (value)
                    {
                        case DateTime date:
                            sb.Append(date.ToString("s"));
                            break;
                        case DateTimeOffset date:
                            sb.Append(date.ToString("s"));
                            break;
                        default:
                            sb.Append(Uri.EscapeDataString(value.ToString()));
                            break;
                    }
                }
                else if (value is IEnumerable<KeyValuePair<string, object>> subDictionary)
                {
                    subDictionary =  subDictionary
#if !NETSTANDARD1_0 && !NETSTANDARD1_3
                                    .AsParallel()
#endif
                                    .ToDictionary(x => $"{key}[{x.Key}]", x => x.Value);

                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(ToQueryString(subDictionary));
                }
                else if (valueTypeInfo.BaseType == typeof(IEnumerable))
                {
                    IEnumerable enumerable = value as IEnumerable;
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
                                    sb.Append("&");
                                }
                                sb.Append(Uri.EscapeDataString($"{key}[{itemPosition}]"))
                                   .Append("=")
                                   .Append(Uri.EscapeDataString(value.ToString()));

                                itemPosition++;
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}