using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore.OData.Abstractions
{
    /// <summary>
    /// A factory to create resources based on OData queries.
    /// </summary>
    /// <seealso cref="HAL.AspNetCore.Abstractions.IResourceFactory" />
    public interface IODataResourceFactory : IResourceFactory
    {
        /// <summary>
        /// Creates a resource for a list endpoint using OData skip and top logic.
        /// </summary>
        /// <typeparam name="TDto">The type of the dto.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources which will become the embedded list.</param>
        /// <param name="keyAccessor">The key accessor which is used as the name of the collection in the _embedded object. If you do not specify a constant value, you will have multiple collections in the _embedded object.</param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <param name="oDataQueryOptions">The o data query options.</param>
        /// <param name="maxTop">The maximum top.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="getMethod">The get method.</param>
        /// <returns></returns>
        Resource<Page> CreateForOdataListEndpointUsingSkipTopPaging<TDto, TEntity, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, ODataQueryOptions<TEntity> oDataQueryOptions, long maxTop = 50, long? totalCount = null, string getMethod = "Get");

        /// <summary>
        /// Creates a resource for a list endpoint using OData skip and top logic.
        /// </summary>
        /// <typeparam name="TDto">The type of the dto.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources which will become the embedded list.</param>
        /// <param name="keyAccessor">The key accessor which is used as the name of the collection in the _embedded object. If you do not specify a constant value, you will have multiple collections in the _embedded object.</param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <param name="links">The links from this page.</param>
        /// <param name="page">The page itself.</param>
        /// <param name="getMethod">The get method.</param>
        /// <returns></returns>
        Resource<Page> CreateForOdataListEndpointUsingSkipTopPaging<TDto, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, IPageLinks links, Page page, string getMethod = "Get");
    }
}