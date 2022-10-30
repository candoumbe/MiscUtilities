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
    /// A <see cref="DateTimeRange"/> that cannot contains other <see cref="DateTimeRange"/> range.
    /// </summary>
    public static DateTimeRange Empty = new(DateTime.MinValue, DateTime.MinValue);

    /// <summary>
    /// A <see cref="DateTimeRange"/> that overlaps any other <see cref="DateTimeRange"/> (except <see cref="Empty"/>)
    /// </summary>
    public static DateTimeRange Infinite = new(DateTime.MinValue, DateTime.MaxValue);

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
    /// <returns><see langword="true"/> when current instance and <paramref name="other"/> overlaps each other <see langword="false"/> otherwise.</returns>
    public bool Overlaps(DateTimeRange other)
        => (Start <= other.Start && other.End <= End)
           || (Start <= other.Start && other.Start < End && End <= other.End)
           || (other.Start <= Start && End <= other.End)
           || (other.Start <= Start && Start <= other.End && other.End <= End)
        ;

    /// <summary>
    /// Checks if <paramref name="other"/> and current instances are contiguoous.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><see langword="true"/> when current instance and <paramref name="other"/> are contiguous and <see langword="false"/> otherwise.</returns>
    public bool IsContiguousWith(DateTimeRange other)
        => End == other.Start || Start == other.End;

    ///<inheritdoc/>
    public override string ToString() => $"{Start} - {End}";

    /// <summary>
    /// Builds a new <see cref="DateTimeRange"/> that spans from <see cref="DateTime.MaxValue"/> up to <paramref name="date"/>
    /// </summary>
    /// <param name="date">The desired upper limit</param>
    /// <returns>a <see cref="DateTimeRange"/> that spans up to <paramref name="date"/>.</returns>
    public static DateTimeRange UpTo(DateTime date) => new(DateTime.MinValue, date);

    /// <summary>
    /// Builds a new <see cref="DateTimeRange"/> that spans from <paramref name="date"/> to <see cref="DateTime.MaxValue"/>.
    /// </summary>
    /// <param name="date">The desired upper limit</param>
    /// <returns>a <see cref="DateTimeRange"/> that spans from <paramref name="date"/>.</returns>
    public static DateTimeRange DownTo(DateTime date) => new(date, DateTime.MaxValue);

    /// <summary>
    /// Expands the current so that the resulting <see cref="DateTimeRange"/> spans over both current and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other <see cref="DateTimeRange"/> to span over</param>
    /// <returns>A new <see cref="DateTimeRange"/> than spans over both current and <paramref name="other"/> range</returns>
    /// <exception cref="InvalidOperationException">if either : current instance does not overlap or is not continuous with <paramref name="other"/>.</exception>
    public DateTimeRange Union(DateTimeRange other)
    {
        DateTimeRange result = Empty;
        if (this == Infinite || other == Infinite)
        {
            result = Infinite;
        }
        else if (IsEmpty())
        {
            result = other;
        }
        else if (other.IsEmpty())
        {
            result = this;
        }
        else if (Overlaps(other) || IsContiguousWith(other))
        {
#if NET5_0_OR_GREATER
            result = this with { Start = GetMinimum(Start, other.Start), End = GetMaximum(other.End, End) };
#else
            result = new(GetMinimum(Start, other.Start), GetMaximum(other.End, End) );
#endif
        }
        else
        {
            throw new InvalidOperationException($"Cannot build a {nameof(DateTimeRange)} as union of '{this}' and {other}");
        }

        return result;
    }

    /// <summary>
    /// Returns the oldest <see cref="DateTime"/> between <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static DateTime GetMinimum(DateTime left, DateTime right) => left.CompareTo(right) switch
    {
        < 0 => left,
        _ => right
    };

    /// <summary>
    /// Returns the furthest <see cref="DateTime"/> between <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static DateTime GetMaximum(DateTime left, DateTime right) => left.CompareTo(right) switch
    {
        > 0 => left,
        _ => right
    };

    /// <summary>
    /// Computes  <see cref="DateTimeRange"/> value that is common between the current instance and <paramref name="other"/>.
    /// </summary>
    /// <remarks>
    /// This methods relies on <see cref="Overlaps(DateTimeRange)"/> to see if there can be a intersection with <paramref name="other"/>.
    /// </remarks>
    /// <param name="other">The other instance to test</param>
    /// <returns>a <see cref="DateTimeRange"/> that represent the overlap between the current instance and <paramref name="other"/> or <see cref="Empty"/> when no intersection found.</returns>
    public DateTimeRange Intersect(DateTimeRange other)
    {
        DateTimeRange result = Empty;

        if (this == Infinite)
        {
            result = other;
        }
        else if (other == Infinite)
        {
            result = this;
        }
        else if (Overlaps(other))
        {
#if NET5_0_OR_GREATER
            result = this with { Start = GetMaximum(Start, other.Start), End = GetMinimum(End, other.End) };
#else
            result = new (GetMaximum(Start, other.Start), GetMinimum(End, other.End));
#endif
        }

        return result;
    }
}
