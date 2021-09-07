using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore.OData
{
    /// <summary>
    /// A factory to create resources based on OData queries.
    /// </summary>
    /// <seealso cref="ResourceFactory" />
    /// <seealso cref="IODataResourceFactory" />
    public class ODataResourceFactory : ResourceFactory, IODataResourceFactory
    {
        private readonly IODataQueryFactory _oDataQueryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceFactory"/> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        /// <param name="oDataQueryFactory">The OData query factory.</param>
        public ODataResourceFactory(ILinkFactory linkFactory, IODataQueryFactory oDataQueryFactory) : base(linkFactory)
        {
            _oDataQueryFactory = oDataQueryFactory ?? throw new ArgumentNullException(nameof(oDataQueryFactory));
        }

        /// <inheritdoc/>
        public Resource<Page> CreateForOdataListEndpointUsingSkipTopPaging<TDto, TEntity, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, ODataQueryOptions<TEntity> oDataQueryOptions, long maxTop = 50, long? totalCount = null, string getMethod = "Get")
        {
            var rawValues = oDataQueryOptions.RawValues;

            (long skip, long top) = _oDataQueryFactory.GetSkipAndTop(maxTop, rawValues);

            var page = new Page { CurrentPage = (skip == 0 ? 0 : skip / top) + 1, TotalPages = (long)Math.Ceiling((double)totalCount / top) };

            var links = GetListNavigation(resources, rawValues, skip, top, totalCount);

            var resource = CreateForOdataListEndpointUsingSkipTopPaging(resources, keyAccessor, idAccessor, links, page, getMethod);

            return resource;
        }

        /// <inheritdoc/>
        public Resource<Page> CreateForOdataListEndpointUsingSkipTopPaging<TDto, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, IPageLinks links, Page page, string getMethod = "Get")
            => CreateForListEndpointWithPaging(resources, keyAccessor, idAccessor, links.FirstHref, links.PrevHref, links.NextHref, links.LastHref, page, getMethod);

        private IPageLinks GetListNavigation<TDto>(IEnumerable<TDto> resources, ODataRawQueryOptions rawValues, long skip, long top, long? totalCount)
            => _oDataQueryFactory.GetListNavigation(resources, rawValues, LinkFactory.Create().Href, skip, top, totalCount);
    }
}