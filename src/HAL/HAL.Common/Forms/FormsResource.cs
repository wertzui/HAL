using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAL.Common.Forms;

/// <summary>
/// A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.
/// </summary>
public record FormsResource : Resource
{
    /// <summary>
    /// The _templates collection describes the available state transition details including the
    /// HTTP method, message content-type, and arguments for the transition. This is a REQUIRED
    /// element. If the HAL-FORMS document does not contain this element or the contents are
    /// unrecognized or unparseable, the HAL-FORMS document SHOULD be ignored. The _templates
    /// element contains a dictionary collection of template objects.A valid HAL-FORMS document
    /// has at least one entry in the _templates dictionary collection. Each template contains
    /// the following possible properties:
    /// </summary>
    [JsonPropertyName(Constants.FormTemplatesPropertyName)]
    public IDictionary<string, FormTemplate> Templates { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormsResource"/> class.
    /// </summary>
    /// <param name="templates"></param>
    public FormsResource(IDictionary<string, FormTemplate> templates)
    {
        Templates = templates;
    }
}