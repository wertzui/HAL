using HAL.Common.Abstractions;
using HAL.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAL.Common
{
    /// <summary>
    ///   <para>   A Resource Object represents a resource.</para>
    ///   <para>   It has two reserved properties:</para>
    ///   <para>   (1)  "_links": contains links to other resources.</para>
    ///   <para>   (2)  "_embedded": contains embedded resources.</para>
    /// </summary>
    [JsonConverter(typeof(ResourceJsonConverter))]
    public class Resource : IResource
    {
        /// <summary>
        ///   <para>
        ///  The reserved "_embedded" property is OPTIONAL</para>
        ///   <para>   It is an object whose property names are link relation types (as<br />   defined by [RFC5988]) and values are either a Resource Object or an<br />   array of Resource Objects.</para>
        ///   <para>   Embedded Resources MAY be a full, partial, or inconsistent version of<br />   the representation served from the target URI. </para>
        /// </summary>
        /// <value>The embedded.</value>
        [JsonPropertyName(Constants.EmbeddedPropertyName)]
        public IDictionary<string, ICollection<IResource>> Embedded { get; set; }

        /// <summary>
        ///   <para>   The reserved "_links" property is OPTIONAL.</para>
        ///   <para>   It is an object whose property names are link relation types (as<br />   defined by [RFC5988]) and values are either a Link Object or an array<br />   of Link Objects.  The subject resource of these links is the Resource<br />   Object of which the containing "_links" object is a property.</para>
        /// </summary>
        /// <value>The links.</value>
        [JsonPropertyName(Constants.LinksPropertyName)]
        public IDictionary<string, ICollection<ILink>> Links { get; set; }
    }
}