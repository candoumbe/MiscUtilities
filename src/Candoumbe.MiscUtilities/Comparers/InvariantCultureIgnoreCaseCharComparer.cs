using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Comparers;

/// <summary>
/// Provides a comparer for characters that ignores case and uses invariant culture.
/// </summary>
public class InvariantCultureIgnoreCaseCharComparer : GenericEqualityComparer<char>, IComparer<char>
{
    /// <summary>
    /// Builds a new <see cref="InvariantCultureIgnoreCaseCharComparer"/> instance.
    /// </summary>
    public InvariantCultureIgnoreCaseCharComparer() : base((x, y) => char.Equals(char.ToUpperInvariant(x), char.ToUpperInvariant(y)))
    {
    }

    /// <inheritdoc />
    public int Compare(char x, char y) => x.CompareTo(y);
}