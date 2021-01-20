using HAL.Common.Converters;
using System.Text.Json.Serialization;

namespace HAL.Common.Abstractions
{
    /// <summary>
    ///   <para>   A Resource Object represents a resource.</para>
    ///   <para>   It has two reserved properties:</para>
    ///   <para>   (1)  "_links": contains links to other resources.</para>
    ///   <para>   (2)  "_embedded": contains embedded resources.</para>
    ///   <para>   In addition this resource also has a state.</para>
    /// </summary>
    [JsonInterfaceConverter(typeof(ResourceOfTJsonConverterFactory))]
    public interface IResource<TState> : IResource
    {
        /// <summary>
        /// Gets or sets the state of the resource.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        TState State { get; set; }
    }
}