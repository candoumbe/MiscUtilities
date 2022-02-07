// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// A <see cref="DateTime"/> range
/// </summary>
#if !NET5_0_OR_GREATER
public class DateTimeRange : Range<DateTime>
#else
public record DateTimeRange : Range<DateTime>
#endif
{
    /// <summary>
    /// Builds a new <see cref="DateTimeRange"/>
    /// </summary>
    /// <param name="start">lower bound</param>
    /// <param name="end">Upper bound</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is after <paramref name="end"/>.</exception>
    public DateTimeRange(DateTime start, DateTime end) : base(start, end)
    {
        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} cannot be after {nameof(end)}");
        }
    }

    /// <summary>
    /// Tests wheters the current instance overlaps with <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><c>true</c> when current instance and <paramref name="other"/> overlaps each other <c>false</c> otherwise.</returns>
    public bool Overlaps(DateTimeRange other)
        => (Start <= other.Start && other.End <= End)
           || (other.Start <= Start && End <= other.End);

    /// <summary>
    /// Checks if <paramref name="other"/> and current instances are contiguoous.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><c>true</c> when current instance and <paramref name="other"/> are contiguous and <c>false</c> otherwise.</returns>
    public bool IsContiguousWith(DateTimeRange other)
        => End == other.Start || Start == other.End;

    ///<inheritdoc/>
    public override string ToString() => $"{Start} - {End}";
}
