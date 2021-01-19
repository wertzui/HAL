using HAL.Common.Abstractions;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore.Abstractions
{
    /// <summary>
    /// A factory to create resources.
    /// </summary>
    public interface IResourceFactory
    {
        /// <summary>
        /// Creates an empty resource.
        /// </summary>
        /// <returns></returns>
        IResource Create();

        /// <summary>
        /// Creates a resource with the specified state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        IResource<T> Create<T>(T state);

        /// <summary>
        /// Creates a resource for a get endpoint.
        /// Call this during a get request in your controller as it will automatically add a "self" link.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        IResource<T> CreateForGetEndpoint<T>(T state);

        /// <summary>
        /// Creates a resource for a list endpoint.
        /// Call this during a get all request in your controller as it will automatically add a "self" link.
        /// This only works if your get endpoint to get the full resources looks like this: "Get(TId id)".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources.</param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <returns></returns>
        IResource CreateForListEndpoint<T, TId>(IEnumerable<T> resources, Func<T, TId> idAccessor);
    }
}