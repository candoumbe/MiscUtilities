// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;

namespace Candoumbe.MiscUtilities.Types;

/// <summary>
/// Result of a <see cref="Range{T}.Contains(T)"/>
/// </summary>
[Flags]
public enum ContainsResult : byte
{
    /// <summary>
    /// The result is irrelevant or not yet performed
    /// </summary>
    Unknown = 0b_00,

    /// <summary>
    /// Range contains the specified value
    /// </summary>
    Yes = 0b_01,

    /// <summary>
    /// Range does not contain the specified value
    /// </summary>
    No = 0b_10
}
