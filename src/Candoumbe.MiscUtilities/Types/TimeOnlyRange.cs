// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if NET6_0_OR_GREATER
using System;
using System.Diagnostics;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// <para>
/// A type suitable to represent a "time range"
/// </para>
/// </summary>
/// <remarks>
/// <see cref="TimeOnlyRange"/> allows to express ranges like :
/// <list type="bullet">
///     <item>"from 22PM to 6AM" : <c>new TimeOnlyRange(new TimeOnly(22, 0), new TimeOnly(6))</c></item>
///     <item>"all day" : <c>TimeOnlyRange.AllDay</c></item>
/// </list>
/// </remarks>
public record TimeOnlyRange : Range<TimeOnly>
{
    private TimeSpan Span => End - Start;

    /// <summary>
    /// Builds a new <see cref="TimeOnlyRange"/> instance.
    /// </summary>
    /// <param name="start">start of the range</param>
    /// <param name="end">end of the range</param>
    public TimeOnlyRange(TimeOnly start, TimeOnly end) : base(start, end)
    {
    }

    /// <summary>
    /// Tests wheters the current instance overlaps with <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><c>true</c> when current instance and <paramref name="other"/> overlaps each other and <c>false</c> otherwise.</returns>
    public bool Overlaps(TimeOnlyRange other)
        => (Start < End) switch
        {
            true => (Start.IsBetween(other.Start, other.End) && Start != other.Start && Start != other.End)
                               || (End.IsBetween(other.Start, other.End) && End != other.Start && End != other.End)
                               || (Start <= other.Start && other.End <= End)
                               || (other.Start.IsBetween(Start, End) && Start != other.Start && Start != other.End)
                               || (other.End.IsBetween(Start, End) && End != other.Start && End != other.End)
                               || (other.Start <= Start && End <= other.End)
            ,
            false => (Start.IsBetween(other.Start, other.End) && Start != other.Start && Start != other.End)
                               || (End.IsBetween(other.Start, other.End) && End != other.Start && End != other.End)
                               || (Start <= other.Start && other.End <= End)
                               || (other.Start.IsBetween(Start, End) && Start != other.Start && Start != other.End)
                               || (other.End.IsBetween(Start, End) && End != other.Start && End != other.End)
                               || (other.Start <= End && Start <= other.End)
        };

    /// <summary>
    /// Checks if <paramref name="other"/> and current instances are contiguoous.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><c>true</c> when current instance and <paramref name="other"/> are contiguous and <c>false</c> otherwise.</returns>
    public bool IsContiguousWith(TimeOnlyRange other) => IsContiguousWithStart(other) || IsContiguousWithEnd(other);

    private bool IsContiguousWithStart(TimeOnlyRange other) => End == other.Start;

    private bool IsContiguousWithEnd(TimeOnlyRange other) => Start == other.End;

    ///<inheritdoc/>
    public override sealed string ToString() => $"{Start} - {End}";

    /// <summary>
    /// Builds a <see cref="TimeOnlyRange"/> that spans from <see cref="TimeOnly.MinValue"/> up to <paramref name="reference"/>
    /// </summary>
    /// <param name="reference">The desired updper bound of the bound</param>
    /// <returns>A new <see cref="TimeOnlyRange"/> that spans from <see cref="TimeOnly.MinValue"/> to <paramref name="reference"/></returns>
    public static TimeOnlyRange UpTo(TimeOnly reference) => new(TimeOnly.MinValue, reference);

    /// <summary>
    /// Builds a <see cref="TimeOnlyRange"/> that expands from <paramref name="reference"/> to <see cref="TimeOnly.MaxValue"/>.
    /// </summary>
    /// <param name="reference">The desired updper bound of the bound</param>
    /// <returns>A new <see cref="TimeOnlyRange"/> that spans from <paramref name="reference"/> to <see cref="TimeOnly.MaxValue"/>.</returns>
    public static TimeOnlyRange DownTo(TimeOnly reference) => new(reference, TimeOnly.MaxValue);

