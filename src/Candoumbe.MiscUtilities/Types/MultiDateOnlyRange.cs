#if NET6_0_OR_GREATER
// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// A type that optimize the storage of several <see cref="DateOnlyRange"/>.
/// </summary>
public class MultiDateOnlyRange
{
    /// <summary>
    /// Ranges holded by the current instance.
    /// </summary>
    public IEnumerable<DateOnlyRange> Ranges => _ranges.ToArray();

    private readonly ISet<DateOnlyRange> _ranges;

    /// <summary>
    /// A <see cref="MultiDateOnlyRange"/> that contains no <see cref="DateOnlyRange"/>.
    /// </summary>
    public readonly static MultiDateOnlyRange Empty = new();

    /// <summary>
    /// A <see cref="MultiDateOnlyRange"/> that overlaps any other <see cref="MultiDateOnlyRange"/>.
    /// </summary>
    public readonly static MultiDateOnlyRange Infinite = new(DateOnlyRange.Infinite);

    /// <summary>
    /// Builds a new <see cref="MultiDateOnlyRange"/> instance
    /// </summary>
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
    /// The algorithm will first tries to find if any other <see cref="DateOnlyRange"/> overlaps or abuts with <paramref name="range"/> and if so,
    /// will swap that element with the result of <c>range.Union(element)</c>
    /// </remarks>
    /// <param name="range">a range to add to the current instance</param>
    /// <exception cref="ArgumentNullException">if <paramref name="range"/> is <see langword="null"/>.</exception>
    public void Add(DateOnlyRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        if (range == DateOnlyRange.Infinite)
        {
            _ranges.Clear();
            _ranges.Add(range);
        }
        else if (!range.IsEmpty())
        {
            DateOnlyRange[] previous = _ranges.Where(item => item.IsContiguousWith(range) || item.Overlaps(range))
                                              .OrderBy(x => x.Start)
                                              .ToArray();
            if (previous.Length != 0)
            {
                previous.ForEach(item => _ranges.Remove(item));
                DateOnlyRange union = previous.Aggregate(range, (a, b) => a.Union(range).Union(b));
                _ranges.Add(union);
            }
            else
            {
                _ranges.Add(range);
            }
        }
        else if (!_ranges.Contains(DateOnlyRange.Infinite))
        {
            _ranges.Add(range);
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
    /// Tests if the current instance contains one or more <see cref="DateOnlyRange"/> which, combined together, covers the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The range to test</param>
    /// <returns><see langword="true"/> if the current instance contains <see cref="DateOnlyRange"/>s which combined together covers <paramref name="range"/> and <see langword="false"/> otherwise.</returns>
    public bool Covers(DateOnlyRange range)
    {
        bool covers = false;

        if (_ranges.Count == 0)
        {
            covers = false;
        }
        else
        {
            covers = _ranges.Any(item => (item.Overlaps(range)
                                         && item.Start <= range.Start && range.End <= item.End)
                                         || item == range
            );
        }

        return covers;
    }

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
        MultiDateOnlyRange complement = new();

        if (this == Infinite)
        {
            complement = Empty;
        }
        else if (this == Empty)
        {
            complement = Infinite;
        }
        else
        {
            DateOnlyRange[] ranges = _ranges.OrderBy(x => x.Start).ToArray();
            if (ranges.Length > 0)
            {
                DateOnlyRange localComplement = DateOnlyRange.UpTo(ranges[0].Start);
                if (!localComplement.IsEmpty())
                {
                    complement.Add(localComplement);
                }

                for (int i = 1; i < ranges.Length; i++)
                {
                    localComplement = new(ranges[i - 1].End, ranges[i].Start);
                    if (!localComplement.IsEmpty())
                    {
                        complement.Add(localComplement);
                    }
                }

                localComplement = DateOnlyRange.DownTo(ranges[^1].End);
                if (!localComplement.IsEmpty())
                {
                    complement.Add(localComplement);
                }
            }
        }

        return complement;
    }
}

#endif