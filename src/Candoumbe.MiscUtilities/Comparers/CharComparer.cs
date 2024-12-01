namespace Candoumbe.MiscUtilities.Comparers
{
    public abstract class CharComparer
    {
        public static OrdinalCharComparer Ordinal { get; } = new();

        public static InvariantCultureIgnoreCaseCharComparer InvariantCultureIgnoreCase { get; } = new ();
    }
}