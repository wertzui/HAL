using HAL.Common;
using Microsoft.AspNetCore.Mvc;
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
        /// Creates a resource for a get endpoint. Call this during a get request in your controller
        /// as it will automatically add a "self" link.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <param name="action">The action.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns></returns>
        Resource<T> CreateForGetEndpoint<T>(T state, string? action = "Get", string? controller = null, object? routeValues = null);

        /// <summary>
        /// Creates a resource for the home endpoint which will contain all possible links to all
        /// controller actions that do not have any parameters in your application. Each controller
        /// will become one relation with all the methods as links inside of that relation. It will
        /// also add a curie to point to a documentation about the relations.
        /// </summary>
        /// <param name="curieName">Name of the curie.</param>
        /// <param name="curieUrlTemplate">The curie URL template.</param>
        /// <param name="version">The version of the API. Default is the latest version.</param>
        /// <returns></returns>
        Resource CreateForHomeEndpoint(string curieName, string curieUrlTemplate, ApiVersion? version = null);

        /// <summary>
        /// Creates a resource for the home endpoint which will contain all possible links to all
        /// controller actions that do not have any parameters in your application. Each controller
        /// will become one relation with all the methods as links inside of that relation. It will
        /// also add a curie to point to a documentation about the relations.
        /// </summary>
        /// <param name="state">The state to display on the home endpoint.</param>
        /// <param name="curieName">Name of the curie.</param>
        /// <param name="curieUrlTemplate">The curie URL template.</param>
        /// <param name="version">The version of the API. Default is the latest version.</param>
        /// <returns></returns>
        Resource<TState> CreateForHomeEndpoint<TState>(TState state, string curieName, string curieUrlTemplate, ApiVersion? version = null);

        /// <summary>
        /// Creates a resource for the home endpoint which will contain all possible links to all
        /// controller actions that do not have any parameters in your application. Each controller
        /// will become one relation with all the methods as links inside of that relation. It will
        /// also add a curie to point to a documentation about the relations based on the default
        /// Swagger UI endpoint at /swagger/index.html. For this to work, you must enable deep
        /// linking in your call to app.UseSwaggerUI(...).
        /// </summary>
        /// <param name="curieName">Name of the curie.</param>
        /// <param name="version">The version of the API. Default is the latest version.</param>
        /// <returns></returns>
        Resource CreateForHomeEndpointWithSwaggerUi(string curieName, ApiVersion? version = null);

        /// <summary>
        /// Creates a resource for the home endpoint which will contain all possible links to all
        /// controller actions that do not have any parameters in your application. Each controller
        /// will become one relation with all the methods as links inside of that relation. It will
        /// also add a curie to point to a documentation about the relations based on the default
        /// Swagger UI endpoint at /swagger/index.html. For this to work, you must enable deep
        /// linking in your call to app.UseSwaggerUI(...).
        /// </summary>
        /// <param name="state">The state to display on the home endpoint.</param>
        /// <param name="curieName">Name of the curie.</param>
        /// <param name="version">The version of the API. Default is the latest version.</param>
        /// <returns></returns>
        Resource<TState> CreateForHomeEndpointWithSwaggerUi<TState>(TState state, string curieName, ApiVersion? version = null);

        /// <summary>
        /// Creates a resource for a list endpoint. Call this during a get all request in your
        /// controller as it will automatically add a "self" link. This only works if your get
        /// endpoint to get the full resources looks like this: "MyGetMethod(TId id)".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources.</param>
        /// <param name="keyAccessor">
        /// The key accessor which is used as the name of the collection in the _embedded object. If
        /// you do not specify a constant value, you will have multiple collections in the _embedded object.
        /// </param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="listGetMethod">
        /// The name of the get method for the list endpoint. Default is "GetList".
        /// </param>
        /// <param name="singleGetMethod">
        /// The name of the get method for the get-single endpoint. Default is "Get".
        /// </param>
        /// <returns></returns>
        Resource CreateForListEndpoint<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get");

        /// <summary>
        /// Creates a resource for a list endpoint. Call this during a get all request in your
        /// controller as it will automatically add a "self" link. This only works if your get
        /// endpoint to get the full resources looks like this: "MyGetMethod(TId id)".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources.</param>
        /// <param name="keyAccessor">
        /// The key accessor which is used as the name of the collection in the _embedded object. If
        /// you do not specify a constant value, you will have multiple collections in the _embedded object.
        /// </param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <param name="firstHref">The href to the first page (optional).</param>
        /// <param name="prevHref">The href to the previous page (optional).</param>
        /// <param name="nextHref">The href to the next page (optional).</param>
        /// <param name="lastHref">The href to the last page (optional).</param>
        /// <param name="state">
        /// The state with paging information (optional if the endpoint supports it).
        /// </param>
        /// <param name="controller">The controller.</param>
        /// <param name="listGetMethod">
        /// The name of the get method for the list endpoint. Default is "GetList".
        /// </param>
        /// <param name="singleGetMethod">
        /// The name of the get method for the get-single endpoint. Default is "Get".
        /// </param>
        /// <returns></returns>
        Resource<Page> CreateForListEndpointWithPaging<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string? firstHref = null, string? prevHref = null, string? nextHref = null, string? lastHref = null, Page? state = null, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get");
    }
}