// "Copyright (c) Cyrille NDOUMBE.
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
    public class MultiTimeOnlyRangeTests
    {
        private readonly ITestOutputHelper _outputHelper;

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
        public void Given_an_instance_that_one_range_eq_AllDay_When_adding_any_other_range_Should_result_in_a_noop_call(NonEmptyArray<TimeOnlyRange> ranges)
        {
            // Arrange
            MultiTimeOnlyRange sut = new();

            sut.Add(TimeOnlyRange.AllDay);

            // Act
            ranges.Item.ForEach(range => sut.Add(range));

            // Assert
            sut.Ranges.Should()
                      .HaveCount(1).And
                      .Contain(range => range == TimeOnlyRange.AllDay);
        }

        [Property(Arbitrary = new[] { typeof(ValueGenerators) })]
        public void Given_an_instance_of_MultiTimeOnlyRange_calling_Minus_operator_Should_return_the_complement_of_the_instance(NonNull<MultiTimeOnlyRange> input)
        {
            // Arrange
            MultiTimeOnlyRange range = input.Item;

            // Act
            MultiTimeOnlyRange complement = -range;

            // Assert
            complement.Should().NotBeNull();
            complement.Ranges.Should().NotBeEmpty();
            foreach (TimeOnlyRange item in range.Ranges)
            {
                complement.Ranges.Should().NotContain(timeRange => timeRange == item && timeRange.Overlaps(item));
            }
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
            MultiTimeOnlyRange union = left.Union(right);

            // Assert
            _outputHelper.WriteLine($"Union : {union}");
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

        [Property(Arbitrary = new [] { typeof(ValueGenerators) })]
        public void Given_two_non_null_instances_when_calling_plus_operator_should_have_same_result_as_calling_Union_method(NonNull<MultiTimeOnlyRange> leftSource, NonNull<MultiTimeOnlyRange> rightSource)
        {
            // Arrange
            MultiTimeOnlyRange left = leftSource.Item;
            MultiTimeOnlyRange right = rightSource.Item;

            _outputHelper.WriteLine($"{nameof(left)} : {left}");
            _outputHelper.WriteLine($"{nameof(right)} : {right}");
            MultiTimeOnlyRange expected = left.Union(right);

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
    }
}

#endif