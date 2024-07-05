// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using Candoumbe.MiscUtilities.UnitTests.Generators;
using FluentAssertions;
using FluentAssertions.Equivalency;
using FsCheck;
using FsCheck.Xunit;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests;

[UnitTest]
public class ArrayExtensionsTests
{
    [Property(Arbitrary = [typeof(ValueGenerators)])]
    public void Given_any_non_empty_array_ToArray_should_produce_an_array_of_the_same_length(NonNull<Array> source)
    {
        // Arrange
        Array array = source.Item;

        // Act
        object[] result = array.ToArray<object>();

        // Assert
        result.Should().HaveCount(array.Length);

        for (int i = 0; i < result.Length; i++)
        {
            result[i].Should().Be(array.GetValue(i));
        }
    }
}
