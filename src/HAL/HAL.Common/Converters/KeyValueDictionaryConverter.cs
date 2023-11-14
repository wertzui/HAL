using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters;

/// <summary>
/// In addition to "normal" dictionary de-/serialization, this converter can also deserialize dictionaries from JSON that looks like [{"key":"foo", "value": 1}] so is if it was an <see cref="IEnumerable{KeyValuePair{string, TValue}}"/>.
/// Declare your DTO properties with a <see cref="JsonConverterAttribute"/> that uses this factory, if they are an <see cref="IDictionary{TKey, TValue}"/> and TKey is a string.
/// This ensures that a HAL-Forms resource can be saved.
/// </summary>
/// <typeparam name="TValue">The value type of the dictionary.</typeparam>
public class KeyValueDictionaryConverter<TValue> : JsonConverter<IDictionary<string, TValue>>
{
    /// <inheritdoc/>
    public override IDictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var collection = JsonSerializer.Deserialize<IEnumerable<KeyValuePair<string, TValue>>>(ref reader, options);
            if (collection is null)
                return null;
            var dictionary = new Dictionary<string, TValue>(collection);
            return dictionary;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var dictionary = new Dictionary<string, TValue>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var key = reader.GetString();
                if (key is null)
                    throw new JsonException("Cannot use null as Dictionary key");
                var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
                dictionary[key] = value;
            }
            return dictionary;
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IDictionary<string, TValue> value, JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Remove(this);
        JsonSerializer.Serialize(writer, value, newOptions);
    }
}