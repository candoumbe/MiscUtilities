// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

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
                         .Select(dateTime => new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day))
                         .ToArbitrary();

    /// <summary>
    /// Generates Arbitrary for <see cref="TimeOnly"/>
    /// </summary>
    public static Arbitrary<TimeOnly> TimeOnlys()
        => ArbMap.Default.ArbFor<DateTime>()
                         .Generator
                         .Select(dateTime => new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond))
                         .ToArbitrary();
#endif
}
