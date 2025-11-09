using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters
{
    /// <summary>
    /// Provides utility methods for working with property metadata and naming policies during serialization.
    /// </summary>
    public class ConverterUtils
    {
        /// <summary>
        /// Gets the name of a property while respecting the <see cref="JsonNamingPolicy"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="propertyNamingPolicy">The naming policy.</param>
        /// <returns>The name of the property as it should appear in the serialized JSON.</returns>
        public static string GetPropertyName(PropertyInfo property, JsonNamingPolicy? propertyNamingPolicy)
        {
            var attribute = property.GetCustomAttribute<JsonPropertyNameAttribute>(true);
            if (attribute is not null)
                return attribute.Name;

            if (propertyNamingPolicy is not null)
                return propertyNamingPolicy.ConvertName(property.Name);

            return property.Name;
        }
    }
}
