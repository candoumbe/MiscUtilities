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
/// A type that optimize the storage of several <see cref="TimeOnlyRange"/>.
/// </summary>
public class MultiTimeOnlyRange
{
    /// <summary>
    /// Ranges holded by the current instance.
    /// </summary>
    public IEnumerable<TimeOnlyRange> Ranges => _ranges.ToArray();

    private readonly ISet<TimeOnlyRange> _ranges;

    /// <summary>
    /// A <see cref="MultiTimeOnlyRange"/> that contains no <see cref="TimeOnlyRange"/>.
    /// </summary>
    public readonly static MultiTimeOnlyRange Empty = new MultiTimeOnlyRange();

    /// <summary>
    /// Builds a new <see cref="MultiTimeOnlyRange"/> instance
    /// </summary>
    public MultiTimeOnlyRange(params TimeOnlyRange[] ranges)
    {
        _ranges = new HashSet<TimeOnlyRange>(ranges.Length);
        foreach (TimeOnlyRange range in ranges.OrderBy(x => x.Start))
        {
            Add(range);
        }
    }

    /// <summary>
    /// Adds <paramref name="range"/>.
    /// </summary>
    /// <remarks>
    /// The algorithm will first tries to find if any other <see cref="TimeOnlyRange"/> overlaps or abuts with <paramref name="range"/> and if so,
    /// will swap that element with the result of <c>range.Union(element)</c>
    /// </remarks>
    /// <param name="range"></param>
    /// <exception cref="ArgumentNullException">if <paramref name="range"/> is <c>null</c>.</exception>
    public void Add(TimeOnlyRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        if (range == TimeOnlyRange.AllDay)
        {
            _ranges.Clear();
            _ranges.Add(range);
        }
        else if (!range.IsEmpty())
        {
            TimeOnlyRange[] previous = _ranges.Where(item => item.IsContiguousWith(range) || item.Overlaps(range))
                                              .OrderBy(x => x.Start)
                                              .ToArray();
            if (previous.Length != 0)
            {
                previous.ForEach(item => _ranges.Remove(item));
                TimeOnlyRange union = previous.Aggregate(range, (a, b) => a.Union(b));
                _ranges.Add(union);
            }
            else
            {
                _ranges.Add(range);
            }
        }
        else if(!_ranges.Contains(TimeOnlyRange.AllDay))
        {
            _ranges.Add(range);
        }
    }

    /// <summary>
    /// Builds a <see cref="MultiTimeOnlyRange"/> instance that represents the union of the current instance with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other instance to add</param>
    /// <exception cref="ArgumentNullException">if <paramref name="other"/> is <c>null</c></exception>
    /// <returns>a <see cref="MultiTimeOnlyRange"/> that represents the union of the current instance with <paramref name="other"/>.</returns>
    public MultiTimeOnlyRange Union(MultiTimeOnlyRange other) => new(_ranges.Concat(other.Ranges).ToArray());

    /// <summary>
    /// Performs a "union" operation between <paramref name="left"/> and <paramref name="right"/> elements.
    /// </summary>
    /// <param name="left">The left element of the operator</param>
    /// <param name="right">The right element of the operator</param>
    /// <returns>a <see cref="MultiTimeOnlyRange"/> that represents <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static MultiTimeOnlyRange operator +(MultiTimeOnlyRange left, MultiTimeOnlyRange right) => left.Union(right);

    /// <summary>
    /// Computes the complement of <paramref name="source"/>
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static MultiTimeOnlyRange operator -(MultiTimeOnlyRange source) => source.Complement();

    /// <summary>
    /// Creates a <see cref="MultiTimeOnlyRange"/> that is the exact complement of the current instance
    /// </summary>
    /// <returns></returns>
    public MultiTimeOnlyRange Complement() => _ranges.AtLeastOnce()
        ? new(_ranges.AsParallel().Select(range => -range).ToArray())
        : new( new []{ TimeOnlyRange.Empty } );

    /// <summary>
    /// Tests if the current instance contains one or more ranges which, combined together, covers the specified <paramref name="range"/>. 
    /// </summary>
    /// <param name="range">The range to test</param>
    /// <returns><see langword="true"/> if the current instance contains <see cref="TimeOnlyRange"/>s which combined together covers <paramref name="range"/> and <see langword="false"/> otherwise.</returns>
    public bool Covers(TimeOnlyRange range)
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
}

#endif