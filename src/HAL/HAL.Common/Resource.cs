using HAL.Common.Converters;
using System;
using System.Collections.Generic;
using System.Text;
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
    public class Resource : IEquatable<Resource>
    {
        /// <summary>
        ///   <para>
        ///  The reserved "_embedded" property is OPTIONAL</para>
        ///   <para>   It is an object whose property names are link relation types (as<br />   defined by [RFC5988]) and values are either a Resource Object or an<br />   array of Resource Objects.</para>
        ///   <para>   Embedded Resources MAY be a full, partial, or inconsistent version of<br />   the representation served from the target URI. </para>
        /// </summary>
        /// <value>The embedded.</value>
        [JsonPropertyName(Constants.EmbeddedPropertyName)]
        public virtual IDictionary<string, ICollection<Resource>> Embedded { get; set; }

        /// <summary>
        ///   <para>   The reserved "_links" property is OPTIONAL.</para>
        ///   <para>   It is an object whose property names are link relation types (as<br />   defined by [RFC5988]) and values are either a Link Object or an array<br />   of Link Objects.  The subject resource of these links is the Resource<br />   Object of which the containing "_links" object is a property.</para>
        /// </summary>
        /// <value>The links.</value>
        [JsonPropertyName(Constants.LinksPropertyName)]
        public virtual IDictionary<string, ICollection<Link>> Links { get; set; }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Resource left, Resource right)
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
        public static bool operator ==(Resource left, Resource right)
        {
            return EqualityComparer<Resource>.Default.Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as Resource);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Resource other)
        {
            return other != null &&
                   DictionaryEqual(Embedded, other.Embedded) &&
                   DictionaryEqual(Links, other.Links);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Embedded, Links);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{ ");
            if (Links is not null)
            {
                sb.Append(Constants.LinksPropertyName);
                sb.Append("[");
                sb.Append(Links.Count);
                sb.Append("]");
            }
            if (Embedded is not null)
            {
                sb.Append(Constants.EmbeddedPropertyName);
                sb.Append("[");
                sb.Append(Embedded.Count);
                sb.Append("]");
            }
            sb.Append(" }");
            return sb.ToString();
        }

        private static bool DictionaryEqual<T>(IDictionary<string, ICollection<T>> oldDict, IDictionary<string, ICollection<T>> newDict)
        {
            // both are null
            if (oldDict is null && newDict is null)
                return true;

            // only one is null
            if (oldDict is null || newDict is null)
                return false;

            // Simple check, are the counts the same?
            if (!oldDict.Count.Equals(newDict.Count))
                return false;

            // iterate through all the keys in oldDict and
            // verify whether the key exists in the newDict
            foreach (string key in oldDict.Keys)
            {
                if (newDict.Keys.Contains(key))
                {
                    // iterate through each value for the current key in oldDict and
                    // verify whether or not it exists for the current key in the newDict
                    foreach (T value in oldDict[key])
                    {
                        if (!newDict[key].Contains(value))
                            return false;
                    }
                }
                else { return false; }
            }

            return true;
        }
    }
}