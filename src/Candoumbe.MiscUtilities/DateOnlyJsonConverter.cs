#if REQUIRES_DATE_AND_TIME_ONLY_SERIALIZATION_WORKAROUND
// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using System.Globalization;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Candoumbe.MiscUtilities;

/// <summary>
/// <see cref="JsonConverter{T}"/> implementation to handle <see cref="DateOnly"/> serialization/deserialization.
/// </summary>
/// <remarks>
/// This is a temporary fix until <see href="https://github.com/dotnet/runtime/issues/53539"/> is solved
/// </remarks>
internal class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    ///<inheritdoc/>
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.ParseExact(reader.GetString(), DateFormat, CultureInfo.InvariantCulture);

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
}

#endif