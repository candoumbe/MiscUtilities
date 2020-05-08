﻿#if !(NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD2_0)
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class AsyncEnumerable
    {
        public static IAsyncEnumerable<T> Empty<T>() => new EmptyAsyncEnumerable<T>();

        class EmptyAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            public static readonly EmptyAsyncEnumerator<T> Instance = new EmptyAsyncEnumerator<T>();

            public T Current => default!;

            public ValueTask DisposeAsync() => default;

            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(false);
        }


        class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new EmptyAsyncEnumerator<T>();
            }
        }


    }
}

#endif