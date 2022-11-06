// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"
#if NET6_0_OR_GREATER

using System;
using System.Diagnostics;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// A <see cref="DateOnly"/> range
/// </summary>
public record DateOnlyRange : Range<DateOnly>
{
    /// <summary>
    /// A <see cref="DateOnlyRange"/> that <see cref="Overlaps(DateOnlyRange)">overlaps</see> any other <see cref="DateOnlyRange"/> except (<see cref="Empty"/>).
    /// </summary>
    public static DateOnlyRange Infinite => new(DateOnly.MinValue, DateOnly.MaxValue);

    /// <summary>
    /// A <see cref="DateOnlyRange"/> that <see cref="Overlaps(DateOnlyRange)">overlaps</see> no other <see cref="DateOnlyRange"/>.
    /// </summary>
    public static DateOnlyRange Empty => new(DateOnly.MinValue, DateOnly.MinValue);

    /// <summary>
    /// Builds a new <see cref="DateTimeRange"/>
    /// </summary>
    /// <param name="start">lower bound</param>
    /// <param name="end">Upper bound</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is after <paramref name="end"/>.</exception>
    public DateOnlyRange(DateOnly start, DateOnly end) : base(start, end)
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
    public bool Overlaps(DateOnlyRange other)
        => (!IsEmpty() && other.IsEmpty())
           || (Start <= other.Start && other.End <= End)
           || (Start <= other.Start && other.Start < End && End <= other.End)
           || (other.Start <= Start && End <= other.End)
           || (other.Start <= Start && Start <= other.End && other.End <= End)
        ;

    /// <summary>
    /// Checks if the current instance is contiguous with <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><see langword="true"/> when current instance and <paramref name="other"/> are contiguous and <see langword="false"/> otherwise.</returns>
    public bool IsContiguousWith(DateOnlyRange other)
        => End == other.Start || Start == other.End;

    /// <summary>
    /// Builds a new <see cref="DateOnlyRange"/> that spans from <see cref="DateOnly.MinValue"/> up to <paramref name="date"/>
    /// </summary>
    /// <param name="date">The desired upper limit</param>
    /// <returns>a <see cref="DateOnlyRange"/> that spans up to <paramref name="date"/>.</returns>
    public static DateOnlyRange UpTo(DateOnly date) => new(DateOnly.MinValue, date);

    /// <summary>
    /// Builds a new <see cref="DateOnlyRange"/> that spans from <paramref name="date"/> up to <see cref="DateOnly.MaxValue"/>.
    /// </summary>
    /// <param name="date">The desired lower limit</param>
    /// <returns>a <see cref="DateOnlyRange"/> that starts from <paramref name="date"/>.</returns>
    public static DateOnlyRange DownTo(DateOnly date) => new(date, DateOnly.MaxValue);

    ///<inheritdoc/>
    public override sealed string ToString() => $"{Start} - {End}";

    /// <summary>
    /// Expands the current so that the resulting <see cref="DateOnlyRange"/> spans over both current and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other <see cref="DateOnlyRange"/> to span over</param>
    /// <returns>A new <see cref="DateOnlyRange"/> than spans over both current and <paramref name="other"/> range</returns>
    /// <exception cref="InvalidOperationException">if current instance does not overlap or is not contiguous with <paramref name="other"/>.</exception>
    public DateOnlyRange Merge(DateOnlyRange other)
    {
        DateOnlyRange result = Empty;
        if (IsInfinite() || other.IsInfinite())
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
            result = this with { Start = GetMinimum(Start, other.Start), End = GetMaximum(End, other.End) };
        }
        else
        {
            throw new InvalidOperationException($"Cannot build a {nameof(DateOnlyRange)} as union of '{this}' and {other}");
        }

        return result;
    }

    /// <summary>
    /// Returns the oldest <see cref="DateOnly"/> between <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static DateOnly GetMinimum(DateOnly left, DateOnly right) => left.CompareTo(right) switch
    {
        < 0 => left,
        _ => right
    };

    /// <summary>
    /// Returns the furthest <see cref="DateOnly"/> between <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static DateOnly GetMaximum(DateOnly left, DateOnly right) => left.CompareTo(right) switch
    {
        > 0 => left,
        _ => right
    };

    /// <summary>
    /// Computes  <see cref="DateOnlyRange"/> value that is common between the current instance and <paramref name="other"/>.
    /// </summary>
    /// <remarks>
    /// This methods relies on <see cref="Overlaps(DateOnlyRange)"/> to see if there can be a intersection with <paramref name="other"/>.
    /// </remarks>
    /// <param name="other">The other instance to test</param>
    /// <returns>a <see cref="DateOnlyRange"/> that represent the overlap between the current instance and <paramref name="other"/> or <see cref="Empty"/> when no intersection found.</returns>
    public DateOnlyRange Intersect(DateOnlyRange other)
    {
        DateOnlyRange result = Empty;

        if (IsInfinite())
        {
            result = other;
        }
        else if (other.IsInfinite())
        {
            result = this;
        }
        else if (Overlaps(other))
        {
            result = this with { Start = GetMaximum(Start, other.Start), End = GetMinimum(End, other.End) };
        }

        return result;
    }

    ///<inheritdoc/>
    public override ContainsResult Contains(DateOnly value) => (IsEmpty(), this == Infinite) switch
    {
        (true, _) or (_, true) => base.Contains(value),
        _ => (Start <= value && value <= End) ? ContainsResult.Yes : ContainsResult.No
    };

    ///<inheritdoc/>
    public bool IsInfinite() => (Start, End).Equals((DateOnly.MinValue, DateOnly.MaxValue));

    ///<inheritdoc/>
    public override bool IsEmpty() => Start == End;
}
#endif