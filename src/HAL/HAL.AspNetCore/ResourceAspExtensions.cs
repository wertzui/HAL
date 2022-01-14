using HAL.AspNetCore.Abstractions;
using System;

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
        /// <param name="action">The action.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns></returns>
        public static TResource AddSelfLink<TResource>(this TResource resource, ILinkFactory linkFactory, string? action = null, string? controller = null, object? routeValues = null)
            where TResource : Resource
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (linkFactory is null)
                throw new ArgumentNullException(nameof(linkFactory));

            return linkFactory.AddSelfLinkTo(resource, action, controller, routeValues);
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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (linkFactory is null)
                throw new ArgumentNullException(nameof(linkFactory));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            return linkFactory.AddSwaggerUiCurieLinkTo(resource, name);
        }
    }
}