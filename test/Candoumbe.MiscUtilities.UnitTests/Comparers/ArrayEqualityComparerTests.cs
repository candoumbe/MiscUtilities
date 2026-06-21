// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using Candoumbe.MiscUtilities.Comparers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests.Comparers;

[UnitTest]
public class ArrayEqualityComparerTests(ITestOutputHelper outputHelper)
{
    public static TheoryData<byte[], byte[], bool, string> ArrayOfByteEqualCases
        => new()
        {
            { [], null, false, "comparing empty array of bytes to null" },
            { [], [], true, "comparing Array.Empty<byte>() to Array.Empty<byte>()" },
            { [1, 2, 3], [1, 2, 3], true, "both arrays are of the same size and contains same data in the same order" },
            { [1, 2, 3], [1, 3, 3], false, "both arrays are of the same size but the data at not the same at all positions" },
            { [1, 2, 3], [1, 2, 3, 4], false, "arrays are not of the same size" },
            { null, null, false, "comparing null to null is inconclusive and should be false by default" }
        };

    [Theory]
    [MemberData(nameof(ArrayOfByteEqualCases))]
    public void TestEquals(byte[] first, byte[] other, bool expected, string reason)
        => TestEqual(first, other, expected, reason);

    private void TestEqual<T>(T[] first, T[] second, bool expected, string reason)
    {
        outputHelper.WriteLine($"First : '{first.Jsonify()}'");
        outputHelper.WriteLine($"Second : '{second.Jsonify()}'");

        ArrayEqualityComparer<T> comparer = new();

        // Act
        bool actual = comparer.Equals(first, second);
        // Assert
        actual.Should()
            .Be(expected, reason);

        if (expected && first is not null && second is not null)
        {
            int firstHashCode = comparer.GetHashCode(first);
            int secondHashCode = comparer.GetHashCode(second);

            firstHashCode.Should()
                .Be(secondHashCode, reason);
        }
    }
}