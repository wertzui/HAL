using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HAL.Common;

/// <summary>
/// A Link Object represents a hyperlink from the containing resource to a URI.
/// </summary>
[Description("A Link Object represents a hyperlink from the containing resource to a URI.")]
public record Link
{
    /// <summary>
    ///   <para>The "deprecation" property is OPTIONAL.</para>
    ///   <para>Its presence indicates that the link is to be deprecated (i.e. removed) at a future date.  Its value is a URL that SHOULD provide further information about the deprecation.</para>
    ///   <para>A client SHOULD provide some notification (for example, by logging a warning message) whenever it traverses over a link that has this property.  The notification SHOULD include the deprecation property's value so that a client maintainer can easily find information about the deprecation.</para>
    /// </summary>
    [Description(
        """
        The "deprecation" property is OPTIONAL.
        Its presence indicates that the link is to be deprecated (i.e. removed) at a future date.  Its value is a URL that SHOULD provide further information about the deprecation.
        A client SHOULD provide some notification (for example, by logging a warning message) whenever it traverses over a link that has this property.  The notification SHOULD include the deprecation property's value so that a client maintainer can easily find information about the deprecation.
        """
    )]
    public virtual string? Deprecation { get; set; }

    /// <summary>
    ///   <para>The "href" property is REQUIRED.</para>
    ///   <para>Its value is either a URI [RFC3986] or a URI Template [RFC6570].</para>
    ///   <para>If the value is a URI Template then the Link Object SHOULD have a "templated" attribute whose value is true.</para>
    /// </summary>
    [Required]
    [Description(
        """
        The "href" property is REQUIRED.
        Its value is either a URI [RFC3986] or a URI Template [RFC6570].
        If the value is a URI Template then the Link Object SHOULD have a "templated" attribute whose value is true.
        """
    )]
    public virtual string Href { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="Link"/> class.
    /// </summary>
    /// <param name="href">The URI to which this link points.</param>
    /// <exception cref="ArgumentException">href cannot be null or whitespace.</exception>
    public Link(string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        Href = href;
    }

    /// <summary>
    ///   <para>The "hreflang" property is OPTIONAL.</para>
    ///   <para>Its value is a string and is intended for indicating the language of the target resource (as defined by [RFC5988]).</para>
    /// </summary>
    [Description(
        """
        The "hreflang" property is OPTIONAL.
        Its value is a string and is intended for indicating the language of the target resource (as defined by [RFC5988]).
        """
    )]
    public virtual string? Hreflang { get; set; }

    /// <summary>
    ///   <para>The "name" property is OPTIONAL.</para>
    ///   <para>Its value MAY be used as a secondary key for selecting Link Objects which share the same relation type.</para>
    /// </summary>
    [Description(
    """
        The "name" property is OPTIONAL.
        Its value MAY be used as a secondary key for selecting Link Objects which share the same relation type.
        """
)]
    public virtual string? Name { get; set; }

    /// <summary>
    ///   <para>The "profile" property is OPTIONAL.</para>
    ///   <para>Its value is a string which is a URI that hints about the profile (as defined by [I-D.wilde-profile-link]) of the target resource.</para>
    /// </summary>
    [Description(
        """
        The "profile" property is OPTIONAL.
        Its value is a string which is a URI that hints about the profile (as defined by [I-D.wilde-profile-link]) of the target resource.
        """
    )]
    public virtual string? Profile { get; set; }

    /// <summary>
    ///   <para>The "templated" property is OPTIONAL.</para>
    ///   <para>Its value is boolean and SHOULD be true when the Link Object's "href" property is a URI Template.</para>
    ///   <para>Its value SHOULD be considered false if it is undefined or any other value than true.</para>
    /// </summary>
    [Description(
        """
        The "templated" property is OPTIONAL.
        Its value is boolean and SHOULD be true when the Link Object's "href" property is a URI Template.
        Its value SHOULD be considered false if it is undefined or any other value than true.
        """
    )]
    public virtual bool Templated { get; set; }

    /// <summary>
    ///   <para>The "title" property is OPTIONAL.</para>
    ///   <para>Its value is a string and is intended for labeling the link with a human-readable identifier (as defined by [RFC5988]).</para>
    /// </summary>
    [Description(
        """
        The "title" property is OPTIONAL.
        Its value is a string and is intended for labeling the link with a human-readable identifier (as defined by [RFC5988]).
        """
    )]
    public virtual string? Title { get; set; }

    /// <summary>
    ///   <para>The "type" property is OPTIONAL.</para>
    ///   <para>Its value is a string used as a hint to indicate the media type expected when dereferencing the target resource.</para>
    /// </summary>
    [Description(
        """
        The "type" property is OPTIONAL.
        Its value is a string used as a hint to indicate the media type expected when dereferencing the target resource.
        """
    )]
    public virtual string? Type { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name is not null ?
            "{ " + Name + ", " + Href + "}" :
            "{ " + Href + "}";
    }
}