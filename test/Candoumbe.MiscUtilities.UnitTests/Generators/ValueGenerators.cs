// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using Candoumbe.MiscUtilities.Types;

using FsCheck;
using FsCheck.Fluent;

using System;

namespace Candoumbe.MiscUtilities.UnitTests.Generators;

/// <summary>
/// Utility class for generating custom <see cref="Arbitrary{T}"/>
/// </summary>
internal static class ValueGenerators
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// Generates Arbitrary for <see cref="DateOnly"/>
    /// </summary>
    public static Arbitrary<DateOnly> DateOnlys()
        => ArbMap.Default.ArbFor<DateTime>()
                         .Generator
                         .Select(dateTime => DateOnly.FromDateTime(dateTime))
                         .ToArbitrary();

    /// <summary>
    /// Generates Arbitrary for <see cref="TimeOnly"/>
    /// </summary>
    public static Arbitrary<TimeOnly> TimeOnlys()
        => ArbMap.Default.ArbFor<DateTime>()
                         .Generator
                         .Select(dateTime => TimeOnly.FromDateTime(dateTime))
                         .ToArbitrary();

    public static Arbitrary<DateOnlyRange> DateOnlyRanges()
        => DateOnlys().Generator.Two()
                         .Select(dates => (start: dates.Item1, end: dates.Item2))
                         .Where(dates => dates.start != dates.end)
                         .Select(dates => (dates.start < dates.end) switch
                         {
                             true => new DateOnlyRange(dates.start, dates.end),
                             _ => new DateOnlyRange(dates.end, dates.start)
                         })
        .ToArbitrary();

    public static Arbitrary<TimeOnlyRange> TimeOnlyRanges()
        => Gen.OneOf(TimeOnlys().Generator.Two()
                         .Select(times => (start: times.Item1, end: times.Item2))
                             .Where(times => times.start != times.end)
                             .Select(times => (times.start < times.end) switch
                             {
                                 true => new TimeOnlyRange(times.start, times.end),
                                 _ => new TimeOnlyRange(times.end, times.start)
                             }),
                      Gen.Constant(TimeOnlyRange.Empty),
                      Gen.Constant(TimeOnlyRange.AllDay))
            .ToArbitrary();

    public static Arbitrary<MultiTimeOnlyRange> MultiTimeOnlyRanges()
        => Gen.Sized(MultiTimeOnlyRangesGenerator).ToArbitrary();

    private static Gen<MultiTimeOnlyRange> MultiTimeOnlyRangesGenerator(int size)
    {
        Gen<MultiTimeOnlyRange> gen;

        switch (size)
        {
            case 0:
                {
                    gen = TimeOnlyRanges().Generator.ArrayOf(2)
                                     .Select(ranges => new MultiTimeOnlyRange(ranges));
                    break;
                }

            default:
                {
                    Gen<MultiTimeOnlyRange> subtree = MultiTimeOnlyRangesGenerator(size / 2);

                    gen = Gen.OneOf(TimeOnlyRanges().Generator.ArrayOf(size)
                                     .Select(ranges => new MultiTimeOnlyRange(ranges)),
                                    subtree);
                    break;
                }
        }

        return gen;
    }

    public static Arbitrary<MultiDateOnlyRange> MultiDateOnlyRanges()
        => Gen.Sized(MultiDateOnlyRangesGenerator).ToArbitrary();

    private static Gen<MultiDateOnlyRange> MultiDateOnlyRangesGenerator(int size)
    {
        Gen<MultiDateOnlyRange> gen;

        switch (size)
        {
            case 0:
                {
                    gen = DateOnlyRanges().Generator.ArrayOf(2)
                                     .Select(ranges => new MultiDateOnlyRange(ranges));
                    break;
                }

            default:
                {
                    Gen<MultiDateOnlyRange> subtree = MultiDateOnlyRangesGenerator(size / 2);

                    gen = Gen.OneOf(DateOnlyRanges().Generator.ArrayOf(size)
                                     .Select(ranges => new MultiDateOnlyRange(ranges)),
                                    subtree);
                    break;
                }
        }

        return gen;
    }
#endif

    public static Arbitrary<DateTimeRange> DateTimeRanges()
        => Gen.OneOf(ArbMap.Default.ArbFor<DateTime>()
                         .Generator
                         .Two()
                         .Where(dates => dates.Item1 != dates.Item2)
                         .Select(dates => (dates.Item1 < dates.Item2) switch
                         {
                             true => new DateTimeRange(dates.Item1, dates.Item2),
                             _ => new DateTimeRange(dates.Item2, dates.Item1)
                         }),
                    Gen.Constant(DateTimeRange.Infinite),
                    Gen.Constant(DateTimeRange.Empty))
        .ToArbitrary();

    public static Arbitrary<Array> Arrays() => ArbMap.Default.ArbFor<int>().Generator.ArrayOf()
                                                  .Select(numbers =>
                                                  {
                                                      Array array = Array.CreateInstance(typeof(int), numbers.Length);

                                                      numbers.CopyTo(array, 0);

                                                      return array;
                                                  }).ToArbitrary();
}
