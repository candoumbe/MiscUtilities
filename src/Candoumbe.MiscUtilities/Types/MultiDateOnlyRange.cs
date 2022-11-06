#if NET6_0_OR_GREATER
// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// A type that optimize the storage of several <see cref="DateOnlyRange"/>.
/// </summary>
public class MultiDateOnlyRange : IEquatable<MultiDateOnlyRange>
{
    /// <summary>
    /// Ranges holded by the current instance.
    /// </summary>
    public IEnumerable<DateOnlyRange> Ranges => _ranges.ToArray();

    private readonly ISet<DateOnlyRange> _ranges;

    /// <summary>
    /// A <see cref="MultiDateOnlyRange"/> that contains no <see cref="DateOnlyRange"/>.
    /// </summary>
    public static MultiDateOnlyRange Empty => new();

    /// <summary>
    /// A <see cref="MultiDateOnlyRange"/> that overlaps any other <see cref="MultiDateOnlyRange"/>.
    /// </summary>
    public static MultiDateOnlyRange Infinite => new(DateOnlyRange.Infinite);

    /// <summary>
    /// Builds a new <see cref="MultiDateOnlyRange"/> instance
    /// </summary>
    /// <param name="ranges"></param>
    public MultiDateOnlyRange(params DateOnlyRange[] ranges)
    {
        _ranges = new HashSet<DateOnlyRange>(ranges.Length);
        foreach (DateOnlyRange range in ranges.OrderBy(x => x.Start))
        {
            Add(range);
        }
    }

    /// <summary>
    /// Adds <paramref name="range"/>.
    /// </summary>
    /// <remarks>
    /// The algorithm will first try to find if any other <see cref="DateOnlyRange"/> overlaps or abuts with <paramref name="range"/> and if so,
    /// will swap that element with the result of <c>range.Union(element)</c>
    /// </remarks>
    /// <param name="range">a range to add to the current instance</param>
    /// <exception cref="ArgumentNullException">if <paramref name="range"/> is <see langword="null"/>.</exception>
    public void Add(DateOnlyRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        if (IsInfinite())
        {
            return;
        }

        if (range.IsEmpty())
        {
            return;
        }

        if (range.IsInfinite())
        {
            _ranges.Clear();
            _ranges.Add(range);
        }
        else
        {
            DateOnlyRange[] previous = _ranges.Where(item => item.IsContiguousWith(range) || item.Overlaps(range))
                                              .OrderBy(x => x.Start)
                                              .ToArray();
            if (previous.Length != 0)
            {
                previous.ForEach(item => _ranges.Remove(item));
                DateOnlyRange union = previous.Aggregate(range, (a, b) => a.Merge(b));
                _ranges.Add(union);
            }
            else
            {
                _ranges.Add(range);
            }
        }

    }

    /// <summary>
    /// Builds a <see cref="MultiDateOnlyRange"/> instance that represents the union of the current instance with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other instance to add</param>
    /// <exception cref="ArgumentNullException">if <paramref name="other"/> is <see langword="null"/></exception>
    /// <returns>a <see cref="MultiDateOnlyRange"/> that represents the union of the current instance with <paramref name="other"/>.</returns>
    public MultiDateOnlyRange Union(MultiDateOnlyRange other) => new(_ranges.Union(other.Ranges).ToArray());

    /// <summary>
    /// Performs a "union" operation between <paramref name="left"/> and <paramref name="right"/> elements.
    /// </summary>
    /// <param name="left">The left element of the operator</param>
    /// <param name="right">The right element of the operator</param>
    /// <returns>a <see cref="MultiDateOnlyRange"/> that represents <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static MultiDateOnlyRange operator +(MultiDateOnlyRange left, MultiDateOnlyRange right) => left.Union(right);

