﻿// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.ExpressionExtensions;
using System.Threading.Tasks;
using System.Collections.Concurrent;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/> type.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Extension method to convert a group to a dictionary of T
        /// </summary>
        /// <typeparam name="TKey">type of the key in the group</typeparam>
        /// <typeparam name="TElement">type of the element groupêd</typeparam>
        /// <param name="groups"></param>
        /// <returns>a dictionary</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="groups"/> is <see langword="null"/>.</exception>
        public static IDictionary<TKey, IEnumerable<TElement>> ToDictionary<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groups)
        {
#if NET
            ArgumentNullException.ThrowIfNull(groups);
#else
            if (groups is null)
            {
                throw new ArgumentNullException(nameof(groups));
            }
#endif
            return groups.ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains exactly one item
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains exactly one element</returns>
        /// <see cref="Exactly{T}(IEnumerable{T}, Expression{Func{T, bool}}, int)"/>
        public static bool Once<T>(this IEnumerable<T> items) => Once(items, True<T>());

        /// <summary>
        /// Tests if <paramref name="items"/> contains no item that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">re</param>
        /// <returns><see langword="true"/> if <paramref name="items"/> does not contain any element that fullfills <paramref name="predicate"/> and <see langword="false"/> otherwise.</returns>
        public static bool None<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate)
        {
#if NET
            ArgumentNullException.ThrowIfNull(items);

            ArgumentNullException.ThrowIfNull(predicate);
#else
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
#endif

            return !AtLeastOnce(items, predicate);
        }

        /// <summary>
        /// Tests if <paramref name="items"/> is empty.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <returns><see langword="true"/> if <paramref name="items"/> does not any element and <see langword="false"/> otherwise.</returns>
        public static bool None<T>(this IEnumerable<T> items) => None(items, True<T>());

        /// <summary>
        /// Tests if <paramref name="items"/> contains exactly one item that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">re</param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains exactly one element that fullfills <paramref name="predicate"/></returns>
        public static bool Once<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate)
        {
#if NET
            ArgumentNullException.ThrowIfNull(items);

            ArgumentNullException.ThrowIfNull(predicate);
#else
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
#endif

            return Exactly(items, predicate, 1);
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains one or more items that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">predicate to use</param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains one or more one element that fullfills <paramref name="predicate"/></returns>
        public static bool AtLeastOnce<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate)
        {
#if NET
            ArgumentNullException.ThrowIfNull(items);

            ArgumentNullException.ThrowIfNull(predicate);
#else
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
#endif

            return items
                .AsParallel()
                .Any(predicate.Compile());
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains one or more items.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains one or more element.s</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="items"/>  <see langword="null"/></exception>
        public static bool AtLeastOnce<T>(this IEnumerable<T> items) => AtLeast(items, True<T>(), 1);

        /// <summary>
        /// Tests if <paramref name="items"/> contains <paramref name="count"/> or more items.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="count"></param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains <paramref name="count"/> or more one elements.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="items"/>  <see langword="null"/></exception>
        public static bool AtLeast<T>(this IEnumerable<T> items, int count) => AtLeast(items, True<T>(), count);

        /// <summary>
        /// Check if <paramref name="source"/> contains at least <paramref name="count"/> elements that match <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// This implementation tries to avoid enumerating <paramref name="source"/>.
        /// </remarks>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">the collection to test</param>
        /// <param name="predicate">the predicate</param>
        /// <param name="count">the number of occurrence</param>
        /// <returns><see langword="true"/> if <paramref name="source"/> contains <paramref name="count"/> elements or more that match <paramref name="predicate"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="predicate"/> are null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative </exception>
        public static bool AtLeast<T>(this IEnumerable<T> source, Expression<Func<T, bool>> predicate, int count)
        {
#if NET
            ArgumentNullException.ThrowIfNull(source);

            ArgumentNullException.ThrowIfNull(predicate);
#else
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
#endif

#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
#else
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{count} is not a valid value");
            }
#endif

            return count == 0
                ? !source.Any()
                : source
                    .AsParallel()
                    .Where(predicate.Compile()).Skip(count - 1).Any();
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains <strong>exactly</strong> <paramref name="count"/> elements that match <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements of</typeparam>
        /// <param name="items">collection under test</param>
        /// <param name="predicate">predicate to match</param>
        /// <param name="count">number of elements in <paramref name="items"/> that must match <paramref name="predicate"/> </param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains <strong>exactly</strong> <paramref name="count"/> elements that match <paramref name="predicate"/> and <see langword="false"/> otherwise</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="items"/> or <paramref name="predicate"/> are null</exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="count"/> is negative.</exception>
        public static bool Exactly<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate, int count)
        {
#if NET
            ArgumentNullException.ThrowIfNull(items);

            ArgumentNullException.ThrowIfNull(predicate);
#else
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
#endif

#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
#else
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{count} is not a valid value");
            }
