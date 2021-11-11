// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Diagnostics.CodeAnalysis;

namespace Candoumbe.MiscUtilities.UnitTests.Models;

[ExcludeFromCodeCoverage]
public class Appointment
{
#if NET6_0_OR_GREATER
    public DateOnly Date { get; init; }

    public TimeOnly Time { get; init; }
#endif

    public string Name { get; set; }
}