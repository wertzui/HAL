using HAL.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// A converter that can read and write <see cref="Resource"/>.
    /// </summary>
    /// <seealso cref="JsonConverter{Resource}" />
    public class ResourceJsonConverter : JsonConverter<Resource>
    {
        /// <inheritdoc/>
        public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var result = new Resource<dynamic>();
            var state = new ExpandoObject();
            dynamic dynamicState = state;
            var hasState = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (hasState)
                        result.State = state;

                    return result;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                if (propertyName == Constants.EmbeddedPropertyName)
                {
                    result.Embedded = JsonSerializer.Deserialize<IDictionary<string, ICollection<IResource>>>(ref reader, options);
                }
                else if (propertyName == Constants.LinksPropertyName)
                {
                    result.Links = JsonSerializer.Deserialize<IDictionary<string, ICollection<ILink>>>(ref reader, options);
                }
                else
                {
                    hasState = true;
                    Type propertyType = dynamicState[propertyName].GetType();
                    dynamicState[propertyName] = JsonSerializer.Deserialize(ref reader, propertyType, options);
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
                var state = stateProperty.GetValue(value);
                WriteState(writer, state, options);
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

        private void WriteState(Utf8JsonWriter writer, object state, JsonSerializerOptions options)
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

                if (!object.Equals(value, defaultValue) || options.DefaultIgnoreCondition == JsonIgnoreCondition.Never)
                {
                    writer.WritePropertyName(name);
                    JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
                }
            }
        }
    }
}