#endif
            return count == default
                ? !items.AsParallel().Any(predicate.Compile())
                : items.AsParallel().Count(predicate.Compile()) == count;
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains <strong>exactly</strong> <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="T">Type of the elements of</typeparam>
        /// <param name="items">collection under test</param>
        /// <param name="count">number of elements in <paramref name="items"/> </param>
        /// <returns><see langword="true"/> if <paramref name="items"/> contains <strong>exactly</strong> <paramref name="count"/> elements and <see langword="false"/> otherwise</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="items"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="count"/> is negative.</exception>
        public static bool Exactly<T>(this IEnumerable<T> items, int count)
        {
#if NET
            ArgumentNullException.ThrowIfNull(items);
#else
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }
#endif

#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
#else
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{count} is not a valid value");
            }
#endif

            return count == default
                ? !items.Any()
#if NET6_0_OR_GREATER
                : items.TryGetNonEnumeratedCount(out int currentCount) ? currentCount == count
#endif
                : items.Count() == count;
        }

        /// <summary>
        /// Checks if there are <paramref name="count"/> elements at most that match <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements of the collection to test</typeparam>
        /// <param name="items"></param>
        /// <param name="predicate">Filter that <paramref name="count"/> elements should match.</param>
        /// <param name="count">Number of elements that match <paramref name="predicate"/></param>
        /// <returns><see langword="true"/> if there are 0 to <paramref name="count"/> elements that matches <paramref name="predicate"/> and <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="items"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative </exception>
        public static bool AtMost<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate, int count)
        {
#if NET
            ArgumentNullException.ThrowIfNull(items);

            ArgumentNullException.ThrowIfNull(predicate);

#else
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
#endif
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
#else
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{count} is not a valid value.");
            }
#endif
            return (count == 0 && !items.Any()) || items.Count(predicate.Compile()) <= count;
        }

        /// <summary>
        /// Checks if <paramref name="items"/> contains <paramref name="count"/> elements or less.
        /// </summary>
        /// <typeparam name="T">Type of the elements of the collection to test</typeparam>
        /// <param name="items"></param>
        /// <param name="count">Number of elements that the current contains at most</param>
        /// <returns><see langword="true"/> if there are 0 to <paramref name="count"/> elements and <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="items"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative </exception>
        public static bool AtMost<T>(this IEnumerable<T> items, int count) => AtMost(items, True<T>(), count);

        /// <summary>
        /// Performs a cartesian product beetwen <paramref name="first"/> and <paramref name="second"/>.
        /// </summary>
        /// <typeparam name="TFirst">Type of element in <paramref name="first"/> collection.</typeparam>
        /// <typeparam name="TSecond">Type of element in <paramref name="second"/> collection.</typeparam>
        /// <param name="first">the first collection of the cross join</param>
        /// <param name="second">the second collection</param>
        /// <returns></returns>
        public static IEnumerable<(TFirst, TSecond)> CrossJoin<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
            => CrossJoin(first, second, (t1, t2) => (t1, t2));

        /// <summary>
        /// Performs a cartesian product beetwen <paramref name="first"/> and <paramref name="second"/>.
        /// </summary>
        /// <typeparam name="TFirst">Type of element in <paramref name="first"/> collection.</typeparam>
        /// <typeparam name="TSecond">Type of element in <paramref name="second"/> collection.</typeparam>
        /// <typeparam name="TResult">Type of element of the result collection.</typeparam>
        /// <param name="first">the first collection of the cross join</param>
        /// <param name="second">the second collection</param>
        /// <param name="selector">projection to perform on each</param>
        /// <returns></returns>
        public static IEnumerable<TResult> CrossJoin<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
