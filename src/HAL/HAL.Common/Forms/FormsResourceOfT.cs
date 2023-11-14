using HAL.Common.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAL.Common.Forms;

/// <summary>
/// <para>
/// A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.
/// </para>
/// <para>In addition this resource also has a state.</para>
/// </summary>
[JsonConverter(typeof(ResourceOfTJsonConverterFactory))]
public record FormsResource<T> : Resource<T>
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
    /// <param name="templates">The form templates for this resource.</param>
    public FormsResource(IDictionary<string, FormTemplate> templates)
    {
        Templates = templates;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormsResource"/> class.
    /// </summary>
    /// <param name="template">The form template for this resource.</param>
    public FormsResource(FormTemplate template)
        : this(new Dictionary<string, FormTemplate>() { { template.Title ?? throw new ArgumentException($"The {nameof(template.Title)} of the {nameof(template)} must not be null.", nameof(template)), template } })
    {
    }
}