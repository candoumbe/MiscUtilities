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

/// <summary>
/// <see cref="JsonConverter{T}"/> implementation to handle <see cref="DateOnly"/> serialization/deserialization.
/// </summary>
/// <remarks>
/// This is a temporary fix until <see href="https://github.com/dotnet/runtime/issues/53539"/> is solved
/// </remarks>
internal class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string TimeLongFormat = "HH:mm:ss.FFFFFFF";
    private const string TimeShortFormat = "HH:mm:ss";

    ///<inheritdoc/>
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeOnly.ParseExact(reader.GetString(), TimeLongFormat, CultureInfo.InvariantCulture);

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue((value.Hour, value.Minute, value.Second, value.Millisecond) switch
        {
            (_, _, _, > 0) => value.ToString(TimeLongFormat, CultureInfo.InvariantCulture),
            _ => value.ToString(TimeShortFormat, CultureInfo.InvariantCulture)
        });
}

#endif