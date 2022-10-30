// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if NET6_0_OR_GREATER
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
public class DateOnlyRangeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly static Faker faker = new();

    public DateOnlyRangeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_start_gt_end_Constructor_should_feed_Properties_accordingly(DateOnly start)
    {
        // Arrange
        DateOnly end = start.AddDays(1);

        // Act
        Action action = () => new DateOnlyRange(start: end, end: start);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>("start cannot be greater than end")
                       .Where(ex => !string.IsNullOrWhiteSpace(ex.Message))
                       .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_start_and_end_Constructor_should_feed_Properties_accordingly(DateOnly start)
    {
        // Arrange
        DateOnly end = faker.Date.FutureDateOnly(refDate: start);

        // Act
        DateOnlyRange range = new(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_non_empty_DateOnlyRange_that_are_equals_Overlaps_should_return_true(DateOnly reference)
    {
        // Arrange
        DateOnly end = reference;
        DateOnly start = faker.Date.RecentDateOnly(refDate: reference);

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
    public void Given_two_non_empty_DateOnlyRange_instances_when_first_ends_where_other_starts_Abuts_should_return_true(NonNull<DateOnlyRange> nonNullLeft, NonNull<DateOnlyRange> nonNullRight)
    {
        // Arrange
        DateOnlyRange left = nonNullLeft.Item;
        DateOnlyRange right = nonNullRight.Item;

        // Act
        bool isContiguous = left.IsContiguousWith(right);

        // Assert
        isContiguous.Should()
                    .Be(left.Start == right.End || right.Start == left.End);
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

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateOnly_value_UpTo_should_build_a_DateOnlyRange_up_to_that_value(DateOnly reference)
    {
        // Act
        DateOnlyRange range = DateOnlyRange.UpTo(reference);

        // Assert
        range.Start.Should()
                   .Be(DateOnly.MinValue);
        range.End.Should()
                 .Be(reference);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateOnly_value_DownTo_should_build_a_DateOnlyRange_down_to_that_value(DateOnly reference)
    {
        // Act
        DateOnlyRange range = DateOnlyRange.DownTo(reference);

        // Assert
        range.Start.Should()
                   .Be(reference);
        range.End.Should()
                 .Be(DateOnly.MaxValue);
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
                new DateOnlyRange(DateOnly.FromDateTime(1.April(1832)), DateOnly.FromDateTime(5.April(1945))),
                new DateOnlyRange(DateOnly.FromDateTime(3.April(1888)), DateOnly.FromDateTime(5.April(1950))),
                true
            };

            /* 
             * first: |---------------|
             * other:                     |---------------| 
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.April(1832)), DateOnly.FromDateTime(5.April(1945))),
                new DateOnlyRange(DateOnly.FromDateTime(3.July(1970)), DateOnly.FromDateTime(5.April(1980))),
                false
            };

            /* 
             * first: |---------------|
             * other:                 |---------------| 
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.April(1832)), DateOnly.FromDateTime(5.April(1945))),
                new DateOnlyRange(DateOnly.FromDateTime(5.April(1945)), DateOnly.FromDateTime(5.April(1950))),
                false
            };

            /* 
             * first:         |--------|
             * other:      |---------------| 
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.April(1832)), DateOnly.FromDateTime(5.April(1945))),
                new DateOnlyRange(DateOnly.FromDateTime(14.July(1789)), DateOnly.FromDateTime(5.April(1950))),
                true
            };
        }
    }

    [Theory]
    [MemberData(nameof(OverlapsCases))]
    public void Given_two_instances_Overlaps_should_behave_as_expected(DateOnlyRange left, DateOnlyRange right, bool expected)
    {
        // Act
        bool actual = left.Overlaps(right);

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Overlaps_should_be_symetric(DateOnlyRange left, DateOnlyRange right)
        => (left.Overlaps(right) == right.Overlaps(left)).ToProperty();

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_AllTime_when_testing_overlap_with_any_other_DateOnlyRange_Overlaps_should_be_true(DateOnlyRange other)
    {
        // Act
        bool actual = DateOnlyRange.Infinite.Overlaps(other);

        // Assert
        actual.Should()
              .BeTrue($"AllTime range overlaps every other {nameof(DateOnlyRange)}s");
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
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(4.January(1990)), DateOnly.FromDateTime(8.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(8.January(1990))),
            };

            /* 
             * current   :         |---------------| 
             * other     : |---------------|
             * expected  : |-----------------------| 
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(4.January(1990)), DateOnly.FromDateTime(8.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(8.January(1990))),
            };

            /* 
             * current   :                 |---------------| 
             * other     : |---------------|
             * expected  : |-------------------------------| 
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(6.January(1990)), DateOnly.FromDateTime(8.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(8.January(1990))),
            };

            /* 
             * current     : |---------------|
             * other       :                 |---------------| 
             * expected    : |-------------------------------| 
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(6.January(1990)), DateOnly.FromDateTime(8.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(8.January(1990))),
            };

            /* 
             * current     : |---------------------|
             * other       :         |---------| 
             * expected    : |---------------------|
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(3.January(1990)), DateOnly.FromDateTime(5.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
            };

            /* 
             * current     : ---------------------
             * other       :         |---------| 
             * expected    : ---------------------
             */
            yield return new object[]
            {
                DateOnlyRange.Infinite,
                new DateOnlyRange(DateOnly.FromDateTime(3.January(1990)), DateOnly.FromDateTime(5.January(1990))),
                DateOnlyRange.Infinite,
            };
        }
    }

    [Theory]
    [MemberData(nameof(UnionCases))]
    public void Given_two_instances_Union_should_behave_as_expected(DateOnlyRange current, DateOnlyRange other, DateOnlyRange expected)
    {
        // Act
        DateOnlyRange actual = current.Union(other);
        _outputHelper.WriteLine($"Result: {actual}");

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_non_empty_TimeOnlyRange_When_merging_with_empty_range_Union_should_returns_the_non_empty_range(DateOnlyRange range, DateOnly date)
    {
        // Arrange
        DateOnlyRange empty = new(date, date);

        // Act
        DateOnlyRange union = range.Union(empty);

        // Assert
        union.Should().Be(range);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_non_empty_TimeOnlyRange_When_merging_with_an_other_TimeOnlyRange_that_does_not_overlaps_nor_is_contiguous_Union_should_throw_InvalidOperationException(DateOnly date)
    {
        // Arrange
        DateOnlyRange left = new(date.AddDays(1), faker.Date.FutureDateOnly(refDate: date.AddDays(2)));
        DateOnlyRange right = new(faker.Date.RecentDateOnly(refDate: date.AddDays(-2)), date.AddDays(-1));

        _outputHelper.WriteLine($"{nameof(left)} : {left}");
        _outputHelper.WriteLine($"{nameof(right)} : {right}");

        // Act
        Action callingUnionWhenLeftAndRightDontOverlapsAndAreNotContiguous = () => left.Union(right);

        // Assert
        callingUnionWhenLeftAndRightDontOverlapsAndAreNotContiguous.Should()
                                                                   .Throw<InvalidOperationException>()
                                                                   .Which.Message.Should()
                                                                   .NotBeNullOrWhiteSpace();
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
                DateOnlyRange.Empty,
                DateOnlyRange.Empty,
                DateOnlyRange.Empty
            };

            /*
             * current   :  |-----------|
             * other     :          |------------|
             * expected  :          |---|
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(4.January(1990)), DateOnly.FromDateTime(8.February(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(4.January(1990)), DateOnly.FromDateTime(6.January(1990))),
            };

            /*
             * current   :          |------------|
             * other     :  |-----------|
             * expected  :          |---|
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(3.January(1990)), DateOnly.FromDateTime(5.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(4.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(3.January(1990)), DateOnly.FromDateTime(4.January(1990))),
            };

            /*
             * current   :  |-----------|
             * other     :      |-----|
             * expected  :      |-----|
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(3.January(1990)), DateOnly.FromDateTime(5.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(3.January(1990)), DateOnly.FromDateTime(5.January(1990))),
            };

            /*
             * current   :  |----|
             * other     :          |------------|
             * expected  :  |
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                new DateOnlyRange(DateOnly.FromDateTime(18.February(1990)), DateOnly.FromDateTime(25.July(1990))),
                DateOnlyRange.Empty
            };

            /*
             * current   :  |----|
             * other     :  ----------------------
             * expected  :  |----|
             */
            yield return new object[]
            {
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
                DateOnlyRange.Infinite,
                new DateOnlyRange(DateOnly.FromDateTime(1.January(1990)), DateOnly.FromDateTime(6.January(1990))),
            };
        }
    }

    [Theory]
    [MemberData(nameof(IntersectCases))]
    public void Given_two_instances_Intersect_should_return_the_intersection(DateOnlyRange current, DateOnlyRange other, DateOnlyRange expected)
    {
        // Act
        DateOnlyRange intersection = current.Intersect(other);

        // Assert
        intersection.Should()
                    .NotBeNull().And
                    .Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Intersect_should_be_symetric(DateOnlyRange left, DateOnlyRange right)
        => (left.Intersect(right) == right.Intersect(left)).ToProperty();

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Empty_should_be_the_neutral_element_of_DateOnlyRange(DateOnlyRange range)
    {
        // Act
        DateOnlyRange result = range.Union(DateOnlyRange.Empty);

        // Assert
        result.Should()
              .Be(range);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateOnlyRange_is_empty_When_value_is_anything_Contains_should_returns_Inconclusive(DateOnly date)
    {
        // Act
        ContainsResult result = DateOnlyRange.Empty.Contains(date);

        // Assert
        result.Should().Be(ContainsResult.No, $"The {nameof(DateOnlyRange)} is empty");
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateOnlyRange_is_infinite_When_value_anything_Contains_should_returns_Yes(DateOnly date)
    {
        // Act
        ContainsResult result = DateOnlyRange.Infinite.Contains(date);

        // Assert
        result.Should().Be(ContainsResult.Yes, $"The {nameof(DateOnlyRange.Infinite)} 7678333contains all {nameof(DateOnly)} values");
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_DateOnlyRange_is_not_empty_and_not_infinite_When_value_is_between_Start_and_End_Contains_should_returns_Yes(DateOnly date)
    {
        // Arrange
        DateOnly start = faker.PickRandom(faker.Date.RecentDateOnly(refDate: date),
                                          faker.Date.PastDateOnly(refDate: date));

        DateOnly end = faker.PickRandom(faker.Date.SoonDateOnly(refDate: date),
                                        faker.Date.FutureDateOnly(refDate: date));

        DateOnlyRange dateRange = (start == end) switch
        {
            true => new DateOnlyRange(start.AddDays(-1), end.AddDays(1)),
            _ => new DateOnlyRange(start, end)
        };

        // Act
        ContainsResult result = dateRange.Contains(date);

        // Assert
        result.Should().Be(ContainsResult.Yes, $"{dateRange} contains {date} value");
    }

    [Fact]
    public void Given_DateOnlyRange_is_not_empty_and_not_infinite_When_value_is_not_between_Start_and_End_Contains_should_returns_No()
    {
        // Arrange
        DateOnly start = DateOnly.FromDateTime(12.July(2012));
        DateOnly end = DateOnly.FromDateTime(16.July(2012));

        DateOnlyRange dateRange = new(start, end);

        DateOnly value = faker.PickRandom(faker.Date.RecentDateOnly(refDate: start.AddDays(-1)),
                                          faker.Date.PastDateOnly(refDate: start.AddDays(-1)),
                                          faker.Date.SoonDateOnly(refDate: end.AddDays(1)),
                                          faker.Date.FutureDateOnly(refDate: end.AddDays(1)));

        // Act
        ContainsResult result = dateRange.Contains(value);

        // Assert
        result.Should().Be(ContainsResult.No, $"{dateRange} does not contains {value} value");
    }
}
#endif
