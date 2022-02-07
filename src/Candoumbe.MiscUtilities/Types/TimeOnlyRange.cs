// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

#if NET6_0_OR_GREATER
using System;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// A <see cref="TimeOnly"/> range/
/// </summary>
public record TimeOnlyRange : Range<TimeOnly>
{
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
    /// <returns><c>true</c> when current instance and <paramref name="other"/> overlaps each other <c>false</c> otherwise.</returns>
    public bool Overlaps(TimeOnlyRange other)
        => Start.IsBetween(other.Start, other.End)
           || End.IsBetween(other.Start, other.End)
           ||Start <= other.Start && other.End <= End;

    /// <summary>
    /// Checks if <paramref name="other"/> and current instances are contiguoous.
    /// </summary>
    /// <param name="other"></param>
    /// <returns><c>true</c> when current instance and <paramref name="other"/> are contiguous and <c>false</c> otherwise.</returns>
    public bool IsContiguousWith(TimeOnlyRange other)
        => End == other.Start || Start == other.End;

    ///<inheritdoc/>
    public override sealed string ToString() => $"{Start} - {End}";

    /// <summary>
    /// Builds a <see cref="TimeOnlyRange"/> that expands from <see cref="TimeOnly.MinValue"/> up to <paramref name="reference"/>
    /// </summary>
    /// <param name="reference">The desired updper bound of the bound</param>
    /// <returns>A new <see cref="TimeOnlyRange"/> that expands from <see cref="TimeOnly.MinValue"/> to <paramref name="reference"/></returns>
    public static TimeOnlyRange UpTo(TimeOnly reference) => new(TimeOnly.MinValue, reference);
    /// <summary>
    /// Builds a <see cref="TimeOnlyRange"/> that expands from <paramref name="reference"/> to <see cref="TimeOnly.MaxValue"/>.
    /// </summary>
    /// <param name="reference">The desired updper bound of the bound</param>
    /// <returns>A new <see cref="TimeOnlyRange"/> that expands from <paramref name="reference"/> to <see cref="TimeOnly.MaxValue"/>.</returns>
    public static TimeOnlyRange DownTo(TimeOnly reference) => new(reference, TimeOnly.MaxValue);
}
#endif