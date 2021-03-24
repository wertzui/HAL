using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;

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
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="resources">The resources.</param>
        /// <param name="idAccessor">The identifier accessor.</param>
        /// <param name="oDataQueryOptions">The o data query options.</param>
        /// <param name="maxTop">The maximum top.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="getMethod">The get method.</param>
        /// <returns></returns>
        Resource CreateForOdataListEndpointUsingSkipTopPaging<TDto, TEntity, TId>(System.Collections.Generic.IEnumerable<TDto> resources, System.Func<TDto, TId> idAccessor, ODataQueryOptions<TEntity> oDataQueryOptions, long maxTop = 50, long? totalCount = null, string getMethod = "Get");
    }
}