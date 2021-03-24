using HAL.Common.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAL.Common
{
    /// <summary>
    ///   <para>   A Resource Object represents a resource.</para>
    ///   <para>   It has two reserved properties:</para>
    ///   <para>   (1)  "_links": contains links to other resources.</para>
    ///   <para>   (2)  "_embedded": contains embedded resources.</para>
    ///   <para>   In addition this resource also has a state.</para>
    /// </summary>
    [JsonConverter(typeof(ResourceOfTJsonConverterFactory))]
    public class Resource<TState> : Resource, IEquatable<Resource<TState>>
    {
        /// <summary>
        /// Gets or sets the state of the resource.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public TState State { get; set; }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Resource<TState> left, Resource<TState> right)
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
        public static bool operator ==(Resource<TState> left, Resource<TState> right)
        {
            return EqualityComparer<Resource<TState>>.Default.Equals(left, right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Resource<TState>);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Resource<TState> other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<TState>.Default.Equals(State, other.State);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), State);
        }
    }
}