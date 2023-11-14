using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Tests;

public static class Constants
{
    static Constants()
    {
        DefaultSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
        DefaultSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static JsonSerializerOptions DefaultSerializerOptions { get; }
}