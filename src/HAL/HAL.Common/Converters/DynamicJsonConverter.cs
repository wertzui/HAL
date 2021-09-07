using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// Temp Dynamic Converter
    /// by:tchivs@live.cn
    /// from https://github.com/dotnet/runtime/issues/29690#issuecomment-571969037
    /// </summary>
    public class DynamicJsonConverter : JsonConverter<dynamic>
    {
        /// <inheritdoc/>
        public override dynamic Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long l))
                {
                    return l;
                }

                return reader.GetDouble();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetDateTime(out DateTime datetime))
                {
                    return datetime;
                }

                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using JsonDocument documentV = JsonDocument.ParseValue(ref reader);
                return ReadObject(documentV.RootElement);
            }
            // Use JsonElement as fallback.
            JsonDocument document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone();
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer,
            object value,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException("This is just a temp workaround for deserializing to a dynamic object");
        }

        private object ReadList(JsonElement jsonElement)
        {
            IList<object> list = new List<object>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                list.Add(ReadValue(item));
            }
            return list.Count == 0 ? null : list;
        }

        private object ReadObject(JsonElement jsonElement)
        {
            IDictionary<string, object> expandoObject = new ExpandoObject();
            foreach (var obj in jsonElement.EnumerateObject())
            {
                var k = obj.Name;
                var value = ReadValue(obj.Value);
                expandoObject[k] = value;
            }
            return expandoObject;
        }

        private object ReadValue(JsonElement jsonElement)
        {
            object result;
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    result = ReadObject(jsonElement);
                    break;

                case JsonValueKind.Array:
                    result = ReadList(jsonElement);
                    break;

                case JsonValueKind.String:
                    if (jsonElement.TryGetDateTimeOffset(out var dto))
                        result = dto;
                    else if (jsonElement.TryGetDateTime(out var dt))
                        result = dt;
                    else if (jsonElement.TryGetGuid(out var g))
                        result = g;
                    else
                        result = jsonElement.GetString();
                    break;

                case JsonValueKind.Number:
                    if (jsonElement.TryGetInt64(out var l))
                        result = l;
                    else if (jsonElement.TryGetDouble(out var d))
                        result = d;
                    else
                        result = 0;
                    break;

                case JsonValueKind.True:
                    result = true;
                    break;

                case JsonValueKind.False:
                    result = false;
                    break;

                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    result = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(jsonElement)}.{nameof(jsonElement.ValueKind)}", $"Unsupported {nameof(jsonElement.ValueKind)} '{jsonElement.ValueKind}'.");
            }
            return result;
        }
    }
}