using System.Collections.Generic;

namespace Candoumbe.MiscUtilities.Comparers;

public class InvariantCultureIgnoreCaseCharComparer : CharComparer, IComparer<char>, IEqualityComparer<char>
{
    /// <inheritdoc />
    public bool Equals(char x, char y) => char.ToUpperInvariant(x) == char.ToUpperInvariant(y);

    /// <inheritdoc />
    public int GetHashCode(char obj) => obj.GetHashCode();

    /// <inheritdoc />
    public int Compare(char x, char y) => x.CompareTo(y);
}