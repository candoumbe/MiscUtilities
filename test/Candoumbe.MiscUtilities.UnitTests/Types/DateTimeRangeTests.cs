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

[UnitTest]
public class DateTimeRangeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly static Faker faker = new();

    public DateTimeRangeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Property]
    public void Given_start_gt_end_Constructor_should_throw_ArgumentOutOfRangeException(DateTime reference)
    {
        // Arrange
        DateTime lower = faker.Date.Past(refDate: reference);
        DateTime upper = reference;

        // Act
        Action ctor = () => new DateTimeRange(start: upper, end: lower);

        // Assert
        ctor.Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Property]
    public void Given_two_non_empty_DateTimeRange_that_are_equals_Overlaps_should_return_true(DateTime reference)
    {
        // Arrange
        DateTime start = reference;
        DateTime end = faker.Date.Future(refDate : reference);

        DateTimeRange first = new (start, end);
        DateTimeRange other = new (start, end);

        // Act
        bool overlaps = first.Overlaps(other);

        // Assert
        overlaps.Should()
                .BeTrue("Two datetime ranges that are equal overlaps");
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Given_two_DateTimeRange_instances_Overlaps_should_be_symetric(DateTimeRange left, DateTimeRange right)
    {
        _outputHelper.WriteLine($"{nameof(left)}: {left}");
        _outputHelper.WriteLine($"{nameof(right)}: {right}");

        return (left.Overlaps(right) == right.Overlaps(left)).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]

    public void Given_two_non_empty_DateTimeRange_instances_when_first_ends_where_other_starts_Abuts_should_return_true(DateTime reference)
    {
        // Arrange
        DateTime start = faker.Date.Past(refDate: reference);
        DateTime end = faker.Date.Future(refDate: reference);

        DateTimeRange current = new(start, reference);
        DateTimeRange other = new(reference, end);

        // Act
        bool isContiguous = current.IsContiguousWith(other);

        // Assert
        isContiguous.Should()
            .BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Given_two_DateTimeRange_instances_IsContiguous_should_be_symetric(DateTimeRange left, DateTimeRange right)
    {
        _outputHelper.WriteLine($"{nameof(left)}: {left}");
        _outputHelper.WriteLine($"{nameof(right)}: {right}");

        return (left.IsContiguousWith(right) == right.IsContiguousWith(left)).ToProperty();
    }

    [Property]
    public void Given_DateTimeRange_instance_When_Start_eq_End_IsEmpty_should_be_True(DateTime reference)
    {
        // Arrange
        DateTimeRange current = new(reference, reference);

        // Act
        bool isEmpty = current.IsEmpty();

        // Assert
        isEmpty.Should()
               .BeTrue();
    }
}