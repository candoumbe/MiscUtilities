// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Candoumbe.MiscUtilities;

/// <summary>
/// Allows switching both <see cref="CultureInfo.CurrentCulture"/> and <see cref="CultureInfo.CurrentUICulture"/> within a given scope.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDisposable"/> to allow restoring the previous culture
/// when leaving the scope.
/// <para>
/// This class is thread-safe since <see cref="CultureInfo.CurrentCulture"/> is thread-specific.
/// </para>
/// </remarks>
public sealed class CultureSwitcher
{
    private readonly CultureInfo _currentCulture;

    /// <summary>
    /// Creates a new instance of <see cref="CultureSwitcher"/>.
    /// </summary>
    /// <remarks>The current culture (given by <c>CultureInfo.CurrentCulture</c> is saved before entering the scope.</remarks>
    public CultureSwitcher() => _currentCulture = CultureInfo.CurrentCulture;

    /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
    /// to the specified <paramref name="newCultureName"/>.
    /// </summary>
    /// <param name="newCultureName">Name of the culture under which </param>
    /// <param name="action">The action to perform</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// <paramref name="action"/> is executed under the specified culture in an isolated thread.
    /// </remarks>
    /// <exception cref="CultureNotFoundException">Thrown when the specified culture cannot be found.</exception>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="newCultureName"/> is <see langword="null"/> or empty.</exception>
    [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "Cette méthode n'est pas utilisé de manière statique")]
    public async Task RunAsync(string newCultureName, Action<CancellationToken> action, CancellationToken cancellationToken = default) => await RunAsync(CultureInfo.CreateSpecificCulture(newCultureName), action, cancellationToken);

   /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
    /// to the specified <paramref name="newCultureName"/>.
    /// </summary>
    /// <param name="newCultureName">Name of the culture under which </param>
    /// <param name="action">The action to perform</param>
    /// <remarks>
    /// <paramref name="action"/> is executed under the specified culture in an isolated thread.
    /// </remarks>
    /// <exception cref="CultureNotFoundException">Thrown when the specified culture cannot be found.</exception>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="newCultureName"/> is <see langword="null"/> or empty.</exception>
    [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "Cette méthode n'est pas utilisé de manière statique")]
    public void Run(string newCultureName, Action action) => Run(CultureInfo.CreateSpecificCulture(newCultureName), action);

    /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
    /// to the specified <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">Culture to use when running <paramref name="action"/>.</param>
    /// <param name="action">The action that will be performed under the specified <paramref name="culture"/></param>
    /// <param name="cancellationToken"></param>
    public async Task RunAsync(CultureInfo culture, Action<CancellationToken> action, CancellationToken cancellationToken = default)
    {
        await Task.Factory.StartNew(() =>
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                action.Invoke(cancellationToken);

            }
            finally
            {
                CultureInfo.CurrentCulture = _currentCulture;
                CultureInfo.CurrentUICulture = _currentCulture;
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs the specified <see cref="action"/> <strong>AFTER</strong> switching <see cref="CultureInfo.CurrentCulture"/>
    /// to the specified <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">Culture to use when running <paramref name="action"/>.</param>
    /// <param name="action">The action that will be performed under the specified <paramref name="culture"/></param>
    public void Run(CultureInfo culture, Action action)
    {
        try
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            action.Invoke();

        }
        finally
        {
            CultureInfo.CurrentCulture = _currentCulture;
            CultureInfo.CurrentUICulture = _currentCulture;
        }
    }
}