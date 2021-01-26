using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Tests
{
    public static class Constants
    {
        public static JsonSerializerOptions DefaultSerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
    }
}