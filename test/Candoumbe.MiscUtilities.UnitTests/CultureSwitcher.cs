// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Candoumbe.MiscUtilities.UnitTests
{
    [ExcludeFromCodeCoverage]
    public sealed class CultureSwitcher : IDisposable
    {
        private readonly CultureInfo _currentCulture;

        public CultureSwitcher() => _currentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
        /// to the specified <paramref name="newCultureName"/>.
        /// </summary>
        /// <param name="newCultureName"></param>
        /// <param name="action"></param>
        [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "Cette méthode n'est pas utilisé de manière statique")]
        public void Run(string newCultureName, Action action) => Run(CultureInfo.CreateSpecificCulture(newCultureName), action);

        /// <summary>
        /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
        /// to the specified <paramref name="newCultureName"/>.
        /// </summary>
        /// <param name="culture">Culture to use when running <paramref name="action"/>.</param>
        /// <param name="action"></param>

        public void Run(CultureInfo culture, Action action)
        {
            CultureInfo.CurrentCulture = culture;
            action.Invoke();
        }

        /// <inheritdoc />
        public void Dispose() => CultureInfo.CurrentCulture = _currentCulture;
    }
}