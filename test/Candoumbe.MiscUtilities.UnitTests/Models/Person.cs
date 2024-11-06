// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System;
using System.Diagnostics.CodeAnalysis;

namespace Candoumbe.MiscUtilities.UnitTests.Models;

[ExcludeFromCodeCoverage]
public class Appointment
{
    public DateOnly Date { get; init; }

    public TimeOnly Time { get; init; }

    public string Name { get; set; }
}