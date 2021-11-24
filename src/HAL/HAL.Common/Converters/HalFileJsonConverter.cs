using HAL.Common.Binary;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// A converter that can serialize and deserialize files to and from URIs.
    /// </summary>
    public class HalFileJsonConverter : JsonConverter<HalFile>
    {
        /// <inheritdoc/>
        public override HalFile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new HalFile(reader.GetString());

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, HalFile value, JsonSerializerOptions options) => writer.WriteStringValue(value.Uri.ToString());
    }
}
