#if !NETSTANDARD2_0
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// A static
    /// </summary>
    public static class AsyncEnumerable
    {
        /// <summary>
        /// An empty <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAsyncEnumerable<T> Empty<T>() => new EmptyAsyncEnumerable<T>();

        internal sealed class EmptyAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            public static readonly EmptyAsyncEnumerator<T> Instance = new();

            public T Current => default!;

            public ValueTask DisposeAsync() => default;

            public ValueTask<bool> MoveNextAsync() => new(false);
        }

        internal sealed class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new EmptyAsyncEnumerator<T>();
            }
        }
    }
}

#endif