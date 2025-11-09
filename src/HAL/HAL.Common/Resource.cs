using HAL.Common.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace HAL.Common;

/// <summary>
///   <para>   A Resource Object represents a resource.</para>
///   <para>   It has two reserved properties:</para>
///   <para>   (1)  "_links": contains links to other resources.</para>
///   <para>   (2)  "_embedded": contains embedded resources.</para>
/// </summary>
[JsonConverter(typeof(ResourceJsonConverter))]
[Description(
    """
    A Resource Object represents a resource.
    It has two reserved properties:
    (1)  "_links": contains links to other resources.
    (2)  "_embedded": contains embedded resources.
    """)]
public record Resource : IEquatable<Resource>
{
    /// <summary>
    ///   <para>
    ///  The reserved "_embedded" property is OPTIONAL</para>
    ///   <para>   It is an object whose property names are link relation types (as<br />   defined by [RFC5988]) and values are either a Resource Object or an<br />   array of Resource Objects.</para>
    ///   <para>   Embedded Resources MAY be a full, partial, or inconsistent version of<br />   the representation served from the target URI. </para>
    /// </summary>
    /// <value>The embedded.</value>
    [JsonPropertyName(Constants.EmbeddedPropertyName)]
    [Description(
        """
        The reserved "_embedded" property is OPTIONAL
        It is an object whose property names are link relation types (as
        defined by [RFC5988]) and values are either a Resource Object or an
        array of Resource Objects.
        Embedded Resources MAY be a full, partial, or inconsistent version of
        the representation served from the target URI.
        """)]
    public virtual IDictionary<string, ICollection<Resource>>? Embedded { get; set; }

    /// <summary>
    ///   <para>   The reserved "_links" property is OPTIONAL.</para>
    ///   <para>   It is an object whose property names are link relation types (as<br />   defined by [RFC5988]) and values are either a Link Object or an array<br />   of Link Objects.  The subject resource of these links is the Resource<br />   Object of which the containing "_links" object is a property.</para>
    /// </summary>
    /// <value>The links.</value>
    [JsonPropertyName(Constants.LinksPropertyName)]
    [Description(
        """
        The reserved "_links" property is OPTIONAL.
        It is an object whose property names are link relation types (as
        defined by [RFC5988]) and values are either a Link Object or an array
        of Link Objects.  The subject resource of these links is the Resource
        Object of which the containing "_links" object is a property.
        """)]
    public virtual IDictionary<string, ICollection<Link>>? Links { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{ ");
        if (Links is not null)
        {
            sb.Append(Constants.LinksPropertyName);
            sb.Append('[');
            sb.Append(Links.Count);
            sb.Append(']');
        }
        if (Embedded is not null)
        {
            sb.Append(Constants.EmbeddedPropertyName);
            sb.Append('[');
            sb.Append(Embedded.Count);
            sb.Append(']');
        }
        sb.Append(" }");
        return sb.ToString();
    }

    /// <inheritdoc/>
    public virtual bool Equals(Resource? other)
    {
        return other is not null && NestedEquals(Embedded, other.Embedded) && NestedEquals(Links, other.Links);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();

        NestedHash(Embedded, hash);
        NestedHash(Links, hash);

        return hash.ToHashCode();
    }

    private static void NestedHash<TKey, TValue>(IDictionary<TKey, ICollection<TValue>>? dictionary, HashCode hash)
    {
        if (dictionary is not null)
        {
            foreach (var pair in dictionary)
            {
                hash.Add(pair.Key);
                if (pair.Value is not null)
                    foreach (var value in pair.Value)
                    {
                        hash.Add(value);
                    }
            }
        }
    }

    private static bool NestedEquals<TKey, TValue>(IDictionary<TKey, ICollection<TValue>>? dictionary, IDictionary<TKey, ICollection<TValue>>? otherDictionary)
    {
        return
            dictionary == otherDictionary ||
            (
                dictionary is not null &&
                otherDictionary is not null &&
                dictionary.Count == otherDictionary.Count &&
                dictionary.All(p =>
                    otherDictionary.TryGetValue(p.Key, out var otherValue) &&
                    (
                        p.Value == otherValue ||
                        (
                            p.Value is not null &&
                            otherValue is not null &&
                            p.Value.All(e => otherValue.Contains(e))
                        )
                    ))
            );
    }
}