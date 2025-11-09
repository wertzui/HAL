using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HAL.Common.Converters;

/// <summary>
/// A converter that can read and write <see cref="Resource"/>.
/// </summary>
/// <seealso cref="JsonConverter{Resource}" />
public class ResourceJsonConverter : JsonConverter<Resource>, IJsonTypeInfoResolver
{
    /// <summary>
    /// Determines if the given property should be written to the JSON payload based on the ignore condition.
    /// </summary>
    /// <param name="property">The property which should be written.</param>
    /// <param name="value">The value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="ignoreCondition">The ignore condition.</param>
    /// <returns></returns>
    public static bool ShouldWriteProperty(PropertyInfo property, object? value, object? defaultValue, JsonIgnoreCondition ignoreCondition)
    {
        var ignoreAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>();
        if (ignoreAttribute is not null)
            return ShouldWriteValue(value, defaultValue, ignoreAttribute.Condition);

        return ShouldWriteValue(value, defaultValue, ignoreCondition);
    }

    /// <summary>
    /// Determines if the given value should be written to the JSON payload based on the ignore condition.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="ignoreCondition">The ignore condition.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">$"Unknown {nameof(JsonIgnoreCondition)}: '{ignoreCondition}'.</exception>
    public static bool ShouldWriteValue(object? value, object? defaultValue, JsonIgnoreCondition ignoreCondition)
    {
        return ignoreCondition switch
        {
            JsonIgnoreCondition.Never => true,
            JsonIgnoreCondition.Always => false,
            JsonIgnoreCondition.WhenWritingDefault => !Equals(value, defaultValue),
            JsonIgnoreCondition.WhenWritingNull => value is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(ignoreCondition), $"Unknown {nameof(JsonIgnoreCondition)}: '{ignoreCondition}'."),
        };
    }

    /// <inheritdoc/>
    public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Malformed JSON. Expected start of object '{{', but got {reader.TokenType}.");

        Resource resource;
        IDictionary<string, ICollection<Link>>? links = default;
        IDictionary<string, ICollection<Resource>>? embedded = default;
        var state = new ExpandoObject();
        JsonSerializerOptions? optionsWithDynamicConverter = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                resource = state.Any() ? new Resource<object> { State = state } : new Resource();

                if (embedded is not null)
                    resource.Embedded = embedded;

                if (links is not null)
                    resource.Links = links;

                return resource;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            if (propertyName == Constants.EmbeddedPropertyName)
            {
                embedded = JsonSerializer.Deserialize<IDictionary<string, ICollection<Resource>>>(ref reader, options);
            }
            else if (propertyName == Constants.LinksPropertyName)
            {
                links = JsonSerializer.Deserialize<IDictionary<string, ICollection<Link>>>(ref reader, options);
            }
            else if (propertyName is not null)
            {
                optionsWithDynamicConverter ??= AddDynamicConverter(options);

                state.TryAdd(propertyName, (object?)JsonSerializer.Deserialize<dynamic>(ref reader, optionsWithDynamicConverter));
            }
        }

        throw new JsonException();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Resource value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var type = value.GetType();
        if (type.IsGenericType)
        {
            var stateProperty = type.GetProperty(nameof(Resource<object>.State));
            if (stateProperty is not null)
            {
                var state = stateProperty.GetValue(value);
                if (state is not null)
                {
                    WriteState(writer, state, options);
                }
            }
        }

        if (value.Links != null)
        {
            writer.WritePropertyName(Constants.LinksPropertyName);
            JsonSerializer.Serialize(writer, value.Links, options);
        }

        if (value.Embedded != null)
        {
            writer.WritePropertyName(Constants.EmbeddedPropertyName);
            JsonSerializer.Serialize(writer, value.Embedded, options);
        }

        writer.WriteEndObject();
    }

    private static JsonSerializerOptions AddDynamicConverter(JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Add(new DynamicJsonConverter());
        return newOptions;
    }

    private static void WriteState(Utf8JsonWriter writer, object state, JsonSerializerOptions options)
    {
        if (state is null)
            return;

        var type = state.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var originalName = property.Name;
            var name = options.PropertyNamingPolicy?.ConvertName(originalName) ?? originalName;
            var value = property.GetValue(state);
            var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

            if (ShouldWriteProperty(property, value, defaultValue, options.DefaultIgnoreCondition))
            {
                writer.WritePropertyName(name);
                JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
            }
        }
    }

    /// <inheritdoc/>
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        if (type != typeof(Resource))
            return null;

        var typeInfo = JsonTypeInfo.CreateJsonTypeInfo<Resource>(options);

        var resourceProperties = typeof(Resource).GetProperties();

        foreach (var property in resourceProperties)
        {
            var propertyName = ConverterUtils.GetPropertyName(property, options.PropertyNamingPolicy);
            typeInfo.AddJsonPropertyInfo(property, propertyName);
        }

        return typeInfo;
    }
}