using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Comparers;

/// <summary>
/// Provides a comparer for characters that uses ordinal comparison.
/// </summary>
public class OrdinalCharComparer : CharComparer, IEqualityComparer<char>
{
    /// <inheritdoc />
    public bool Equals(char x, char y) => x == y;

    /// <inheritdoc />
    public int GetHashCode(char obj) => obj.GetHashCode();
}