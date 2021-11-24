using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAL.Common.Forms
{
    /// <summary>
    /// A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.
    /// </summary>
    public class FormsResource : Resource
    {
        /// <summary>
        /// The property which stores the templates for this document.
        /// </summary>
        [JsonPropertyName(Constants.FormTemplatesPropertyName)]
        public IDictionary<string, FormTemplate> Templates { get; set; }
    }
}