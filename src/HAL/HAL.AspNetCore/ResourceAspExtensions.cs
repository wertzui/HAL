using HAL.AspNetCore.Abstractions;

namespace HAL.Common
{
    /// <summary>
    /// Contains extension methods for <see cref="Resource"/> which also utilize services from ASP.Net Core.
    /// </summary>
    public static class ResourceAspExtensions
    {
        /// <summary>
        /// Adds the "self" link to the resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="linkFactory">The link factory.</param>
        /// <returns></returns>
        public static TResource AddSelfLink<TResource>(this TResource resource, ILinkFactory linkFactory)
            where TResource : Resource
        {
            return linkFactory.AddSelfLinkTo(resource);
        }
        /// <summary>
        /// Adds the "self" link to the resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="linkFactory">The link factory.</param>
        /// <param name="name">The name of the curie. You should prefix all your self defined rels in other links with name: so the user knows where to get information on that relation.</param>
        /// <returns></returns>
        public static TResource AddSwaggerUiCurieLink<TResource>(this TResource resource, ILinkFactory linkFactory, string name)
            where TResource : Resource
        {
            return linkFactory.AddSwaggerUiCurieLinkTo(resource, name);
        }
    }
}