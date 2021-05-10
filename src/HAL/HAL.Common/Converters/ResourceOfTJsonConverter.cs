using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// A converter that can read and write <see cref="Resource{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the state of the resource.</typeparam>
    /// <seealso cref="JsonConverter{Resource{T}}" />
    public class ResourceJsonConverter<T> : JsonConverter<Resource<T>>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            return
                typeToConvert == typeof(Resource) ||
                typeToConvert == typeof(Resource);
        }

        /// <inheritdoc/>
        public override Resource<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            Resource<T> resource;
            IDictionary<string, ICollection<Link>> links = default;
            IDictionary<string, ICollection<Resource>> embedded = default;
            T state = default;
            var stateType = typeof(T);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    resource = new Resource<T> { State = state };

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
                else
                {
                    var property = stateType.GetProperty(propertyName);
                    if (property == null && options.PropertyNameCaseInsensitive)
                    {
                        property = stateType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    }

                    if (property != null)
                    {
                        if (state is null)
                            state = Activator.CreateInstance<T>();

                        property.SetValue(state, JsonSerializer.Deserialize(ref reader, property.PropertyType, options));
                    }
                }
            }

            throw new JsonException();
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Resource<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            WriteState(writer, value.State, options);

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

        private void WriteState(Utf8JsonWriter writer, T state, JsonSerializerOptions options)
        {
            if (state is null)
                return;

            var type = typeof(T);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var originalName = property.Name;
                var name = options.PropertyNamingPolicy?.ConvertName(originalName) ?? originalName;
                var value = property.GetValue(state);
                var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

                if (ResourceJsonConverter.ShouldWriteValue(value, defaultValue, options.DefaultIgnoreCondition))
                {
                    writer.WritePropertyName(name);
                    JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
                }
            }
        }
    }
}