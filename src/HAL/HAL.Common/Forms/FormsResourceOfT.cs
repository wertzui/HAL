using HAL.Common.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HAL.Common.Forms;

/// <summary>
/// <para>A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.</para>
/// <para>In addition this resource also has a state.</para>
/// </summary>
[JsonConverter(typeof(FormsResourceOfTJsonConverterFactory))]
[Description(
    """
    A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.
    In addition this resource also has a state.
    """
    )]
public record FormsResource<TState> : FormsResource, IEquatable<FormsResource<TState>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormsResource"/> class.
    /// </summary>
    /// <param name="templates">The form templates for this resource.</param>
    public FormsResource(IDictionary<string, FormTemplate> templates)
        : base(templates)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormsResource"/> class.
    /// </summary>
    /// <param name="template">The form template for this resource.</param>
    public FormsResource(FormTemplate template)
        : this(new Dictionary<string, FormTemplate>() { { template.Title ?? throw new ArgumentException($"The {nameof(template.Title)} of the {nameof(template)} must not be null.", nameof(template)), template } })
    {
    }

    /// <summary>
    /// Gets or sets the state of the resource.
    /// </summary>
    /// <value>The state.</value>
    [Description("Gets or sets the state of the resource.")]
    public TState? State { get; set; }

    /// <inheritdoc/>
    public virtual bool Equals(FormsResource<TState>? other)
    {
        return other is not null && Equals(State, other.State) && base.Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(State, base.GetHashCode());
    }
}