#if NET6_0_OR_GREATER
// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif
using System.Text;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// A type that optimize the storage of several <see cref="TimeOnlyRange"/>.
/// </summary>
public class MultiTimeOnlyRange : IEquatable<MultiTimeOnlyRange>
#if NET7_0_OR_GREATER
    , IAdditionOperators<MultiTimeOnlyRange, MultiTimeOnlyRange, MultiTimeOnlyRange>
    , IAdditionOperators<MultiTimeOnlyRange, TimeOnlyRange, MultiTimeOnlyRange>
    , IUnaryNegationOperators<MultiTimeOnlyRange, MultiTimeOnlyRange>
#endif

{
    /// <summary>
    /// Ranges holded by the current instance.
    /// </summary>
    public IEnumerable<TimeOnlyRange> Ranges => _ranges.ToArray();

    private readonly ISet<TimeOnlyRange> _ranges;

    /// <summary>
    /// A <see cref="MultiTimeOnlyRange"/> that contains no <see cref="TimeOnlyRange"/>.
    /// </summary>
    public static MultiTimeOnlyRange Empty => new();

    /// <summary>
    /// A <see cref="MultiTimeOnlyRange"/> that covers any other <see cref="MultiTimeOnlyRange"/>s.
    /// </summary>
    public static MultiTimeOnlyRange Infinite => new(TimeOnlyRange.AllDay);

    /// <summary>
    /// Builds a new <see cref="MultiTimeOnlyRange"/> instance
    /// </summary>
    public MultiTimeOnlyRange(params TimeOnlyRange[] ranges)
    {
        _ranges = new HashSet<TimeOnlyRange>();
        IReadOnlySet<TimeOnlyRange> localRanges = ranges.OrderBy(x => x.Start)
                                                        .ToHashSet();
        localRanges.ForEach(Add);
    }

    /// <summary>
    /// Adds <paramref name="range"/>.
    /// </summary>
    /// <remarks>
    /// The algorithm will first tries to find if any other <see cref="TimeOnlyRange"/> overlaps or abuts with <paramref name="range"/> and if so,
    /// will swap that element with the result of <c>range.Union(element)</c>
    /// </remarks>
    /// <param name="range"></param>
    /// <exception cref="ArgumentNullException">if <paramref name="range"/> is <see langword="null"/>.</exception>
    public MultiTimeOnlyRange Add(TimeOnlyRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        if (!IsInfinite() && !range.IsEmpty())
        {
            if (range.IsAllDay())
            {
                _ranges.Clear();
                _ranges.Add(range);
            }
            else
            {
                TimeOnlyRange[] previous = _ranges.Where(item => item.IsContiguousWith(range) || item.Overlaps(range))
                                                  .OrderBy(x => x.Start)
                                                  .ToArray();
                if (previous.Length != 0)
                {
                    previous.ForEach(item => _ranges.Remove(item));
                    TimeOnlyRange union = previous.Aggregate(range, (left, right) => left.Merge(right));
                    _ranges.Add(union);
                }
                else
                {
                    _ranges.Add(range);
                }
            }
        }

        return this;
    }

    ///<inheritdoc/>
    public static MultiTimeOnlyRange operator +(MultiTimeOnlyRange left, TimeOnlyRange right) => left?.Add(right);

    /// <summary>
    /// Builds a <see cref="MultiTimeOnlyRange"/> instance that represents the union of the current instance with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other instance to add</param>
    /// <returns>a <see cref="MultiTimeOnlyRange"/> that represents the union of the current instance with <paramref name="other"/>.</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="other"/> is <see langword="null"/></exception>
    public MultiTimeOnlyRange Merge(MultiTimeOnlyRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        MultiTimeOnlyRange union;
        switch ((IsEmpty(), IsInfinite(), other.IsEmpty(), other.IsInfinite()))
        {
            case (true, _, _, _):
                union = other;
                break;
            case (_, _, true, _):
                union = this;
                break;
            case (_, true, _, _) or (_, _, _, true):
                union = Infinite;
                break;
            default:
                {
                    if (Complement() == other)
                    {
                        union = Infinite;
                    }
                    else
                    {
                        union = new();
                        IEnumerable<TimeOnlyRange> ranges = _ranges.Concat(other.Ranges)
                                                                   .OrderBy(range => range.Start);
                        ranges.ForEach(union.Add);
                    }
                }
                break;
        }

        return union;
    }

    /// <summary>
    /// Performs a "union" operation between <paramref name="left"/> and <paramref name="right"/> elements.
    /// </summary>
    /// <param name="left">The left element of the operator</param>
    /// <param name="right">The right element of the operator</param>
    /// <returns>a <see cref="MultiTimeOnlyRange"/> that is equivalent to <c>left.Union(right)</c>.</returns>
    public static MultiTimeOnlyRange operator +(MultiTimeOnlyRange left, MultiTimeOnlyRange right) => left.Merge(right);

    /// <summary>
    /// Computes the complement of <paramref name="source"/>
    /// </summary>
    /// <param name="source"></param>
    /// <returns>a <see cref="MultiTimeOnlyRange"/> that is the complement of <paramref name="source"/>.</returns>
    public static MultiTimeOnlyRange operator -(MultiTimeOnlyRange source) => source.Complement();

    /// <summary>
    /// Computes the difference between <paramref name="left"/> and <paramref name="right"/>.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>A <see cref="MultiTimeOnlyRange"/> that contains <see cref="TimeOnlyRange"/>s that are not covered by both <paramref name="left"/> and <paramref name="right"/>.</returns>
    public static MultiTimeOnlyRange operator -(MultiTimeOnlyRange left, MultiTimeOnlyRange right) => left.Diff(right);

    /// <summary>
    /// Computes the difference between current instance and <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    private MultiTimeOnlyRange Diff(MultiTimeOnlyRange other) => (IsEmpty(), IsInfinite(), other.IsEmpty(), other.IsInfinite()) switch
    {
        (true, _, true, _) or (_, true, _, true) => Empty,
        (false, _, true, _) => this,
        (true, _, false, _) => other,
        _ => -Merge(other)
    };

    /// <summary>
    /// Creates a <see cref="MultiTimeOnlyRange"/> that is the exact complement of the current instance
    /// </summary>
    /// <returns></returns>
    public MultiTimeOnlyRange Complement()
    {
        MultiTimeOnlyRange complement;
        if (IsEmpty())
        {
            complement = Infinite;
        }
        else if (IsInfinite())
        {
            complement = Empty;
        }
        else
        {
            complement = new();
            switch (_ranges)
            {
                case { Count: 1 }:
                    {
                        TimeOnlyRange current = _ranges.ElementAt(0);
                        complement.Add(TimeOnlyRange.UpTo(current.Start));
                        complement.Add(TimeOnlyRange.DownTo(current.End));
                    }
                    break;
                default:
                    for (int i = 0; i < _ranges.Count; i++)
                    {
                        TimeOnlyRange current = _ranges.ElementAt(i);
                        switch (i)
                        {
                            case 0:
                                complement.Add(TimeOnlyRange.UpTo(current.Start));
                                break;
                            case int index when index <= _ranges.Count - 2:
                                {
                                    TimeOnlyRange previous = _ranges.ElementAt(i - 1);
                                    TimeOnlyRange next = _ranges.ElementAt(i + 1);
                                    complement.Add(new TimeOnlyRange(previous.End, current.Start));
                                    complement.Add(new TimeOnlyRange(current.End, next.Start));
                                }
                                break;
                            default:
                                {
                                    TimeOnlyRange previous = _ranges.ElementAt(i - 1);
                                    complement.Add(new TimeOnlyRange(previous.End, current.Start));
                                    complement.Add(TimeOnlyRange.DownTo(current.End));
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        return complement;
    }

    /// <summary>
    /// Checks if the current instance covers no other <see cref="MultiTimeOnlyRange"/> ranges
    /// </summary>
    /// <returns><see langword="true"/> if the current instance covers no other <see cref="MultiTimeOnlyRange"/>s and <see langword="false"/> otherwise.</returns>
    public bool IsEmpty() => _ranges.Count == 0 || _ranges.All(range => range.IsEmpty());

    /// <summary>
    /// Checks if the current instance covers all other <see cref="MultiTimeOnlyRange"/> ranges
    /// </summary>
    /// <returns><see langword="true"/> if the current instance covers all other <see cref="MultiTimeOnlyRange"/>s and <see langword="false"/> otherwise.</returns>
    public bool IsInfinite() => Covers(TimeOnlyRange.AllDay);

    /// <summary>
    /// Checks if the current instance contains one or more <see cref="TimeOnlyRange"/>s which, combined together, covers the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The range to test</param>
    /// <returns><see langword="true"/> if the current instance contains <see cref="TimeOnlyRange"/>s which combined together covers <paramref name="range"/> and <see langword="false"/> otherwise.</returns>
    public bool Covers(TimeOnlyRange range) => _ranges.Count != 0
                                               && _ranges.AsParallel().Any(item => item == range
                                                                                   || (item.Overlaps(range) && item.Start <= range.Start && range.End <= item.End));

    /// <summary>
    /// Checks if the current instance contains one or more <see cref="TimeOnlyRange"/>s which, combined together, covers the specified <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The range to test</param>
    /// <returns><see langword="true"/> if the current instance contains <see cref="TimeOnlyRange"/>s which combined together covers <paramref name="other"/> and <see langword="false"/> otherwise.</returns>
    public bool Covers(MultiTimeOnlyRange other) => other is not null
                                                    && other.Ranges.AsParallel().All(range => Covers(range))
                                                    && _ranges.AsParallel().All(range => other.Covers(range));

    ///<inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (TimeOnlyRange item in _ranges)
        {
            if (sb.Length > 0)
            {
                sb.Append(',');
            }
            sb.Append(item);
        }

        return sb.Insert(0, "[").Append(']').ToString();
    }

    ///<inheritdoc />
    public override bool Equals(object obj) => Equals(obj as MultiTimeOnlyRange);

    ///<inheritdoc />
    public bool Equals(MultiTimeOnlyRange other)
    {
        bool equals = false;
        if (other is not null)
        {
            if (!ReferenceEquals(this, other))
            {
                equals = Covers(other) && other.Covers(this);
            }
            else
            {
                equals = true;
            }
        }
        return equals;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hashCode = new();

        _ranges.ForEach(range => hashCode.Add(range));

        return hashCode.ToHashCode();
    }
}

#endif