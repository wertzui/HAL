using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HAL.Common
{
    /// <summary>
    /// A Link Object represents a hyperlink from the containing resource to a URI.
    /// </summary>
    public class Link : IEquatable<Link>
    {
        /// <summary>
        ///   <para>   The "deprecation" property is OPTIONAL.</para>
        ///   <para>   Its presence indicates that the link is to be deprecated (i.e.<br />   removed) at a future date.  Its value is a URL that SHOULD provide<br />   further information about the deprecation.</para>
        ///   <para>   A client SHOULD provide some notification (for example, by logging a<br />   warning message) whenever it traverses over a link that has this<br />   property.  The notification SHOULD include the deprecation property's<br />   value so that a client maintainer can easily find information about<br />   the deprecation.</para>
        /// </summary>
        public virtual string Deprecation { get; set; }

        /// <summary>
        ///   <para>   The "href" property is REQUIRED.</para>
        ///   <para>   Its value is either a URI [RFC3986] or a URI Template [RFC6570].</para>
        ///   <para>   If the value is a URI Template then the Link Object SHOULD have a<br />   "templated" attribute whose value is true.</para>
        /// </summary>
        [Required]
        public virtual string Href { get; set; }

        /// <summary>
        ///   <para>   The "hreflang" property is OPTIONAL.</para>
        ///   <para>   Its value is a string and is intended for indicating the language of<br />   the target resource (as defined by [RFC5988]).</para>
        /// </summary>
        public virtual string Hreflang { get; set; }

        /// <summary>
        ///   <para>   The "name" property is OPTIONAL.</para>
        ///   <para>   Its value MAY be used as a secondary key for selecting Link Objects<br />   which share the same relation type.</para>
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///   <para>   The "profile" property is OPTIONAL.</para>
        ///   <para>   Its value is a string which is a URI that hints about the profile (as<br />   defined by [I-D.wilde-profile-link]) of the target resource.</para>
        /// </summary>
        public virtual string Profile { get; set; }

        /// <summary>
        ///   <para>   The "templated" property is OPTIONAL.</para>
        ///   <para>   Its value is boolean and SHOULD be true when the Link Object's "href"<br />   property is a URI Template.</para>
        ///   <para>   Its value SHOULD be considered false if it is undefined or any other<br />   value than true.</para>
        /// </summary>
        public virtual bool Templated { get; set; }

        /// <summary>
        ///   <para>   The "title" property is OPTIONAL.</para>
        ///   <para>   Its value is a string and is intended for labelling the link with a<br />   human-readable identifier (as defined by [RFC5988]).</para>
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        ///   <para>   The "type" property is OPTIONAL.</para>
        ///   <para>   Its value is a string used as a hint to indicate the media type<br />   expected when dereferencing the target resource.</para>
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Link left, Link right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Link left, Link right)
        {
            return EqualityComparer<Link>.Default.Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as Link);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Link other)
        {
            return other != null &&
                   Deprecation == other.Deprecation &&
                   Href == other.Href &&
                   Hreflang == other.Hreflang &&
                   Name == other.Name &&
                   Profile == other.Profile &&
                   Templated == other.Templated &&
                   Title == other.Title &&
                   Type == other.Type;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Deprecation, Href, Hreflang, Name, Profile, Templated, Title, Type);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name is not null ?
                "{ " + Name + ", " + Href + "}" :
                "{ " + Href + "}";
        }
    }
}