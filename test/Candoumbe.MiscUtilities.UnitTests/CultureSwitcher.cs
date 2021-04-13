using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Utilities.UnitTests
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
        public void Run(string newCultureName, Action action)
        {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(newCultureName);
            action.Invoke();
        }

        public void Dispose() => CultureInfo.CurrentCulture = _currentCulture;
    }
}