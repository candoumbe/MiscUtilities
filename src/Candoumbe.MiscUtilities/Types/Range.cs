// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// It's quite common to see comparisons where a value is checked against a range of values. Ranges are most of the time handled by a pair of values
/// and you check against them both. <see cref="Range{T}"/> instead uses a single object to represent the range as a whole, and then provides the relevant operations
/// to test to see if values fall in the <see cref="Range{T}"/> and to compare <see cref="Range{T}"/>s.
/// </summary>
/// <typeparam name="T">Type of the <see cref="Start"/> and <see cref="End"/> bounds.</typeparam>
#if !NET5_0_OR_GREATER
public abstract class Range<T> : IEquatable<Range<T>>, IComparable<Range<T>>
    where T : IComparable<T>
#else
public abstract record Range<T>(T Start, T End) : IComparable<Range<T>> where T : IComparable<T>
#endif
{
#if !NET5_0_OR_GREATER
    /// <summary>
    /// Builds a new <see cref="Range{T}"/> instance
    /// </summary>
    /// <param name="start">start of the interval</param>
    /// <param name="end">end of the interval</param>
    protected Range(T start, T end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Start of the interval
    /// </summary>
    public T Start { get; }

    /// <summary>
    /// End of the interval
    /// </summary>
    public T End { get; }

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as Range<T>);

    /// <inheritdoc/>
    public virtual bool Equals(Range<T> other) => other is not null
                                          && EqualityComparer<T>.Default.Equals(Start, other.Start)
                                          && EqualityComparer<T>.Default.Equals(End, other.End);

    ///<inheritdoc/>
#if !NETSTANDARD2_1
    public override int GetHashCode()
    {
        int hashCode = -1676728671;
        hashCode = (hashCode * -1521134295) + EqualityComparer<T>.Default.GetHashCode(Start);
        hashCode = (hashCode * -1521134295) + EqualityComparer<T>.Default.GetHashCode(End);
        return hashCode;
    }
#else
    public override int GetHashCode() => HashCode.Combine(Start, End);
#endif

    ///<inheritdoc/>
    public static bool operator ==(Range<T> left, Range<T> right) => EqualityComparer<Range<T>>.Default.Equals(left, right);

    ///<inheritdoc/>
    public static bool operator !=(Range<T> left, Range<T> right) => !(left == right);
#endif

    /// <summary>
    /// Checks if the current instance reprensents an empty range
    /// </summary>
    /// <returns><see langword="true"/> when current instance is empty and <see langword="false"/> otherwise.</returns>
    public virtual bool IsEmpty() => Start.Equals(End);

    ///<inheritdoc/>
    public int CompareTo(Range<T> other) => Start.CompareTo(other.Start);

    /// <summary>
    /// Checks if <paramref name="value"/> is between <see cref="Start"/> and <see cref="End"/>.
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns></returns>
    public virtual ContainsResult Contains(T value) => (IsEmpty(), value.CompareTo(Start), value.CompareTo(End)) switch
    {
        (true, _, _) or (_, <= 0, _) or (_, _, >= 1) => ContainsResult.No,
        _ => ContainsResult.Yes
    };

    /// <summary>
    /// Checks if the currrent instance overlaps with <paramref name="other"/>
    /// </summary>
    /// <param name="other">The other instance</param>
    /// <returns><see langword="true"/> if the current instance overlaps <paramref name="other"/> and <see langword="false"/> otherwise.</returns>
    public virtual bool Overlaps(Range<T> other) => other is not null && (Start.CompareTo(other.Start), Start.CompareTo(other.End), End.CompareTo(other.Start), End.CompareTo(other.End)) switch
    {
        ( > 0, < 0, _, _) => true,
        // current :   |-------|
        // other   :        |-------|
        (_, _, > 0, < 0) => true,
        // current :   |---------------|
        // other   :        |-------|
        ( <= 0, _, _, >= 0) => true,
        _ => other is not null && IsEmpty() && other.IsEmpty() && Start.Equals(other.Start),
    };

    /// <summary>
    /// Checks if the current instance is contiguous with <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><see langword="true"/> when current instance and <paramref name="other"/> are contiguous and <see langword="false"/> otherwise.</returns>
    public virtual bool IsContiguousWith(Range<T> other) => End.Equals(other.Start) || Start.Equals(other.End);

    ///<inheritdoc/>
    public override string ToString() => $"[{Start} - {End}]";
}
