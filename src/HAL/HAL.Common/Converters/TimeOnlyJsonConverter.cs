using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// Converts a <see cref="TimeOnly"/> to and from JSON.
    /// Can also convert the string representation of a <see cref="DateTime"/> from JSON to a <see cref="TimeOnly"/> instance.
    /// </summary>
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        /// <inheritdoc/>
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var timeString = reader.GetString();

            if (string.IsNullOrWhiteSpace(timeString))
                return TimeOnly.MinValue;

            if (TimeOnly.TryParse(timeString, out var time))
                return time;

            if (DateTime.TryParse(timeString, out var dateTime))
                return TimeOnly.FromDateTime(dateTime);

            throw new FormatException($"The given time '{timeString}' cannot be parsed.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
