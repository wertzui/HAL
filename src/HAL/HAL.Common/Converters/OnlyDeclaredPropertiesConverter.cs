using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters;

/// <summary>
/// A converter that will only serialized properties which have been declared on the current type and no inherited properties.
/// </summary>
/// <typeparam name="T">The type to serialize.</typeparam>
public class OnlyDeclaredPropertiesConverter<T> : JsonConverter<T>
{
    /// <inheritdoc/>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        options.Converters.Remove(this);
        return JsonSerializer.Deserialize<T>(ref reader, newOptions);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var property in properties)
        {
            var originalName = property.Name;
            var name = options.PropertyNamingPolicy?.ConvertName(originalName) ?? originalName;
            var propertyValue = property.GetValue(value);
            var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

            if (ResourceJsonConverter.ShouldWriteProperty(property, propertyValue, defaultValue, options.DefaultIgnoreCondition))
            {
                writer.WritePropertyName(name);
                JsonSerializer.Serialize(writer, propertyValue, propertyValue?.GetType() ?? typeof(object), options);
            }
        }

        writer.WriteEndObject();
    }
}
