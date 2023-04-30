using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// Converts a <see cref="DateOnly"/> to and from JSON.
    /// Can also convert the string representation of a <see cref="DateTime"/> from JSON to a <see cref="DateOnly"/> instance.
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        /// <inheritdoc/>
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                return DateOnly.MinValue;

            if (DateOnly.TryParse(dateString, out var date))
                return date;

            if (DateTime.TryParse(dateString, out var dateTime))
                return DateOnly.FromDateTime(dateTime);

            throw new FormatException($"The given date '{dateString}' cannot be parsed.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}
