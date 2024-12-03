namespace Candoumbe.MiscUtilities.Comparers
{
    /// <summary>
    /// Provides a base class for comparing <see cref="char"/> objects.
    /// </summary>
    /// <remarks>
    /// This class offers predefined comparers for ordinal and invariant culture, case-insensitive comparisons.
    /// </remarks>
    public abstract class CharComparer
    {
        /// <summary>
        /// Gets an instance of <see cref="OrdinalCharComparer"/> that performs ordinal comparisons of characters.
        /// </summary>
        public static OrdinalCharComparer Ordinal { get; } = new();

        /// <summary>
        /// Gets an instance of <see cref="InvariantCultureIgnoreCaseCharComparer"/>
        /// that performs a case-insensitive comparison using the invariant culture.
        /// </summary>
        public static InvariantCultureIgnoreCaseCharComparer InvariantCultureIgnoreCase { get; } = new ();
    }
}