    /// <summary>
    /// Expands the current so that the resulting <see cref="TimeOnlyRange"/> spans over both current and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other <see cref="TimeOnlyRange"/> to span over</param>
    /// <returns>A new <see cref="TimeOnlyRange"/> than spans over both current and <paramref name="other"/> range</returns>
    /// <exception cref="InvalidOperationException">if either : current instance does not overlap or is not continuous with <paramref name="other"/>.</exception>
    public TimeOnlyRange Union(TimeOnlyRange other)
    {
        TimeOnlyRange result = Empty;
        if (this == AllDay || other == AllDay)
        {
            result = AllDay;
        }
        else if (IsEmpty())
        {
            result = other;
        }
        else if (other.IsEmpty())
        {
            result = this;
        }
        else if (Overlaps(other))
        {
            if (Start > End)
            {
                TimeOnlyRange complement = Complement(this);
                if (!complement.Overlaps(other))
                {
                    result = this;
                }
                else if (other.Start < other.End)
                {
                    TimeOnlyRange intersection = complement.Intersect(other);
                    Debug.Assert(!intersection.IsEmpty());

                    if (IsContiguousWith(intersection))
                    {
                        result = IsContiguousWithStart(intersection)
                                        ? this with { End = End.Add(intersection.Span) }
                                        : this with { Start = Start.Add(-intersection.Span) };

                        if (result.End == result.Start)
                        {
                            result = AllDay;
                        }
                    }
                }
            }
            else
            {
                result = this with { Start = GetMinimum(Start, other.Start), End = GetMaximum(other.End, End) };
            }
        }
        else if (IsContiguousWith(other))
        {
            if (Complement(this) == other)
            {
                result = AllDay;
            }
            else
            {
                result = this with { Start = GetMinimum(Start, other.Start), End = GetMaximum(other.End, End) };
            }
        }

        return Normalize(result);

        // Local function to get the max of to TimeOnly
        static TimeOnly GetMinimum(TimeOnly left, TimeOnly right) => left.CompareTo(right) switch
        {
            < 0 => left,
            _ => right
        };

        // Local function to get the max of to TimeOnly
        static TimeOnly GetMaximum(TimeOnly left, TimeOnly right) => left.CompareTo(right) switch
        {
            > 0 => left,
            _ => right
        };

        static TimeOnlyRange Normalize(TimeOnlyRange range)
        {
            TimeOnlyRange normalizedOutput;

            if (range.End - range.Start >= AllDay.Span)
            {
                normalizedOutput = AllDay.ShiftTo(range.Start);
            }
            else if (range.End - range.Start <= TimeSpan.Zero)
            {
                normalizedOutput = Empty;
            }
            else
            {
                normalizedOutput = range;
            }

            return normalizedOutput;
        }
    }

    /// <summary>
    /// A <see cref="TimeOnlyRange"/> that span accros every other <see cref="TimeOnlyRange"/>.
    /// </summary>
    public static TimeOnlyRange AllDay => new(TimeOnly.MinValue, TimeOnly.MaxValue);

    /// <summary>
    /// An empty <see cref="TimeOnlyRange"/>.
    /// </summary>
    public static TimeOnlyRange Empty => new(TimeOnly.MinValue, TimeOnly.MinValue);

    /// <summary>
    /// Computes  <see cref="TimeOnlyRange"/> value that is common between the current instance and <paramref name="other"/>.
    /// </summary>
    /// <remarks>
    /// This methods relies on <see cref="Overlaps(TimeOnlyRange)"/> to see if there can be a intersection with <paramref name="other"/>.
    /// </remarks>
    /// <param name="other">The other instance to test</param>
    /// <returns>a <see cref="TimeOnlyRange"/> that represent the overlap between the current instance and <paramref name="other"/> or <see cref="Empty"/> when no intersection found.</returns>
    public TimeOnlyRange Intersect(TimeOnlyRange other)
    {
        TimeOnlyRange intersection = Empty;
        if (this == AllDay)
        {
            intersection = other;
        }
        else if (other == AllDay)
        {
            intersection = this;
        }
        else if (!IsEmpty() && !other.IsEmpty() && Overlaps(other))
        {
            TimeOnlyRange left = this;
            TimeOnlyRange right = other;

            intersection = (left.Start.IsBetween(right.Start, right.End), left.End.IsBetween(right.Start, right.End)) switch
            {
                /*
                 * left      :    |----|
                 * right     : |------------|
                 * expected  :    |----|
                 */
                (true, true) => new(left.Start, left.End),
                /*
                 * left      :    |------------|
                 * right     : |------------|
                 * expected  :    |----|
                 */
                (true, false) => new(left.Start, right.End),
                /*
                 * left      : |------------|
                 * right     :       |------------|
                 * expected  :       |------|
                 */
                (false, true) => new(right.Start, left.End),
                /*
                 * left      : |------------|
                 * right     :       |----|
                 * expected  :       |----|
                 */
                _ => (right.Start.IsBetween(left.Start, left.End), right.End.IsBetween(left.Start, left.End)) switch
                {
                    (true, true) => new(right.Start, right.End),
                    (true, false) => new(right.Start, left.End),
                    (false, true) => new(left.Start, right.End),
                    _ => Empty
                }
            };
        }

        return intersection;
    }

    /// <summary>
    /// Creates a new <see cref="TimeOnlyRange"/> that complement <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The <see cref="TimeOnlyRange"/> to "invert" </param>
    /// <returns></returns>
    private static TimeOnlyRange Complement(TimeOnlyRange input) => (input == Empty, input == AllDay) switch
    {
        (true, _) => AllDay,
        (false, true) => Empty,
        _ => input with { Start = input.End, End = input.Start }
    };

    private TimeOnlyRange ShiftTo(TimeOnly offset)
        => this with { Start = offset, End = offset.Add(Span) };
}
#endif