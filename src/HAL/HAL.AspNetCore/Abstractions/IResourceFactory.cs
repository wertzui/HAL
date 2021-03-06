﻿using HAL.Common;
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
        Resource Create();

        /// <summary>
        /// Creates a resource with the specified state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        Resource<T> Create<T>(T state);

        /// <summary>
        /// Creates a resource for a get endpoint.
        /// Call this during a get request in your controller as it will automatically add a "self" link.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        Resource<T> CreateForGetEndpoint<T>(T state);

        /// <summary>
        /// Creates a resource for the home endpoint which will contain all possible links to all controller actions that do not have any parameters in your application.
        /// Each controller will become one relation with all the methods as links inside of that relation.
        /// It will also add a curie to point to a documentation about the relations.
        /// </summary>
        /// <returns></returns>
        Resource CreateForHomeEndpoint(string curieName, string curieUrlTemplate);

        /// <summary>
        /// Creates a resource for the home endpoint which will contain all possible links to all controller actions that do not have any parameters in your application.
        /// Each controller will become one relation with all the methods as links inside of that relation.
        /// It will also add a curie to point to a documentation about the relations based on the default Swagger UI endpoint at /swagger/index.html.
        /// For this to work, you must enable deep linking in your call to app.UseSwaggerUI(...).
        /// </summary>
        /// <param name="curieName">Name of the curie.</param>
        /// <returns></returns>
        Resource CreateForHomeEndpointWithSwaggerUi(string curieName);

        /// <summary>
        /// Creates a resource for a list endpoint.
        /// Call this during a get all request in your controller as it will automatically add a "self" link.
        /// This only works if your get endpoint to get the full resources looks like this: "MyGetMethod(TId id)".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources.</param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <param name="getMethod">The name of the get method. Default is "Get".</param>
        /// <returns></returns>
        Resource CreateForListEndpoint<T, TId>(IEnumerable<T> resources, Func<T, TId> idAccessor, string getMethod = "Get");
    }
}