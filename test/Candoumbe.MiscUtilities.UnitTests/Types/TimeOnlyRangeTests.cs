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
#if NET6_0_OR_GREATER
[UnitTest]
public class TimeOnlyRangeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly static Faker faker = new();

    public TimeOnlyRangeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
    public void Given_start_and_end_Constructor_should_feed_Properties_accordingly(TimeOnly start, TimeOnly end)
    {
        // Act
        TimeOnlyRange range = new (start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
    public void Given_two_non_empty_TimeOnlyRange_that_are_equals_Overlaps_should_return_true(TimeOnly reference)
    {
        // Arrange
        TimeOnly end = reference;
        TimeOnly start = faker.Date.RecentTimeOnly(mins: (int)(reference - TimeOnly.MinValue).TotalMinutes, refTime: reference) ;

        TimeOnlyRange first = new(start, end);

        if (first.IsEmpty())
        {
            first = first.Union(new TimeOnlyRange(first.End, first.End.Add(1.Milliseconds())));
        }

        TimeOnlyRange other = new(first.Start, first.End);

        _outputHelper.WriteLine($"First : {first}");
        _outputHelper.WriteLine($"Other : {other}");

        // Act
        bool overlaps = first.Overlaps(other);

        // Assert
        overlaps.Should()
                .BeTrue("Two TimeOnly ranges that are equal overlaps");
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Given_two_TimeOnlyRange_instances_Overlaps_should_be_symetric(TimeOnlyRange left, TimeOnlyRange right)
    {
        _outputHelper.WriteLine($"{nameof(left)}: {left}");
        _outputHelper.WriteLine($"{nameof(right)}: {right}");

        return (left.Overlaps(right) == right.Overlaps(left)).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_non_empty_TimeOnlyRange_instances_when_first_ends_where_other_starts_Abuts_should_return_true(NonNull<TimeOnlyRange> nonNullLeft, NonNull<TimeOnlyRange> nonNullRight)
    {
        // Arrange
        TimeOnlyRange left = nonNullLeft.Item;
        TimeOnlyRange right = nonNullRight.Item;

        // Act
        bool isContiguous = left.IsContiguousWith(right);

        // Assert
        isContiguous.Should()
                    .Be(left.Start == right.End || right.Start == left.End);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Given_two_TimeOnlyRange_instances_IsContiguous_should_be_symetric(TimeOnlyRange left, TimeOnlyRange right)
    {
        _outputHelper.WriteLine($"{nameof(left)}: {left}");
        _outputHelper.WriteLine($"{nameof(right)}: {right}");

        return (left.IsContiguousWith(right) == right.IsContiguousWith(left)).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_TimeOnlyRange_instance_When_Start_eq_End_IsEmpty_should_be_True(TimeOnly reference)
    {
        // Arrange
        TimeOnlyRange current = new(reference, reference);

        // Act
        bool isEmpty = current.IsEmpty();

        // Assert
        isEmpty.Should()
               .BeTrue();
    }

    [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
    public void Given_TimeOnly_value_UpTo_should_build_a_TimeOnlyRange_up_to_that_value(TimeOnly reference)
    {
        // Act
        TimeOnlyRange range = TimeOnlyRange.UpTo(reference);

        // Assert
        range.Start.Should()
                   .Be(TimeOnly.MinValue);
        range.End.Should()
                 .Be(reference);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_TimeOnly_value_DownTo_should_build_a_TimeOnlyRange_up_to_that_value(TimeOnly reference)
    {
        // Act
        TimeOnlyRange range = TimeOnlyRange.DownTo(reference);

        // Assert
        range.Start.Should()
                   .Be(reference);
        range.End.Should()
                 .Be(TimeOnly.MaxValue);
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
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                true
            };

            /* 
             * first: |---------------|
             * other:                     |---------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(14.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                false
            };

            /* 
             * first: |---------------|
             * other:                 |---------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(12.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                false
            };

            /* 
             * first: --------|        |---------
             * other:      |---------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(6.Hours()), TimeOnly.FromTimeSpan(10.Hours())),
                true
            };

            /* 
             * first:         |--------|
             * other:      |---------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(14.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(08.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                true
            };

            /* 
             * first:      |---------------| 
             * other:         |--------|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(08.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(14.Hours())),
                true
            };

            /* 
             * first:     -----|      |------ 
             * other:            |--|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(08.Hours()), TimeOnly.FromTimeSpan(14.Hours())),
                false
            };

            /* 
            * first:     -----|      |------ 
            * other:                   |--|
            */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(22.Hours()), TimeOnly.FromTimeSpan(23.Hours())),
                true
            };

            /* 
            * first:     ------------------ 
            * other:                      |
            */
            yield return new object[]
            {
                TimeOnlyRange.AllDay,
                TimeOnlyRange.Empty,
                true
            };
        }
    }

    [Theory]
    [MemberData(nameof(OverlapsCases))]
    public void Given_two_instances_Overlaps_should_behave_as_expected(TimeOnlyRange left, TimeOnlyRange right, bool expected)
    {
        // Act
        bool actual = left.Overlaps(right);

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
    public FsCheck.Property Overlaps_should_be_symetric(TimeOnlyRange left, TimeOnlyRange right)
        => (left.Overlaps(right) == right.Overlaps(left)).ToProperty();

    [Property(Arbitrary = new [] { typeof(ValueGenerators) })]
    public void Given_infinity_when_testing_overlap_with_any_other_TimeOnlyRange_Overlaps_should_be_true(TimeOnlyRange other)
    {
        // Act
        bool actual = TimeOnlyRange.AllDay.Overlaps(other);

        // Assert
        actual.Should()
              .BeTrue($"infinity range overlaps every other {nameof(TimeOnlyRange)}s");
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
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(13.Hours()))
            };

            /* 
             * current   :         |---------------| 
             * other     : |---------------|
             * expected  : |-----------------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(13.Hours()))
            };

            /* 
             * current   :                 |---------------| 
             * other     : |---------------|
             * expected  : |-------------------------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(9.Hours()), TimeOnly.FromTimeSpan(11.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(9.Hours()), TimeOnly.FromTimeSpan(13.Hours()))
            };

            /* 
             * current     : |---------------|
             * other       :                 |---------------| 
             * expected    : |-------------------------------| 
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(9.Hours()), TimeOnly.FromTimeSpan(11.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(9.Hours()), TimeOnly.FromTimeSpan(13.Hours()))
            };

            /* 
             * current     : |---------------------|
             * other       :         |---------| 
             * expected    : |---------------------|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(9.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(9.Hours()), TimeOnly.FromTimeSpan(13.Hours()))
            };

            /* 
             * current     : -------|       |-----
             * other       :    |-------------| 
             * expected    : ---------------------
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(7.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(05.Hours()), TimeOnly.FromTimeSpan(22.Hours())),
                TimeOnlyRange.AllDay
            };

            /* 
             * current     : -------|       |-----
             * other       :        |-------| 
             * expected    : ---------------------
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(7.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(7.Hours()), TimeOnly.FromTimeSpan(21.Hours())),
                TimeOnlyRange.AllDay
            };

            /* 
             * current     : -------|       |-----
             * other       :    |-------| 
             * expected    : -----------|   |-----
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(05.Hours()), TimeOnly.FromTimeSpan(09.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(09.Hours()))
            };

            /* 
             * current     : -------|       |-----
             * other       :    |-------| 
             * expected    : -----------|   |-----
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(05.Hours()), TimeOnly.FromTimeSpan(09.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(09.Hours()))
            };

            /* 
             * current     : -------|       |-----
             * other       :           |-------| 
             * expected    : -------|  |----------
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(19.Hours()), TimeOnly.FromTimeSpan(23.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(19.Hours()), TimeOnly.FromTimeSpan(07.Hours()))
            };

            /* 
             * current     : -------|       |-----
             * other       :                  |-|
             * expected    : -------|       |-----
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(22.Hours()), TimeOnly.FromTimeSpan(23.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(07.Hours()))
            };
        }
    }

    [Theory]
    [MemberData(nameof(UnionCases))]
    public void Given_two_instances_Union_should_behave_as_expected(TimeOnlyRange current, TimeOnlyRange other, TimeOnlyRange expected)
    {
        // Act
        TimeOnlyRange actual = current.Union(other);
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
                TimeOnlyRange.Empty,
                TimeOnlyRange.Empty,
                TimeOnlyRange.Empty
            };

            /*
             * current   :  |-----------|
             * other     :          |------------|
             * expected  :          |---|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(4.Hours()), TimeOnly.FromTimeSpan(11.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(09.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(09.Hours()), TimeOnly.FromTimeSpan(11.Hours()))
            };

            /*
             * current   :          |------------|
             * other     :  |-----------|
             * expected  :          |---|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(09.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(4.Hours()), TimeOnly.FromTimeSpan(11.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(09.Hours()), TimeOnly.FromTimeSpan(11.Hours()))
            };

            /*
             * current   :  |-----------|
             * other     :      |-----|
             * expected  :      |-----|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(09.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours()))
            };

            /*
             * current   :  |----|
             * other     :          |------------|
             * expected  :  |
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(07.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(21.Hours()), TimeOnly.FromTimeSpan(06.Hours())),
                TimeOnlyRange.Empty
            };

            /*
             * current   :  |----|
             * other     :  ----------------------
             * expected  :  |----|
             */
            yield return new object[]
            {
                new TimeOnlyRange(TimeOnly.FromTimeSpan(07.Hours()), TimeOnly.FromTimeSpan(13.Hours())),
                TimeOnlyRange.AllDay,
                new TimeOnlyRange(TimeOnly.FromTimeSpan(07.Hours()), TimeOnly.FromTimeSpan(13.Hours()))
            };
        }
    }

    [Theory]
    [MemberData(nameof(IntersectCases))]
    public void Given_two_instances_Intersect_should_return_the_intersection(TimeOnlyRange current, TimeOnlyRange other, TimeOnlyRange expected)
    {
        // Act
        TimeOnlyRange intersection = current.Intersect(other);

        // Assert
        intersection.Should()
                    .NotBeNull().And
                    .Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public FsCheck.Property Intersect_should_be_symetric(TimeOnlyRange left, TimeOnlyRange right)
        => (left.Intersect(right) == right.Intersect(left)).ToProperty();

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Zero_should_be_the_neutral_element_of_TimeOnlyRange(TimeOnlyRange range)
    {
        // Act
        TimeOnlyRange result = range.Union(TimeOnlyRange.Empty);

        // Assert
        result.Should()
              .Be(range);
    }
}
#endif
