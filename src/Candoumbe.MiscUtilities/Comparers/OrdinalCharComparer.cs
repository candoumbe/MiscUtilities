namespace Candoumbe.MiscUtilities.Comparers;

/// <summary>
/// Provides a comparer for characters that uses ordinal comparison.
/// </summary>
public class OrdinalCharComparer : GenericEqualityComparer<char>
{
    /// <inheritdoc />
    public OrdinalCharComparer() : base((x, y) => x == y)
    {
    }
}