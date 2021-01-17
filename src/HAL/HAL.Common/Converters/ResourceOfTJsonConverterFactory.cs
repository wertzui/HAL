using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// A factory to instantiate a <see cref="ResourceJsonConverter{T}"/> with the correct type.
    /// </summary>
    /// <seealso cref="JsonConverterFactory" />
    public class ResourceOfTJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            var generic = typeToConvert.GetGenericTypeDefinition();
            if (generic == typeof(Resource<>))
                return true;

            return false;
        }

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var stateType = typeToConvert.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(ResourceJsonConverter<>).MakeGenericType(stateType));

            return converter;
        }
    }
}