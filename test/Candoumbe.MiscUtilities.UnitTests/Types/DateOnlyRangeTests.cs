// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using Bogus;

using Candoumbe.MiscUtilities.Types;
using Candoumbe.MiscUtilities.UnitTests.Generators;

using FluentAssertions;
using FluentAssertions.Equivalency;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using System;

using Xunit.Abstractions;
using Xunit.Categories;

namespace Candoumbe.MiscUtilities.UnitTests.Types;
#if NET6_0_OR_GREATER
[UnitTest]
public class DateOnlyRangeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly static Faker faker = new();

    public DateOnlyRangeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
    public void Given_start_gt_end_Constructor_should_throw_ArgumentOutOfRangeException(DateOnly reference)
    {
        // Arrange
        DateOnly lower = faker.Date.PastDateOnly(refDate: reference);
        DateOnly upper = reference;

        // Act
        Action ctor = () => new DateOnlyRange(start: upper, end: lower);

        // Assert
        ctor.Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
    public void Given_two_non_empty_DateOnlyRange_that_are_equals_Overlaps_should_return_true(DateOnly reference)
    {
        // Arrange
        DateOnly start = reference;
        DateOnly end = faker.Date.FutureDateOnly(refDate: reference);

        DateOnlyRange first = new(start, end);
        DateOnlyRange other = new(start, end);

        // Act
        bool overlaps = first.Overlaps(other);

        // Assert
        overlaps.Should()
                .BeTrue("Two DateOnly ranges that are equal overlaps");
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Given_two_DateOnlyRange_instances_Overlaps_should_be_symetric(DateOnlyRange left, DateOnlyRange right)
    {
        _outputHelper.WriteLine($"{nameof(left)}: {left}");
        _outputHelper.WriteLine($"{nameof(right)}: {right}");

        return (left.Overlaps(right) == right.Overlaps(left)).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]

    public void Given_two_non_empty_DateOnlyRange_instances_when_first_ends_where_other_starts_Abuts_should_return_true(DateOnly reference)
    {
        // Arrange
        DateOnly start = faker.Date.PastDateOnly(refDate: reference);
        DateOnly end = faker.Date.FutureDateOnly(refDate: reference);

        DateOnlyRange current = new(start, reference);
        DateOnlyRange other = new(reference, end);

        // Act
        bool isContiguous = current.IsContiguousWith(other);

        // Assert
        isContiguous.Should()
            .BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Given_two_DateOnlyRange_instances_IsContiguous_should_be_symetric(DateOnlyRange left, DateOnlyRange right)
    {
        _outputHelper.WriteLine($"{nameof(left)}: {left}");
        _outputHelper.WriteLine($"{nameof(right)}: {right}");

        return (left.IsContiguousWith(right) == right.IsContiguousWith(left)).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateOnlyRange_instance_When_Start_eq_End_IsEmpty_should_be_True(DateOnly reference)
    {
        // Arrange
        DateOnlyRange current = new(reference, reference);

        // Act
        bool isEmpty = current.IsEmpty();

        // Assert
        isEmpty.Should()
               .BeTrue();
    }
}
#endif