#pragma warning disable RCS1163 // Unused parameter.
            => first?.SelectMany(t1 => second, selector);
#pragma warning restore RCS1163 // Unused parameter.

        /// <summary>
        /// Performs a cartesian product beetwen <paramref name="first"/> and <paramref name="second"/>.
        /// </summary>
        /// <typeparam name="TFirst">Type of element in <paramref name="first"/> collection.</typeparam>
        /// <typeparam name="TSecond">Type of element in <paramref name="second"/> collection.</typeparam>
        /// <typeparam name="TThird">Type of element in <paramref name="third"/> collection.</typeparam>
        /// <param name="first">the first collection of the cross join</param>
        /// <param name="second">the second collection</param>
        /// <param name="third">the third collection</param>
        /// <returns>The cartesian product of <paramref name="first"/>.<paramref name="second"/>.<paramref name="third"/>></returns>
        public static IEnumerable<(TFirst, TSecond, TThird)> CrossJoin<TFirst, TSecond, TThird>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third)
            => CrossJoin(first, second, third, (x, y, z) => (x, y, z));

        /// <summary>
        /// Performs a cartesian product between <paramref name="first"/> and <paramref name="second"/>.
        /// </summary>
        /// <typeparam name="TFirst">Type of element in <paramref name="first"/> collection.</typeparam>
        /// <typeparam name="TSecond">Type of element in <paramref name="second"/> collection.</typeparam>
        /// <typeparam name="TThird">Type of element in <paramref name="third"/> collection.</typeparam>
        /// <typeparam name="TResult">Type of element of the result collection.</typeparam>
        /// <param name="first">the first collection of the cross join</param>
        /// <param name="second">the second collection</param>
        /// <param name="third">the third collection</param>
        /// <param name="selector">projection to perform</param>
        /// <returns></returns>
        public static IEnumerable<TResult> CrossJoin<TFirst, TSecond, TThird, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third, Func<TFirst, TSecond, TThird, TResult> selector) =>
#pragma warning disable RCS1163 // Unused parameter.
            first.SelectMany(x => second.SelectMany(_ => third, (y, z) => selector(x, y, z)));
#pragma warning restore RCS1163 // Unused parameter.

        /// <summary>
        /// Synchronously iterates over source an execute the <paramref name="body"/> action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">
        /// </param>
        /// <param name="body">
        ///     code to be execute the <paramref name="body"/> action on each item of the <paramref name="source" />
        /// </param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> body)
        {
            source.ForEach(BodyWithIndex);

            return;

            void BodyWithIndex(T item, int _) => body(item);
        }

        /// <summary>
        /// Synchronously iterates over source an execute the <paramref name="body"/> action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">
        /// </param>
        /// <param name="body">
        ///     code to be execute the <paramref name="body"/> action on each item of the <paramref name="source" />
        /// </param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> body)
        {
            IList<Exception> exceptions = null;
            int index = 0;
            using IEnumerator<T> enumerator = source.GetEnumerator();

            while (enumerator.MoveNext())
            {
                try
                {
                    body(enumerator.Current, index);
                }
                catch (Exception exc)
                {
                    (exceptions ??= new List<Exception>()).Add(exc);
                }
                finally
                {
                    index++;
                }
            }

            if (exceptions?.AtLeastOnce() ?? false)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Splits <paramref name="source"/> into buckets so that each one holds <paramref name="bucketSize"/> elements at most
        /// </summary>
        /// <typeparam name="T">Type of elements that <paramref name="source"/> contains </typeparam>
        /// <param name="source"></param>
        /// <param name="bucketSize">number of elements that each bucket will hold at most.</param>
        /// <returns>Collection of "buckets" where each bucket are</returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int bucketSize)
        {
#if NET
            ArgumentNullException.ThrowIfNull(source);
#endif

#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(bucketSize, 0);
#else
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (bucketSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bucketSize), bucketSize, "Bucket size must be greater than 0");
            }
