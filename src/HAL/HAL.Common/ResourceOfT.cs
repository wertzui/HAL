using HAL.Common.Converters;
using System;
using System.Text.Json.Serialization;

namespace HAL.Common
{
    /// <summary>
    /// <para>A Resource Object represents a resource.</para>
    /// <para>It has two reserved properties:</para>
    /// <para>(1) "_links": contains links to other resources.</para>
    /// <para>(2) "_embedded": contains embedded resources.</para>
    /// <para>In addition this resource also has a state.</para>
    /// </summary>
    [JsonConverter(typeof(ResourceOfTJsonConverterFactory))]
    public record Resource<TState> : Resource, IEquatable<Resource<TState>>
    {
        /// <summary>
        /// Gets or sets the state of the resource.
        /// </summary>
        /// <value>The state.</value>
        public TState? State { get; set; }

        /// <inheritdoc/>
        public virtual bool Equals(Resource<TState>? other)
        {
            return other is not null && Equals(State, other.State) && base.Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(State, base.GetHashCode());
        }
    }
}