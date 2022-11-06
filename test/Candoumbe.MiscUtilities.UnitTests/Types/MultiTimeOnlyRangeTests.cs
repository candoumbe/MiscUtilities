// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if NET6_0_OR_GREATER
using Bogus;

using Candoumbe.MiscUtilities.Types;
using Candoumbe.MiscUtilities.UnitTests.Generators;

using FluentAssertions;
using FluentAssertions.Extensions;

using FsCheck;
using FsCheck.Xunit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using Xunit.Sdk;

namespace Candoumbe.MiscUtilities.UnitTests.Types;

[UnitTest]
public class MultiTimeOnlyRangeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly Faker faker = new();

    public MultiTimeOnlyRangeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public static IEnumerable<object[]> ConstructorCases
    {
        get
        {
            yield return new object[]
            {
                Array.Empty<TimeOnlyRange>(),
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Exactly(0))
            };

            /**
             * inputs :  ------------------------
             *                 |--------|
             *          
             * ranges : ------------------------
             */
            yield return new object[]
            {
                new[]
                {
                    TimeOnlyRange.AllDay,
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(8.Hours()), TimeOnly.FromTimeSpan(16.Hours()))
                },
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == TimeOnlyRange.AllDay)
                )
            };

            /**
             * inputs :       |--------|
             *          ------------------------
             *          
             * ranges : ------------------------
             */
            yield return new object[]
            {
                new[]
                {
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(8.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                    TimeOnlyRange.AllDay
                },
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == TimeOnlyRange.AllDay)
                )
            };

            /**
             * inputs :       |--------|
             *           |--------|
             *          
             * ranges :  |-------------|
             */
            yield return new object[]
            {
                new[]
                {
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(8.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                },
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(16.Hours())))
                )
            };

            /**
             * inputs :  |--------|
             *                |--------|
             *          
             * ranges :  |-------------|
             */
            yield return new object[]
            {
                new[]
                {
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(8.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                },
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(16.Hours())))
                )
            };

            /**
             * inputs :  |--|
             *                |------|
             *          
             * ranges :  |--|
             *                |------|
             */
            yield return new object[]
            {
                new[]
                {
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(14.Hours()), TimeOnly.FromTimeSpan(16.Hours())),
                },
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Exactly(2)
                                                                               && ranges.Once(range => range == new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(12.Hours())))
                                                                               && ranges.Once(range => range == new TimeOnlyRange(TimeOnly.FromTimeSpan(14.Hours()), TimeOnly.FromTimeSpan(16.Hours())))
                )
            };

            /**
            * inputs :  |--|
            *             |----|
            *                |------|
            * ranges :  |-----------|
            */
            yield return new object[]
            {
                new[]
                {
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(05.Hours()), TimeOnly.FromTimeSpan(12.Hours())),
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(09.Hours()), TimeOnly.FromTimeSpan(14.Hours())),
                    new TimeOnlyRange(TimeOnly.FromTimeSpan(12.Hours()), TimeOnly.FromTimeSpan(18.Hours())),
                },
                (Expression<Func<IEnumerable<TimeOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == new TimeOnlyRange(TimeOnly.FromTimeSpan(5.Hours()), TimeOnly.FromTimeSpan(18.Hours())))
                )
            };
        }
    }

    [Theory]
    [MemberData(nameof(ConstructorCases))]
    public void Given_non_empty_array_of_TimeOnlyRange_Constructor_should_merge_them(TimeOnlyRange[] timeOnlyRanges, Expression<Func<IEnumerable<TimeOnlyRange>, bool>> rangeExpectation)
    {
        // Act
        MultiTimeOnlyRange range = new(timeOnlyRanges);

        // Assert
        range.Ranges.Should()
                    .Match(rangeExpectation);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_date_only_ranges_that_overlaps_each_other_When_adding_them_to_the_instance_Add_should_pack_into_one_Range_only(NonNull<TimeOnlyRange> leftSource, NonNull<TimeOnlyRange> rightSource)
    {
        // Arrange
        TimeOnlyRange left = leftSource.Item;
        TimeOnlyRange right = rightSource.Item;
        MultiTimeOnlyRange range = new();
        range.Add(left);

        // Act
        range.Add(right);

        // Assert
        _ = (left.IsEmpty(), right.IsEmpty(), left.IsContiguousWith(right) || left.Overlaps(right)) switch
        {
            (true, false, _) => range.Ranges.Should()
                                            .HaveCount(1).And
                                            .Contain(right),
            (false, true, _) => range.Ranges.Should()
                                            .HaveCount(1).And
                                            .Contain(left),

            (false, false, true) => range.Ranges.Should()
                                                .HaveCount(1).And
                                                .Contain(left.Merge(right)),
            (true, true, _) => range.Ranges.Should()
                                    .BeEmpty("Both left and right ranges are empty"),
            _ => range.Ranges.Should()
                             .HaveCount(2).And
                             .ContainSingle(range => range == right).And
                             .ContainSingle(range => range == left)
        };
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_an_instance_that_one_range_eq_AllDay_When_adding_any_other_range_Should_result_in_a_noop_call(NonEmptyArray<TimeOnlyRange> ranges)
    {
        // Arrange
        MultiTimeOnlyRange sut = MultiTimeOnlyRange.Infinite;

        // Act
        ranges.Item.ForEach(range => sut.Add(range));

        // Assert
        sut.Ranges.Should()
                  .HaveCount(1).And
                  .Contain(range => range == TimeOnlyRange.AllDay);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_MultiTimeOnlyRange_When_adding_its_complement_Then_the_result_should_be_Infinite(NonNull<MultiTimeOnlyRange> input)
    {
        // Arrange
        MultiTimeOnlyRange range = input.Item;
        _outputHelper.WriteLine($"Range is {range}");
        MultiTimeOnlyRange complement = -range;
        _outputHelper.WriteLine($"Complement is {complement}");

        // Act
        MultiTimeOnlyRange result = complement + range;

        // Assert
        result.Should().Be(MultiTimeOnlyRange.Infinite);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_one_MultiTimeOnlyRange_when_calling_union_with_an_other_MultiTimeOnlyRange_Should_return_a_MultiTimeOnlyRange_instance_that_covers_all_TimeOnlyRange_from_initial_MultiTimeOnlyRange(NonNull<MultiTimeOnlyRange> leftSource, NonNull<MultiTimeOnlyRange> rightSource)
    {
        // Arrange
        MultiTimeOnlyRange left = leftSource.Item;
        MultiTimeOnlyRange right = rightSource.Item;

        _outputHelper.WriteLine($"{nameof(left)} : {left}");
        _outputHelper.WriteLine($"{nameof(right)} : {right}");

        // Act
        MultiTimeOnlyRange union = left.Merge(right);

        // Assert
        _outputHelper.WriteLine($"Merge : {union}");
        foreach (TimeOnlyRange range in left.Ranges.Concat(right.Ranges))
        {
            union.Covers(range).Should().BeTrue();
        }

        TimeOnlyRange[] ranges = union.Ranges.ToArray();

        ranges.ForEach((range, index) =>
        {
            for (int i = 0; i < ranges.Length; i++)
            {
                if (i != index)
                {
                    bool overlaps = range.Overlaps(ranges[i]);
                    bool abuts = range.IsContiguousWith(ranges[i]);

                    overlaps.Should().BeFalse($"{nameof(MultiTimeOnlyRange)} internal storage is optimized to not hold two {nameof(TimeOnlyRange)}s that overlap each other");
                    abuts.Should().BeFalse($"{nameof(MultiTimeOnlyRange)} internal storage is optimized to not hold two {nameof(TimeOnlyRange)}s that abuts each other");
                }
            }
        });
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_non_null_instances_when_calling_plus_operator_should_have_same_result_as_calling_Merge_method(NonNull<MultiTimeOnlyRange> leftSource, NonNull<MultiTimeOnlyRange> rightSource)
    {
        // Arrange
        MultiTimeOnlyRange left = leftSource.Item;
        MultiTimeOnlyRange right = rightSource.Item;

        _outputHelper.WriteLine($"{nameof(left)} : {left}");
        _outputHelper.WriteLine($"{nameof(right)} : {right}");
        MultiTimeOnlyRange expected = left.Merge(right);

        // Act
        MultiTimeOnlyRange actual = left + right;

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    public static IEnumerable<object[]> CoversCases
    {
        get
        {
            /*
             * multirange : ----------------------
             * current    : ---------------------- 
             * expected   : true
             */
            yield return new object[]
            {
                new MultiTimeOnlyRange(TimeOnlyRange.AllDay),
                TimeOnlyRange.AllDay,
                true
            };

            /*
             * multirange :       |--------|
             *              |--|
             * current    :   |-----|
             * expected   : false
             */
            yield return new object[]
            {
                new MultiTimeOnlyRange(new TimeOnlyRange(TimeOnly.FromTimeSpan(06.Hours()), TimeOnly.FromTimeSpan(09.Hours())),
                                       new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours()))),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(8.Hours()), TimeOnly.FromTimeSpan(11.Hours())),
                false
            };

            /*
             * multirange :        
             *             --|      |----     
             *                 |--| 
             * current    :     |-----|
             * expected   : false
             */
            yield return new object[]
            {
                new MultiTimeOnlyRange(new TimeOnlyRange(TimeOnly.FromTimeSpan(22.Hours()), TimeOnly.FromTimeSpan(08.Hours())),
                                       new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours()))),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(23.Hours())),
                false
            };

            /*
             * multirange :        
             *                
             *                |--|      |----     
             *                 |--| 
             * current    :     |-----|
             * expected   : false
             */
            yield return new object[]
            {
                new MultiTimeOnlyRange(new TimeOnlyRange(TimeOnly.FromTimeSpan(22.Hours()), TimeOnly.FromTimeSpan(08.Hours())),
                                       new TimeOnlyRange(TimeOnly.FromTimeSpan(10.Hours()), TimeOnly.FromTimeSpan(12.Hours()))),
                new TimeOnlyRange(TimeOnly.FromTimeSpan(11.Hours()), TimeOnly.FromTimeSpan(23.Hours())),
                false
            };
        }
    }

    [Theory]
    [MemberData(nameof(CoversCases))]
    public void Given_non_empty_MultiTimeOnlyRange_instance_when_TimeOnlyRange_is_not_empty_Covers_should_behave_as_expected(MultiTimeOnlyRange multiTimeOnlyRange, TimeOnlyRange timeOnlyRange, bool expected)
    {
        // Act
        bool actual = multiTimeOnlyRange.Covers(timeOnlyRange);

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Equals_should_be_reflexive(MultiTimeOnlyRange range)
    {
        // Act
        bool expected = range.Equals(range);

        // Assert
        expected.Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Equals_should_be_symetric(MultiTimeOnlyRange first, MultiTimeOnlyRange other)
    {
        // Act
        bool firstEqualsOther = first.Equals(other);
        bool otherEqualsFirst = other.Equals(first);

        // Assert
        firstEqualsOther.Should().Be(otherEqualsFirst);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Equals_should_be_transitive(MultiTimeOnlyRange one, MultiTimeOnlyRange second, MultiTimeOnlyRange third)
    {
        // Arrange
        bool oneEqualsSecond = one.Equals(second);
        bool secondEqualsThird = second.Equals(third);

        // Act
        bool oneEqualsThird = one.Equals(third);

        // Assert
        if (oneEqualsSecond && secondEqualsThird)
        {
            oneEqualsThird.Should().BeTrue();
        }
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Empty_multiTimeOnlyRange_should_be_the_neutral_element(MultiTimeOnlyRange initialValue)
    {
        // Act
        MultiTimeOnlyRange initialValuePlusEmpty = initialValue + MultiTimeOnlyRange.Empty;
        MultiTimeOnlyRange initialValueMinusEmpty = initialValue - MultiTimeOnlyRange.Empty;

        // Assert
        initialValuePlusEmpty.Should().Be(initialValueMinusEmpty);
    }

    public static IEnumerable<object[]> ComplementCases
    {
        get
        {
            yield return new object[]
            {
                new MultiTimeOnlyRange(new TimeOnlyRange(new TimeOnly(0, 15), new TimeOnly(3, 1)),
                                       new TimeOnlyRange(new TimeOnly(17, 37), new TimeOnly(21, 7))),
                new MultiTimeOnlyRange(TimeOnlyRange.UpTo(new TimeOnly(0, 15)),
                                       new TimeOnlyRange(new (3, 1), new(17, 37)),
                                       TimeOnlyRange.DownTo(new TimeOnly(21, 7)))
            };
        }
    }

    [Theory]
    [MemberData(nameof(ComplementCases))]
    public void Complement_should_be_computed_as_expected(MultiTimeOnlyRange initial, MultiTimeOnlyRange expected)
    {
        // Act
        MultiTimeOnlyRange actual = initial.Complement();

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_MultiTimeOnlyRange_When_calling_Complement_on_the_complement_of_initial_value_Then_result_should_be_eq_to_the_initial_value(MultiTimeOnlyRange range)
    {
        // Arrange
        MultiTimeOnlyRange complement = range.Complement();

        // Act
        MultiTimeOnlyRange actual = complement.Complement();

        // Assert
        actual.Should().Be(range);
    }

}

#endif