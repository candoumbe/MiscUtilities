using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Comparers;

/// <summary>
/// Provides a comparer for characters that ignores case and uses invariant culture.
/// </summary>
public class InvariantCultureIgnoreCaseCharComparer : CharComparer, IComparer<char>, IEqualityComparer<char>
{
    /// <inheritdoc />
    public bool Equals(char x, char y) => char.Equals(char.ToUpperInvariant(x), char.ToUpperInvariant(y));

    /// <inheritdoc />
    public int GetHashCode(char obj) => obj.GetHashCode();

    /// <inheritdoc cref="IComparer{T}"/>
    public int Compare(char x, char y) => x.CompareTo(y);
}