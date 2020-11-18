﻿using FluentAssertions;
using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Utilities.UnitTests
{
    [UnitTest]
    public class ArrayEqualityComparerTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ArrayEqualityComparerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> ArrayOfByteEqualCases
        {
            get
            {
                yield return new object[]
                {
                    Array.Empty<byte>(),
                    null,
                    false,
                    "comparing empty array of bytes to null"
                };

                yield return new object[]
                {
                    Array.Empty<byte>(),
                    Array.Empty<byte>(),
                    true,
                    "comparing Array.Empty<byte>() to Array.Empty<byte>()"
                };

                yield return new object[]
                {
                    new byte[]{1, 2, 3 },
                    new byte[]{1, 2, 3 },
                    true,
                    "both arrays are of the same size and contains same data in the same order"
                };

                yield return new object[]
                {
                    new byte[]{1, 2, 3 },
                    new byte[]{1, 3, 3 },
                    false,
                    "both arrays are of the same size but the data at not the same at all positions"
                };

                yield return new object[]
                {
                    new byte[]{1, 2, 3 },
                    new byte[]{1, 2, 3, 4 },
                    false,
                    "arrays are not of the same size"
                };

                yield return new object[]
                {
                    null,
                    null,
                    false,
                    "comparing null to null is inconclusive and should be false by default"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ArrayOfByteEqualCases))]
        public void TestEquals(byte[] first, byte[] other, bool expected, string reason)
            => TestEqual(first, other, expected, reason);

        private void TestEqual<T>(T[] first, T[] second, bool expected, string reason)
        {
            _outputHelper.WriteLine($"First : '{first.Jsonify()}'");
            _outputHelper.WriteLine($"Second : '{second.Jsonify()}'");

            ArrayEqualityComparer<T> comparer = new ArrayEqualityComparer<T>();

            // Act
            bool actual = comparer.Equals(first, second);
            int firstHashCode = comparer.GetHashCode(first);
            int secondHashCode = comparer.GetHashCode(second);

            // Assert
            actual.Should()
                  .Be(expected, reason);

            if (expected)
            {
                firstHashCode.Should()
                             .Be(secondHashCode, reason);
            }
        }
    }
}