// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using Bogus;

using Candoumbe.MiscUtilities.Types;
using Candoumbe.MiscUtilities.UnitTests.Generators;

using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentAssertions.Extensions;

using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

using System;
using System.Collections.Generic;

using Xunit;
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

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_start_gt_end_Constructor_should_feed_Properties_accordingly(DateTime start)
    {
        // Arrange
        DateTime end = start.AddDays(1);

        // Act
        Action action = () => new DateTimeRange(start: end, end: start);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>("start cannot be greater than end");
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_start_and_end_Constructor_should_feed_Properties_accordingly(DateTime start)
    {
        // Arrange
        DateTime end = faker.Date.Future(refDate: start);

        // Act
        DateTimeRange range = new(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_non_empty_DateTimeRange_that_are_equals_Overlaps_should_return_true(DateTime reference)
    {
        // Arrange
        DateTime end = reference;
        DateTime start = faker.Date.Recent(refDate: reference);

        DateTimeRange first = new(start, end);
        DateTimeRange other = new(start, end);

        // Act
        bool overlaps = first.Overlaps(other);

        // Assert
        overlaps.Should()
                .BeTrue("Two DateTime ranges that are equal overlaps");
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
        DateTime start = faker.Date.Recent(refDate: reference);
        DateTime end = reference;

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

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
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

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateTime_value_UpTo_should_build_a_DateTimeRange_up_to_that_value(DateTime reference)
    {
        // Act
        DateTimeRange range = DateTimeRange.UpTo(reference);

        // Assert
        range.Start.Should()
                   .Be(DateTime.MinValue);
        range.End.Should()
                 .Be(reference);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateTime_value_DownTo_should_build_a_DateTimeRange_down_to_that_value(DateTime reference)
    {
        // Act
        DateTimeRange range = DateTimeRange.DownTo(reference);

        // Assert
        range.Start.Should()
                   .Be(reference);
        range.End.Should()
                 .Be(DateTime.MaxValue);
    }

    public static IEnumerable<object[]> OverlapsCases
    {
        get
        {
            /* 
             * first: |---------------|
             * other:         |---------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(1.April(1832), 5.April(1945)),
                new DateTimeRange(3.April(1888), 5.April(1950)),
                true
            };

            /* 
             * first: |---------------|
             * other:                     |---------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(1.April(1832), 5.April(1945)),
                new DateTimeRange(3.July(1970), 5.April(1980)),
                false
            };

            /* 
             * first: |---------------|
             * other:                 |---------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(1.April(1832), 5.April(1945)),
                new DateTimeRange(5.April(1945), 5.April(1950)),
                false
            };

            /* 
             * first:         |--------|
             * other:      |---------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(1.April(1832), 5.April(1945)),
                new DateTimeRange(14.July(1789), 5.April(1950)),
                true
            };
        }
    }

    [Theory]
    [MemberData(nameof(OverlapsCases))]
    public void Given_two_instances_Overlaps_should_behave_as_expected(DateTimeRange left, DateTimeRange right, bool expected)
    {
        // Act
        bool actual = left.Overlaps(right);

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Overlaps_should_be_symetric(DateTimeRange left, DateTimeRange right)
        => (left.Overlaps(right) == right.Overlaps(left)).ToProperty();

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_AllTime_when_testing_overlap_with_any_other_DateTimeRange_Overlaps_should_be_true(DateTimeRange other)
    {
        // Act
        bool actual = DateTimeRange.Infinite.Overlaps(other);

        // Assert
        actual.Should()
              .BeTrue($"AllTime range overlaps every other {nameof(DateTimeRange)}s");
    }

    public static IEnumerable<object[]> UnionCases
    {
        get
        {
            /* 
             * curernt   : |---------------|
             * other     :         |---------------| 
             * expected  : |-----------------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(4.January(1990), 8.January(1990)),
                new DateTimeRange(1.January(1990), 8.January(1990)),
            };

            /* 
             * current   :         |---------------| 
             * other     : |---------------|
             * expected  : |-----------------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(4.January(1990), 8.January(1990)),
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(1.January(1990), 8.January(1990)),
            };

            /* 
             * current   :                 |---------------| 
             * other     : |---------------|
             * expected  : |-------------------------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(6.January(1990), 8.January(1990)),
                new DateTimeRange(1.January(1990), 8.January(1990)),
            };

            /* 
             * current     : |---------------|
             * other       :                 |---------------| 
             * expected    : |-------------------------------| 
             */
            yield return new object[]
            {
                new DateTimeRange(6.January(1990), 8.January(1990)),
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(1.January(1990), 8.January(1990)),
            };

            /* 
             * current     : |---------------------|
             * other       :         |---------| 
             * expected    : |---------------------|
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(3.January(1990), 5.January(1990)),
                new DateTimeRange(1.January(1990), 6.January(1990)),
            };
        }
    }

    [Theory]
    [MemberData(nameof(UnionCases))]
    public void Given_two_instances_Union_should_behave_as_expected(DateTimeRange current, DateTimeRange other, DateTimeRange expected)
    {
        // Act
        DateTimeRange actual = current.Union(other);
        _outputHelper.WriteLine($"Result: {actual}");

        // Assert
        actual.Should().Be(expected);
    }

    public static IEnumerable<object[]> IntersectCases
    {
        get
        {
            /*
             * current  :     |
             * other    :  |
             * expected :  |
             */
            yield return new object[]
            {
                DateTimeRange.Empty,
                DateTimeRange.Empty,
                DateTimeRange.Empty
            };

            /*
             * current   :  |-----------|
             * other     :          |------------|
             * expected  :          |---|
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(4.January(1990), 8.February(1990)),
                new DateTimeRange(4.January(1990), 6.January(1990)),
            };

            /*
             * current   :          |------------|
             * other     :  |-----------|
             * expected  :          |---|
             */
            yield return new object[]
            {
                new DateTimeRange(3.January(1990), 5.January(1990)),
                new DateTimeRange(1.January(1990), 4.January(1990)),
                new DateTimeRange(3.January(1990), 4.January(1990)),
            };

            /*
             * current   :  |-----------|
             * other     :      |-----|
             * expected  :      |-----|
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(3.January(1990), 5.January(1990)),
                new DateTimeRange(3.January(1990), 5.January(1990)),
            };

            /*
             * current   :  |----|
             * other     :          |------------|
             * expected  :  |
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                new DateTimeRange(18.February(1990), 25.July(1990)),
                DateTimeRange.Empty
            };

            /*
             * current   :  |----|
             * other     :  ----------------------
             * expected  :  |----|
             */
            yield return new object[]
            {
                new DateTimeRange(1.January(1990), 6.January(1990)),
                DateTimeRange.Infinite,
                new DateTimeRange(1.January(1990), 6.January(1990)),
            };
        }
    }

    [Theory]
    [MemberData(nameof(IntersectCases))]
    public void Given_two_instances_Intersect_should_return_the_intersection(DateTimeRange current, DateTimeRange other, DateTimeRange expected)
    {
        // Act
        DateTimeRange intersection = current.Intersect(other);

        // Assert
        intersection.Should()
                    .NotBeNull().And
                    .Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Intersect_should_be_symetric(DateTimeRange left, DateTimeRange right)
        => (left.Intersect(right) == right.Intersect(left)).ToProperty();

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Empty_should_be_the_neutral_element_of_DateTimeRange(DateTimeRange range)
    {
        // Act
        DateTimeRange result = range.Union(DateTimeRange.Empty);

        // Assert
        result.Should()
              .Be(range);
    }
}