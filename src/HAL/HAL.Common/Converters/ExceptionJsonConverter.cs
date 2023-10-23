using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// Converts an Exception to JSON, omits the TargetSite property.
    /// </summary>
    public class ExceptionJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Exception).IsAssignableFrom(typeToConvert);
        }

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(ExceptionJsonConverter<>).MakeGenericType(typeToConvert);
            var converter = Activator.CreateInstance(converterType) as JsonConverter;

            return converter;
        }
    }

    /// <summary>
    /// Converts an Exception to JSON, omits the TargetSite property.
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    public class ExceptionJsonConverter<TException> : JsonConverter<TException>
        where TException : Exception
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Exception).IsAssignableFrom(typeToConvert);
        }

        /// <inheritdoc/>
        public override TException? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserializing exceptions is not unsafe and not allowed.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TException value, JsonSerializerOptions options)
        {
            var serializableProperties = value
                .GetType()
                .GetProperties()
                .Where(p => p.Name != nameof(Exception.TargetSite)) // TargetSite cannot be serialized
                .Select(p => new { p.Name, Value = p.GetValue(value) });

            if (options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull))
            {
                serializableProperties = serializableProperties.Where(uu => uu.Value != null);
            }

            if (options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingDefault))
            {
                serializableProperties = serializableProperties.Where(uu => uu.Value != default);
            }

            writer.WriteStartObject();

            foreach (var prop in serializableProperties)
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
