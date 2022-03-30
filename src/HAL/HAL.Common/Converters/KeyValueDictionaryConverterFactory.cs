using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// A factory to create <see cref="KeyValueDictionaryConverter{TValue}"/>s.
    /// Declare your DTO properties with a <see cref="JsonConverterAttribute"/> that uses this factory, if they are an <see cref="IDictionary{TKey, TValue}"/> and TKey is a string.
    /// This ensures that a HAL-Forms resource can be saved.
    /// </summary>
    public class KeyValueDictionaryConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                typeToConvert.GetGenericTypeDefinition().IsAssignableTo(typeof(IDictionary<,>)) &&
                typeToConvert.GetGenericArguments().Length == 2 &&
                typeToConvert.GetGenericArguments()[0] == typeof(string);
        }

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var genericArguments = typeToConvert.GetGenericArguments();
            var tValue = genericArguments[1];
            var tConverter = typeof(KeyValueDictionaryConverter<>);
            var tConverterGeneric = tConverter.MakeGenericType(tValue);
            return Activator.CreateInstance(tConverterGeneric) as JsonConverter;
        }
    }
}
