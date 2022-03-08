﻿// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if NET6_0_OR_GREATER
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

namespace Candoumbe.MiscUtilities.UnitTests.Types
{
    [UnitTest]
    public class MultiDateOnlyRangeTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public MultiDateOnlyRangeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                yield return new object[]
                {
                    Array.Empty<DateOnlyRange>(),
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Exactly(0))
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
                        DateOnlyRange.Infinite,
                        new DateOnlyRange(DateOnly.FromDateTime(8.April(2014)), DateOnly.FromDateTime(16.April(2014)))
                    },
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                                   && ranges.Once(range => range == DateOnlyRange.Infinite)
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
                        new DateOnlyRange(DateOnly.FromDateTime(8.April(2014)), DateOnly.FromDateTime(16.April(2014))),
                        DateOnlyRange.Infinite
                    },
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                                   && ranges.Once(range => range == DateOnlyRange.Infinite)
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
                        new DateOnlyRange(DateOnly.FromDateTime(8.April(2014)), DateOnly.FromDateTime(16.April(2014))),
                        new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(12.April(2014))),
                    },
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                                   && ranges.Once(range => range == new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(16.April(2014))))
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
                        new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(12.April(2014))),
                        new DateOnlyRange(DateOnly.FromDateTime(8.April(2014)), DateOnly.FromDateTime(16.April(2014))),
                    },
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                                   && ranges.Once(range => range == new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(16.April(2014))))
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
                        new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(12.April(2014))),
                        new DateOnlyRange(DateOnly.FromDateTime(14.April(2014)), DateOnly.FromDateTime(16.April(2014))),
                    },
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Exactly(2)
                                                                                   && ranges.Once(range => range == new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(12.April(2014))))
                                                                                   && ranges.Once(range => range == new DateOnlyRange(DateOnly.FromDateTime(14.April(2014)), DateOnly.FromDateTime(16.April(2014))))
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
                        new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)), DateOnly.FromDateTime(12.April(2014))),
                        new DateOnlyRange(DateOnly.FromDateTime(9.April(2014)), DateOnly.FromDateTime(14.April(2014))),
                        new DateOnlyRange(DateOnly.FromDateTime(12.April(2014)), DateOnly.FromDateTime(18.April(2014))),
                    },
                    (Expression<Func<IEnumerable<DateOnlyRange>, bool>>)(ranges => ranges.Once()
                                                                                   && ranges.Once(range => range == new DateOnlyRange(DateOnly.FromDateTime(5.April(2014)),
                                                                                                                                      DateOnly.FromDateTime(18.April(2014))))
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void Given_non_empty_array_of_DateOnlyRange_Constructor_should_merge_them(DateOnlyRange[] DateOnlyRanges, Expression<Func<IEnumerable<DateOnlyRange>, bool>> rangeExpectation)
        {
            // Act
            MultiDateOnlyRange range = new(DateOnlyRanges);

            // Assert
            range.Ranges.Should()
                        .Match(rangeExpectation);
        }

        [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
        public void Given_two_date_only_ranges_that_overlaps_each_other_When_adding_them_to_the_instance_Add_should_pack_into_one_Range_only(NonNull<DateOnlyRange> leftSource, NonNull<DateOnlyRange> rightSource)
        {
            // Arrange
            DateOnlyRange left = leftSource.Item;
            DateOnlyRange right = rightSource.Item;
            MultiDateOnlyRange range = new();
            range.Add(left);

            // Act
            range.Add(right);

            // Assert
            _ = (left.IsEmpty(), right.IsEmpty(), left.IsContiguousWith(right) || left.Overlaps(right)) switch
            {
                (true, false, true) => range.Ranges.Should()
                                                .HaveCount(1).And
                                                .Contain(right),
                (false, true, true) => range.Ranges.Should()
                                                .HaveCount(1).And
                                                .Contain(left),
                (false, true, false) => range.Ranges.Should()
                                                    .HaveCount(2).And
                                                    .Contain(left).And
                                                    .Contain(right),
                (false, false, true) => range.Ranges.Should()
                                                    .HaveCount(1).And
                                                    .Contain(left.Union(right)),
                (true, true, _) => (left == right) switch
                {
                    true => range.Ranges.Should()
                                        .HaveCount(1).And
                                        .Contain(left),
                    _ => range.Ranges.Should().HaveCount(2).And
                                              .Contain(right).And
                                              .Contain(left)
                },
                _ => range.Ranges.Should()
                                 .HaveCount(2).And
                                 .ContainSingle(range => range == right).And
                                 .ContainSingle(range => range == left)
            };
        }

        [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
        public void Given_an_instance_that_one_range_eq_Infinite_When_adding_any_other_range_Should_result_in_a_noop_call(NonEmptyArray<DateOnlyRange> ranges)
        {
            // Arrange
            MultiDateOnlyRange sut = new();

            sut.Add(DateOnlyRange.Infinite);

            // Act
            ranges.Item.ForEach(range => sut.Add(range));

            // Assert
            sut.Ranges.Should()
                      .HaveCount(1).And
                      .Contain(range => range == DateOnlyRange.Infinite);
        }

        [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
        public void Given_one_MultiDateOnlyRange_when_calling_union_with_an_other_MultiDateOnlyRange_Should_return_a_MultiDateOnlyRange_instance_that_covers_all_DateOnlyRange_from_initial_MultiDateOnlyRange(NonNull<MultiDateOnlyRange> leftSource, NonNull<MultiDateOnlyRange> rightSource)
        {
            // Arrange
            MultiDateOnlyRange left = leftSource.Item;
            MultiDateOnlyRange right = rightSource.Item;

            _outputHelper.WriteLine($"{nameof(left)} : {left}");
            _outputHelper.WriteLine($"{nameof(right)} : {right}");

            // Act
            MultiDateOnlyRange union = left.Union(right);

            // Assert
            _outputHelper.WriteLine($"Union : {union}");
            foreach (DateOnlyRange range in left.Ranges.Concat(right.Ranges))
            {
                union.Covers(range).Should().BeTrue();
            }

            DateOnlyRange[] ranges = union.Ranges.ToArray();

            ranges.ForEach((range, index) =>
            {
                for (int i = 0; i < ranges.Length; i++)
                {
                    if (i != index)
                    {
                        bool overlaps = range.Overlaps(ranges[i]);
                        bool abuts = range.IsContiguousWith(ranges[i]);

                        overlaps.Should().BeFalse($"{nameof(MultiDateOnlyRange)} internal storage is optimized to not hold two {nameof(DateOnlyRange)}s that overlap each other");
                        abuts.Should().BeFalse($"{nameof(MultiDateOnlyRange)} internal storage is optimized to not hold two {nameof(DateOnlyRange)}s that abuts each other");
                    }
                }
            });
        }

        [Property(Arbitrary = new [] { typeof(ValueGenerators) })]
        public void Given_two_non_null_instances_when_calling_plus_operator_should_have_same_result_as_calling_Union_method(NonNull<MultiDateOnlyRange> leftSource, NonNull<MultiDateOnlyRange> rightSource)
        {
            // Arrange
            MultiDateOnlyRange left = leftSource.Item;
            MultiDateOnlyRange right = rightSource.Item;

            _outputHelper.WriteLine($"{nameof(left)} : {left}");
            _outputHelper.WriteLine($"{nameof(right)} : {right}");
            MultiDateOnlyRange expected = left.Union(right);

            // Act
            MultiDateOnlyRange actual = left + right;

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
                    new MultiDateOnlyRange(DateOnlyRange.Infinite),
                    DateOnlyRange.Infinite,
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
                    new MultiDateOnlyRange(new DateOnlyRange(DateOnly.FromDateTime(6.April(2014)), DateOnly.FromDateTime(9.April(2014))),
                                           new DateOnlyRange(DateOnly.FromDateTime(10.April(2014)), DateOnly.FromDateTime(12.April(2014)))),
                    new DateOnlyRange(DateOnly.FromDateTime(8.April(2014)), DateOnly.FromDateTime(11.April(2014))),
                    false
                };
            }
        }

        [Theory]
        [MemberData(nameof(CoversCases))]
        public void Given_non_empty_MultiDateOnlyRange_instance_when_DateOnlyRange_is_not_empty_Covers_should_behave_as_expected(MultiDateOnlyRange multiDateOnlyRange, DateOnlyRange DateOnlyRange, bool expected)
        {
            // Act
            bool actual = multiDateOnlyRange.Covers(DateOnlyRange);

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = new[] {typeof(ValueGenerators)})]
        public void Given_non_null_MultiDateOnlyRange_instance_When_adding_to_its_complement_Union_should_return_Infinite(MultiDateOnlyRange original)
        {
            // Arrange
            MultiDateOnlyRange complement = original.Complement();
            _outputHelper.WriteLine($"Complement of {original} is {complement}");

            // Act
            MultiDateOnlyRange actual = original + complement;
            _outputHelper.WriteLine($"Union of {original} and {complement} is {actual}");

            // Assert
            actual.Should().BeEquivalentTo(MultiDateOnlyRange.Infinite);
        }

        public static IEnumerable<object[]> ComplementCases
        {
            get
            {
                /*
                 * current : |
                 * expected:  ----------------------
                 */
                yield return new object[]
                {
                    MultiDateOnlyRange.Infinite,
                    MultiDateOnlyRange.Empty
                };

                /*
                 * current :  ----------------------
                 * expected: |
                 */
                yield return new object[]
                {
                    MultiDateOnlyRange.Empty,
                    MultiDateOnlyRange.Infinite
                };

                /*
                 * current :            |----|
                 * 
                 * expected:  ----------|    |------
                 */
                yield return new object[]
                {
                    new MultiDateOnlyRange(new DateOnlyRange (DateOnly.FromDateTime(7.April(2015)), DateOnly.FromDateTime(9.April(2015)))),
                    new MultiDateOnlyRange(DateOnlyRange.UpTo(DateOnly.FromDateTime(7.April(2015))),
                                           DateOnlyRange.DownTo(DateOnly.FromDateTime(9.April(2015))))
                };

                /*
                 * current :  ----------|           
                 * expected:            |-----------
                 */
                yield return new object[]
                {
                    new MultiDateOnlyRange(DateOnlyRange.UpTo(DateOnly.FromDateTime(7.April(2015)))),
                    new MultiDateOnlyRange(DateOnlyRange.DownTo(DateOnly.FromDateTime(7.April(2015))))
                };

                /*
                 * current :  ----------|           
                 * expected:            |-----------
                 */
                yield return new object[]
                {
                    new MultiDateOnlyRange(DateOnlyRange.UpTo(DateOnly.FromDateTime(7.April(2015)))),
                    new MultiDateOnlyRange(DateOnlyRange.DownTo(DateOnly.FromDateTime(7.April(2015))))
                };

                /*
                 * current :        |-|
                 *            |-|
                 *                     |-|
                 * expected: -|  |-|     |-----------
                 */
                yield return new object[]
                {
                    new MultiDateOnlyRange(new DateOnlyRange(DateOnly.FromDateTime(7.February(2010)), DateOnly.FromDateTime(7.March(2010))),
                                           new DateOnlyRange(DateOnly.FromDateTime(5.January(2010)), DateOnly.FromDateTime(5.February(2010))),
                                           new DateOnlyRange(DateOnly.FromDateTime(15.May(2010)), DateOnly.FromDateTime(17.May(2010)))),
                    new MultiDateOnlyRange(DateOnlyRange.UpTo(DateOnly.FromDateTime(5.January(2010))),
                                           new DateOnlyRange(DateOnly.FromDateTime(5.February(2010)), DateOnly.FromDateTime(7.February(2010))),
                                           new DateOnlyRange(DateOnly.FromDateTime(7.March(2010)), DateOnly.FromDateTime(15.May(2010))),
                                           DateOnlyRange.DownTo(DateOnly.FromDateTime(17.May(2010))))
                };
            }
        }

        [Theory]
        [MemberData(nameof(ComplementCases))]
        public void Given_MultiDateOnlyRange_Complement_should_behave_as_expected(MultiDateOnlyRange source, MultiDateOnlyRange expected)
        {
            // Act
            MultiDateOnlyRange actual = source.Complement();

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> UnionCases
        {
            get
            {
                yield return new object[]
                {
                    MultiDateOnlyRange.Infinite,
                    MultiDateOnlyRange.Empty,
                    MultiDateOnlyRange.Infinite
                };

                yield return new object[]
                {
                    new MultiDateOnlyRange(new DateOnlyRange(DateOnly.FromDateTime(27.March(1900)), DateOnly.FromDateTime(6.January(2071))),
                                           new DateOnlyRange(DateOnly.FromDateTime(17.March(2079)), DateOnly.FromDateTime(2.February(2084)))),
                    new MultiDateOnlyRange(DateOnlyRange.UpTo(DateOnly.FromDateTime(27.March(1900))),
                                           new DateOnlyRange(DateOnly.FromDateTime(6.January(2071)), DateOnly.FromDateTime(17.March(2079))),
                                           DateOnlyRange.DownTo(DateOnly.FromDateTime(2.February(2084)))),
                    MultiDateOnlyRange.Infinite
                };
            }
        }

        [Theory]
        [MemberData(nameof(UnionCases))]
        public void Given_two_MultiDateOnlyRange_instances_Union_should_behave_as_expected(MultiDateOnlyRange left, MultiDateOnlyRange right, MultiDateOnlyRange expected)
        {
            // Act
            MultiDateOnlyRange actual = left + right;

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

#endif