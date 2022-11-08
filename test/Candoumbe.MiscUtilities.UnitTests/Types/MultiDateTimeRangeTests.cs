// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

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

namespace Candoumbe.MiscUtilities.UnitTests.Types;

[UnitTest]
public class MultiDateTimeRangeTests
{
    private readonly ITestOutputHelper _outputHelper;

    public MultiDateTimeRangeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public static IEnumerable<object[]> ConstructorCases
    {
        get
        {
            yield return new object[]
            {
                Array.Empty<DateTimeRange>(),
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Exactly(0))
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
                    DateTimeRange.Infinite,
                    new DateTimeRange(8.April(2014), 16.April(2014))
                },
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range.IsInfinite())
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
                    new DateTimeRange(8.April(2014), 16.April(2014)),
                    DateTimeRange.Infinite
                },
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == DateTimeRange.Infinite)
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
                    new DateTimeRange(8.April(2014), 16.April(2014)),
                    new DateTimeRange(5.April(2014), 12.April(2014)),
                },
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == new DateTimeRange(5.April(2014), 16.April(2014)))
                )
            };

            /**
             * inputs :  |--------|
             *                |--------|
             * ranges :  |-------------|
             */
            yield return new object[]
            {
                new[]
                {
                    new DateTimeRange(5.April(2014), 12.April(2014)),
                    new DateTimeRange(8.April(2014), 16.April(2014)),
                },
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == new DateTimeRange(5.April(2014), 16.April(2014)))
                )
            };

            /**
             * inputs :  |--|
             *                |------|
             * ranges :  |--|
             *                |------|
             */
            yield return new object[]
            {
                new[]
                {
                    new DateTimeRange(5.April(2014), 12.April(2014)),
                    new DateTimeRange(14.April(2014), 16.April(2014)),
                },
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Exactly(2)
                                                                               && ranges.Once(range => range == new DateTimeRange(5.April(2014), 12.April(2014)))
                                                                               && ranges.Once(range => range == new DateTimeRange(14.April(2014), 16.April(2014)))
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
                    new DateTimeRange(5.April(2014), 12.April(2014)),
                    new DateTimeRange(9.April(2014), 14.April(2014)),
                    new DateTimeRange(12.April(2014),18.April(2014)),
                },
                (Expression<Func<IEnumerable<DateTimeRange>, bool>>)(ranges => ranges.Once()
                                                                               && ranges.Once(range => range == new DateTimeRange(5.April(2014),
                                                                                                                                  18.April(2014)))
                )
            };
        }
    }

    [Theory]
    [MemberData(nameof(ConstructorCases))]
    public void Given_non_empty_array_of_DateTimeRange_Constructor_should_merge_them(DateTimeRange[] DateTimeRanges, Expression<Func<IEnumerable<DateTimeRange>, bool>> rangeExpectation)
    {
        // Act
        MultiDateTimeRange range = new(DateTimeRanges);

        // Assert
        range.Ranges.Should()
                    .Match(rangeExpectation);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_date_only_ranges_that_overlaps_each_other_When_adding_them_using_Add_Then_should_pack_into_one_Range_only(NonNull<DateTimeRange> leftSource, NonNull<DateTimeRange> rightSource)
    {
        // Arrange
        DateTimeRange left = leftSource.Item;
        DateTimeRange right = rightSource.Item;
        MultiDateTimeRange range = new();
        range.Add(left);

        // Act
        range.Add(right);

        // Assert
        _ = (left.IsContiguousWith(right) || left.Overlaps(right)) switch
        {
            true => range.Ranges.Should()
                                .HaveCount(1).And
                                .ContainSingle(range => range == left.Merge(right)),
            _ => range.Ranges.Should()
                .HaveCount(2).And
                .Contain(left).And
                .Contain(right)
        };
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) }, Replay = "(16035462008899407285,4231346714582972547)")]
    public void Given_an_instance_that_one_range_eq_Infinite_When_adding_any_other_range_Should_result_in_a_noop_call(NonEmptyArray<DateTimeRange> ranges)
    {
        // Arrange
        MultiDateTimeRange sut = new(DateTimeRange.Infinite);

        // Act
        ranges.Item.ForEach(range => sut.Add(range));

        // Assert
        sut.IsEmpty().Should().BeFalse();
        sut.IsInfinite().Should().BeTrue($"The initial {nameof(MultiDateTimeRange)} already contains infinite");
        sut.Ranges.Should()
                  .HaveCount(1, "The only range is infinite").And
                  .Contain(range => range == DateTimeRange.Infinite);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_one_MultiDateTimeRange_when_calling_union_with_an_other_MultiDateTimeRange_Then_the_result_should_covers_all_DateTimeRange_from_initial_MultiDateTimeRange(NonNull<MultiDateTimeRange> leftSource, NonNull<MultiDateTimeRange> rightSource)
    {
        // Arrange
        MultiDateTimeRange left = leftSource.Item;
        MultiDateTimeRange right = rightSource.Item;

        _outputHelper.WriteLine($"{nameof(left)} : {left}");
        _outputHelper.WriteLine($"{nameof(right)} : {right}");

        // Act
        MultiDateTimeRange union = left.Union(right);

        // Assert
        _outputHelper.WriteLine($"Union : {union}");
        foreach (DateTimeRange range in left.Ranges.Concat(right.Ranges))
        {
            union.Overlaps(range).Should().BeTrue();
        }

        DateTimeRange[] ranges = union.Ranges.ToArray();

        ranges.ForEach((range, index) =>
        {
            for (int i = 0; i < ranges.Length; i++)
            {
                if (i != index)
                {
                    bool overlaps = range.Overlaps(ranges[i]);
                    bool abuts = range.IsContiguousWith(ranges[i]);

                    overlaps.Should().BeFalse($"{nameof(MultiDateTimeRange)} internal storage is optimized to not hold two {nameof(DateTimeRange)}s that overlap each other");
                    abuts.Should().BeFalse($"{nameof(MultiDateTimeRange)} internal storage is optimized to not hold two {nameof(DateTimeRange)}s that abuts each other");
                }
            }
        });
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_two_non_null_instances_when_calling_plus_operator_should_have_same_result_as_calling_Union_method(NonNull<MultiDateTimeRange> leftSource, NonNull<MultiDateTimeRange> rightSource)
    {
        // Arrange
        MultiDateTimeRange left = leftSource.Item;
        MultiDateTimeRange right = rightSource.Item;

        _outputHelper.WriteLine($"{nameof(left)} : {left}");
        _outputHelper.WriteLine($"{nameof(right)} : {right}");
        MultiDateTimeRange expected = left.Union(right);

        // Act
        MultiDateTimeRange actual = left + right;

        // Assert
        actual.Should().Be(expected);
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
                new MultiDateTimeRange(DateTimeRange.Infinite),
                DateTimeRange.Infinite,
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
                new MultiDateTimeRange(new DateTimeRange(6.April(2014), 9.April(2014)),
                                       new DateTimeRange(10.April(2014), 12.April(2014))),
                new DateTimeRange(8.April(2014), 11.April(2014)),
                false
            };
        }
    }

    [Theory]
    [MemberData(nameof(CoversCases))]
    public void Given_non_empty_MultiDateTimeRange_instance_when_DateTimeRange_is_not_empty_Covers_should_behave_as_expected(MultiDateTimeRange multiDateTimeRange, DateTimeRange dateOnlyRange, bool expected)
    {
        // Act
        bool actual = multiDateTimeRange.Overlaps(dateOnlyRange);

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_non_null_MultiDateTimeRange_instance_When_adding_to_its_complement_Union_should_return_Infinite(MultiDateTimeRange original)
    {
        // Arrange
        MultiDateTimeRange complement = original.Complement();
        _outputHelper.WriteLine($"Complement of {original} is {complement}");

        // Act
        MultiDateTimeRange actual = original + complement;
        _outputHelper.WriteLine($"Union of {original} and {complement} is {actual}");

        // Assert
        actual.Should().Be(MultiDateTimeRange.Infinite);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) }, Replay = "(13215877118328040669,378035299666480085)")]
    public void Given_MultiDateTimeRange_When_calling_Complement_on_the_complement_of_initial_value_Then_result_should_be_eq_to_the_initial_value(MultiDateTimeRange range)
    {
        // Arrange
        MultiDateTimeRange complement = range.Complement();
        _outputHelper.WriteLine($"Complement is {complement}");

        // Act
        MultiDateTimeRange actual = complement.Complement();

        // Assert
        actual.Should().Be(range);
    }

    public static IEnumerable<object[]> UnionCases
    {
        get
        {
            yield return new object[]
            {
                MultiDateTimeRange.Infinite,
                MultiDateTimeRange.Empty,
                MultiDateTimeRange.Infinite
            };

            yield return new object[]
            {
                new MultiDateTimeRange(new DateTimeRange(27.March(1900), 6.January(2071)),
                                       new DateTimeRange(17.March(2079), 2.February(2084))),
                new MultiDateTimeRange(DateTimeRange.UpTo(27.March(1900)),
                                       new DateTimeRange(6.January(2071), 17.March(2079)),
                                       DateTimeRange.DownTo(2.February(2084))),
                MultiDateTimeRange.Infinite
            };
        }
    }

    [Theory]
    [MemberData(nameof(UnionCases))]
    public void Given_two_MultiDateTimeRange_instances_Union_should_behave_as_expected(MultiDateTimeRange left, MultiDateTimeRange right, MultiDateTimeRange expected)
    {
        // Act
        MultiDateTimeRange actual = left + right;

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
    public void Given_any_MultiDateTimeRange_instance_When_adding_its_complement_Then_result_should_be_Infinite(MultiDateTimeRange value)
    {
        // Arrange
        MultiDateTimeRange complement = value.Complement();
        _outputHelper.WriteLine($"Complement is {complement}");

        // Act
        MultiDateTimeRange result = value + complement;

        // Assert
        result.Should().Be(MultiDateTimeRange.Infinite);
    }

    [Fact]
    public void Given_non_null_instance_When_adding_a_DateTimeRange_that_is_infinite_Then_IsInfinite_should_return_true()
    {
        // Arrange
        MultiDateTimeRange range = new();

        // Act
        range.Add(DateTimeRange.Infinite);

        // Assert
        range.IsInfinite().Should().BeTrue();
    }

    [Fact]
    public void Given_non_null_instance_Then_IsEmpty_should_return_true()
    {
        // Arrange
        MultiDateTimeRange range = new();

        // Assert
        range.IsEmpty().Should().BeTrue();
    }
}