#endif
            return PartitionInternal();

            IEnumerable<IEnumerable<T>> PartitionInternal()
            {
                if (bucketSize > 0)
                {
                    using IEnumerator<T> enumerator = source.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IList<T> bucket = new List<T>(bucketSize);

                        int i = 0;
                        do
                        {
                            bucket.Add(enumerator.Current);
                            i++;
                        } while (i < bucketSize && enumerator.MoveNext());

                        yield return bucket;
                    }
                }
            }
        }

        /// <summary>
        /// Sorts <paramref name="source"/> into 2 groups
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns>
        /// A tuple that contains two <see cref="IEnumerable{T}"/> parts :
        /// <list type="bullet">
        /// <item><c>Truthy</c> : that contains all items from <paramref name="source"/> that satifies <paramref name="predicate"/>.</item>
        /// <item><c>Falsy</c> : that contains all items from <paramref name="source"/> that do not satifies <paramref name="predicate"/>.</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">if either <paramref name="source"/> or <paramref name="predicate"/>is
        /// <see langword="null"/>.</exception>
        public static (IEnumerable<T> Thruthy, IEnumerable<T> Falsy) SortBy<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            => (source.Where(predicate), source.Where(val => !predicate(val)));

        /// <summary>
        /// Asynchronously iterates over each element of <paramref name="source"/> and applies the specified <paramref name="body"/> function.
        /// </summary>
        /// <remarks>
        /// If <paramref name="degreeOfParallelism"/> is not provided, this method will relies on the value returned by <see cref="Environment.ProcessorCount"/>
        /// in order to run <paramref name="body"/> in a concurrent manner.
        /// </remarks>
        /// <typeparam name="T">The type of elements <paramref name="source"/> contains</typeparam>
        /// <param name="source">The collection to iterate over</param>
        /// <param name="body">A function that will be called asynchronously</param>
        /// <param name="degreeOfParallelism">Defines the degree of parallelism to use when running</param>
        /// <returns>A <see cref="Task"/> that will be completed as soon as <paramref name="source"/> was fully iterated</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="degreeOfParallelism"/> is negative or <c>0</c></exception>
        /// <exception cref="ArgumentNullException">either <paramref name="source"/> or <paramref name="body"/> is <see langword="null"/></exception>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int? degreeOfParallelism = null)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source)
                    .GetPartitions(degreeOfParallelism ?? Environment.ProcessorCount / 2)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current)
                                .ConfigureAwait(false);
                        }
                    }
                }));
        }

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
        /// <summary>
        /// Converts a <see cref="IEnumerable{T}"/> to its <see cref="IAsyncEnumerable{T}"/> counterpart.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">
        /// The collection to convert.
        /// </param>
        /// <param name="millisecondsDelay">
        /// Delay between each iteration of <paramref name="source"/>.
        /// The first element of source will be available after at least <paramref name="millisecondsDelay"/> milliseconds,
        /// and then each one after <paramref name="millisecondsDelay"/> milliseconds.
        /// </param>
        /// <returns>
        /// <see cref="IAsyncEnumerable{T}"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="millisecondsDelay"/> is less than <c>0</c>.</exception>
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source, int millisecondsDelay = 1)
        {
#if NET
            ArgumentNullException.ThrowIfNull(source);
#else
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
#endif
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsDelay, 0);
#else
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), millisecondsDelay, "cannot be negative");
            }
#endif

            async IAsyncEnumerable<T> AsAsyncEnumerableIterator()
            {
                foreach (var item in source)
                {
                    await Task.Delay(millisecondsDelay).ConfigureAwait(false);
                    yield return item;
                }
            }

            return AsAsyncEnumerableIterator();
        }
#endif
    }
}