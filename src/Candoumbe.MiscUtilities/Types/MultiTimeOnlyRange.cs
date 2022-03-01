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
    public IEnumerable<TimeOnlyRange> Ranges => _ranges;

    private readonly IList<TimeOnlyRange> _ranges;

    /// <summary>
    /// A <see cref="MultiTimeOnlyRange"/> that contains no <see cref="TimeOnlyRange"/>.
    /// </summary>
    public readonly static MultiTimeOnlyRange Empty = new MultiTimeOnlyRange();

    /// <summary>
    /// Builds a new <see cref="MultiTimeOnlyRange"/> instance
    /// </summary>
    public MultiTimeOnlyRange(params TimeOnlyRange[] ranges)
    {
        _ranges = new List<TimeOnlyRange>(ranges.Length);
        TimeOnlyRange[] copies = ranges.OrderBy(x => x.Start)
                                       .ToArray();

        for (int i = 0; i < copies.Length; i++)
        {
            Add(copies[i]);
        }
    }

    /// <summary>
    /// Adds <paramref name="range"/>.
    /// </summary>
    /// <remarks>
    /// The algorithm will first tries to find if any other <see cref="TimeOnlyRange"/> overlaps <paramref name="range"/> and if so,
    /// will swap that element with the result of <c>range.Union(element)</c>
    /// </remarks>
    /// <param name="range"></param>
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
            TimeOnlyRange previous = _ranges.SingleOrDefault(item => item.IsContiguousWith(range) || item.Overlaps(range), range);

            _ranges.Remove(previous);
            _ranges.Add(previous.Union(range));
        }
    }

    /// <summary>
    /// Adds all <see cref="TimeOnlyRange"/>s 
    /// </summary>
    /// <param name="other"></param>
    /// <exception cref="ArgumentNullException">if <paramref name="other"/> is <c>null</c></exception>
    public MultiTimeOnlyRange Union(MultiTimeOnlyRange other) => new(_ranges.Concat(other.Ranges).ToArray());

    /// <summary>
    /// Creates a <see cref="MultiTimeOnlyRange"/> that is the exact complement of the current instance
    /// </summary>
    /// <returns></returns>
    public MultiTimeOnlyRange Complement()
    {
        MultiTimeOnlyRange complement = new MultiTimeOnlyRange();

        return complement;

    }

    /// <summary>
    /// Tests if the current instance contains one or more ranges which, combined together, covers the specified <paramref name="range"/>. 
    /// </summary>
    /// <param name="range">The range to test</param>
    /// <returns><c>true</c> if the current instance contains <see cref="TimeOnlyRange"/>s which combined together covers <paramref name="range"/> and <c>false</c> otherwise.</returns>
    public bool Covers(TimeOnlyRange range)
    {
        bool covers = false;

        if (_ranges.Count == 0)
        {
            covers = false;
        }
        else
        {
            covers = _ranges.Any(item => item.Overlaps(range)
                                         && item.Start <= range.Start && range.End <= item.End
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