    /// <summary>
    /// Tests if the current instance contains one or more <see cref="DateOnlyRange"/> which, combined together, overlap the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The range to test</param>
    /// <returns><see langword="true"/> if the current instance contains <see cref="DateOnlyRange"/>s which combined together overlap <paramref name="range"/> and <see langword="false"/> otherwise.</returns>
    public bool Overlaps(DateOnlyRange range)
    {
        bool covers = false;

        if (_ranges.Count == 0)
        {
            covers = false;
        }
        else
        {
            covers = _ranges.AsParallel().Any(item => (item.Overlaps(range)
                                                       && item.Start <= range.Start && range.End <= item.End)
                                                        || item == range);
        }

        return covers;
    }

    /// <summary>
    /// Checks if the current instance contains one or more <see cref="TimeOnlyRange"/>s which, combined together, covers the specified <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The range to test</param>
    /// <returns><see langword="true"/> if the current instance contains <see cref="TimeOnlyRange"/>s which combined together covers <paramref name="other"/> and <see langword="false"/> otherwise.</returns>
    public bool Overlaps(MultiDateOnlyRange other) => other is not null
                                                    && other.Ranges.AsParallel().All(range => Overlaps(range))
                                                    && _ranges.AsParallel().All(range => other.Overlaps(range));

    ///<inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (DateOnlyRange item in _ranges)
        {
            if (sb.Length > 0)
            {
                sb.Append(',');
            }
            sb.Append(item);
        }

        return sb.Insert(0, "[").Append(']').ToString();
    }

    /// <summary>
    /// Computes the complement of the current instance.
    /// </summary>
    public MultiDateOnlyRange Complement()
    {
        MultiDateOnlyRange complement;

        if (IsInfinite())
        {
            complement = Empty;
        }
        else if (IsEmpty())
        {
            complement = Infinite;
        }
        else
        {
            complement = new();
            switch (_ranges)
            {
                case { Count: 1 }:
                    {
                        DateOnlyRange current = _ranges.ElementAt(0);
                        complement.Add(DateOnlyRange.UpTo(current.Start));
                        complement.Add(DateOnlyRange.DownTo(current.End));
                    }
                    break;
                default:
                    {
                        DateOnlyRange[] ranges = _ranges.OrderBy(range => range.Start)
                                                        .ToArray();
                        for (int i = 0; i < ranges.Length; i++)
                        {
                            DateOnlyRange current = ranges[i];
                            switch (i)
                            {
                                case 0:
                                    complement.Add(DateOnlyRange.UpTo(current.Start));
                                    break;
                                case int index when index <= ranges.Length - 2:
                                    {
                                        DateOnlyRange previous = ranges[i - 1];
                                        DateOnlyRange next = ranges[i + 1];
                                        complement.Add(new DateOnlyRange(previous.End, current.Start));
                                        complement.Add(new DateOnlyRange(current.End, next.Start));
                                    }
                                    break;
                                default:
                                    {
                                        DateOnlyRange previous = ranges[i - 1];
                                        complement.Add(new DateOnlyRange(previous.End, current.Start));
                                        complement.Add(DateOnlyRange.DownTo(current.End));
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        return complement;
    }

    /// <summary>
    /// Checks if the current instance is empty.
    /// </summary>
    /// <returns><see langword="true"/> when the current instance cannot cover any other </returns>
    public bool IsEmpty() => _ranges.Count == 0 || _ranges.All(range => range.IsEmpty());

    /// <summary>
    /// Checks if the current instance is "infinite" which, if <see langword="true" />, means that it overlaps any other <see cref="DateOnlyRange"/>.
    /// </summary>
    /// <returns></returns>
    public bool IsInfinite() => _ranges.AtLeastOnce() && _ranges.All(range => range.IsInfinite());

    ///<inheritdoc/>
    public bool Equals(MultiDateOnlyRange other)
    {
        bool equals = false;
        if (other is not null)
        {
            if (!ReferenceEquals(this, other))
            {
                equals = Overlaps(other) && other.Overlaps(this);
            }
            else
            {
                equals = true;
            }
        }
        return equals;
    }

    ///<inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as MultiDateOnlyRange);

    ///<inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hashCode = new();

        foreach (DateOnlyRange range in _ranges)
        {
            hashCode.Add(range);
        }

        return hashCode.ToHashCode();
    }
}

#endif