﻿// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using FsCheck;
using FsCheck.Fluent;

namespace Candoumbe.MiscUtilities.UnitTests.Generators;

/// <summary>
/// Utility class for generating custom <see cref="Arbitrary{T}"/>
/// </summary>
internal static class ValueGenerators
{
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
                         .Zip(ArbMap.Default.ArbFor<NonNegativeInt>().Generator)
                         .Select(tuple => (dateTime: tuple.Item1, milliseconds: tuple.Item2.Get))
                         .Select((dateTimeAndMillisecobds) => TimeOnly.FromDateTime(dateTimeAndMillisecobds.dateTime).Add(TimeSpan.FromMilliseconds(dateTimeAndMillisecobds.milliseconds)))
                         .ToArbitrary();

    public static Arbitrary<Array> Arrays() => ArbMap.Default.ArbFor<int>().Generator.ArrayOf<int>()
                                                  .Select(numbers =>
                                                  {
                                                      Array array = Array.CreateInstance(typeof(int), numbers.Length);

                                                      numbers.CopyTo(array, 0);

                                                      return array;
                                                  }).ToArbitrary();
}
