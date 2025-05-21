// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using ZLinq;
using ZLinq.Linq;
using static System.Text.Json.JsonSerializer;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Extension methods for object
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Deeply clone the <paramref name="source"/>
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="source"/></typeparam>
        /// <param name="source">The object to clone</param>
        /// <returns>A deep copy of the object</returns>
        public static T DeepClone<T>(this T source)
        {
            // Don't serialize a null object, return the default for that object
            T clone = ReferenceEquals(source, default(T))
                ? default
                : Deserialize<T>(Serialize(source));

            return clone;
        }

        /// <summary>
        /// This method is intend is to parse an object to extract its properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/></returns>
        public static IDictionary<string, object> ParseAnonymousObject(this object obj)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();

            if (obj != null)
            {
                if (obj is IEnumerable enumerable)
                {
                    dictionary = new Dictionary<string, object>();
                    using ValueEnumerator<FromNonGenericEnumerable<object>, object> enumerator = enumerable.AsValueEnumerable().GetEnumerator();
                    //using IDisposable disposable = enumerator as IDisposable;
                    int count = 0;
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current != null)
                        {
                            Type currentType = current.GetType();
                            TypeInfo currentTypeInfo = currentType.GetTypeInfo();
                            if (currentTypeInfo.IsPrimitive || currentTypeInfo.IsEnum || currentType == typeof(string))
                            {
                                dictionary.Add($"{count}", current);
                            }
                            count++;
                        }
                    }
                }
                else
                {
                    IEnumerable<PropertyInfo> properties = [ .. obj.GetType()
                            .GetRuntimeProperties()
                            .AsValueEnumerable()
                            .Where(pi => pi is { CanRead: true, GetMethod.IsStatic: false } && pi.GetValue(obj) != null)];

                    dictionary = properties.AsValueEnumerable().ToDictionary(
                        pi => pi.Name,
                        pi =>
                        {
                            object value = pi.GetValue(obj);
                            Type valueType = value?.GetType();
                            TypeInfo valueTypeInfo = valueType?.GetTypeInfo();

                            if (valueTypeInfo is not null && !(valueTypeInfo.IsEnum || valueTypeInfo.IsPrimitive || valueType == typeof(string) || DictionaryExtensions.PrimitiveTypes.Contains(valueType)))
                            {
                                value = ParseAnonymousObject(value);
                            }

                            return value;
                        }
                    );
                }
            }
            return dictionary
                .AsValueEnumerable()
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Converts an object to its string representation suitable for appending as query string in an url
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>the query string representation preceded with the "?" or an empty string</returns>
        public static string ToQueryString(this object obj) => ToQueryString(obj, null);

        /// <summary>
        /// Converts an object to its string representation suitable for appending as query string in an url
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <param name="transform">A delegate to apply to each property of <paramref name="obj"/>
        /// </param>
        /// <remarks>
        /// <para>
        /// The <paramref name="transform"/> delegate can be used to "transform" the value associated to a key prior to putting it in a query string.
        /// </para>
        /// <para>
        ///     Without <paramref name="transform"/>
        ///     <example>
        ///         <code>
        ///         class Example
        ///         {
        ///             var source = new { id = 1 };
        ///             string query =  source.ToQueryString() // "id=1"
        ///         }
        ///         </code>
        ///     </example>
        /// </para>
        /// <para>
        ///     With a <paramref name="transform"/>
        ///     <example>
        ///         <code>
        ///             var source = new { id = 1 };
        ///             string query =  source.ToQueryString((key, value) => key == "id" ? "newValue" : value) // "id=newValue"
        ///         </code>
        ///     </example>
        /// </para>
        /// </remarks>
        /// <returns>the query string representation</returns>
        public static string ToQueryString(this object obj, Func<string, object, object> transform)
        {
            return obj is IEnumerable<KeyValuePair<string, object>> dictionary
                ? DictionaryExtensions.ToQueryString(dictionary, transform)
                : DictionaryExtensions.ToQueryString(obj.ParseAnonymousObject(), transform);
        }

        /// <summary>
        /// Performs a "safe cast" of the specified object to the specified type.
        /// </summary>
        /// <typeparam name="TDest">targeted type</typeparam>
        /// <param name="obj">The object to cast</param>
        /// <returns>The "safe cast" result</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="obj"/> is <see langword="null"/></exception>
        public static TDest As<TDest>(this object obj) => (TDest)As(obj, typeof(TDest));

        /// <summary>
        /// Performs a "safe cast" of <paramref name="obj"/> to the type <paramref name="targetType"/>.
        /// </summary>
        /// <param name="obj">The object to cast</param>
        /// <param name="targetType">type to cast </param>
        /// <returns>The "safe cast" result</returns>
        /// <exception cref="ArgumentNullException"><paramref name="targetType"/> is <see langword="null"/></exception>
        public static object As(this object obj, Type targetType)
        {
#if NET
            ArgumentNullException.ThrowIfNull(targetType);
#else
            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }
#endif

            object safeCastResult = null;

            if (obj is not null)
            {
                Type sourceType = obj.GetType();

                if (targetType == sourceType)
                {
                    safeCastResult = obj;
                }
                else
                {
                    ParameterExpression param = Expression.Parameter(obj.GetType());
                    Expression asExpression = Expression.TypeAs(param, targetType);
                    LambdaExpression expression = Expression.Lambda(asExpression, param);
                    safeCastResult = expression.Compile().DynamicInvoke(obj);
                }
            }

            return safeCastResult;
        }

        /// <summary>
        /// Converts <paramref name="obj"/> to its JSON representation using provided <paramref name="settings"/>.
        /// </summary>
        /// <param name="obj">The object to "jsonify"</param>
        /// <param name="settings">settings to use when serializing <paramref name="obj"/> to Json.</param>
        /// <returns>Json representation</returns>
        public static string Jsonify(this object obj, JsonSerializerOptions settings = null)
        {
            string json = null;
#if NET8_0_OR_GREATER
            settings ??= JsonSerializerOptions.Default;
#endif
            if (obj is not null)
            {
                json = Serialize(obj, obj.GetType(), settings);
            }

            return json;
        }
    }
}