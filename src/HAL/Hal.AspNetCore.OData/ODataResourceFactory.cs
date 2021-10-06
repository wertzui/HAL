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
        public Resource<Page> CreateForOdataListEndpointUsingSkipTopPaging<TDto, TEntity, TKey, TId>(
            IEnumerable<TDto> resources,
            Func<TDto, TKey> keyAccessor,
            Func<TDto, TId> idAccessor,
            ODataQueryOptions<TEntity> oDataQueryOptions,
            long maxTop = 50,
            long? totalCount = null,
            string controller = null,
            string listGetMethod = "GetList",
            string singleGetMethod = "Get")
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            if (keyAccessor is null)
                throw new ArgumentNullException(nameof(keyAccessor));

            if (idAccessor is null)
                throw new ArgumentNullException(nameof(idAccessor));

            if (oDataQueryOptions is null)
                throw new ArgumentNullException(nameof(oDataQueryOptions));

            if (maxTop < 1)
                throw new ArgumentOutOfRangeException(nameof(maxTop), $"'{nameof(maxTop)}' cannot be less than 1, but is {maxTop}.");

            if (totalCount.GetValueOrDefault() < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCount), $"'{nameof(totalCount)}' cannot be less than 0, but is {totalCount}.");

            if (string.IsNullOrWhiteSpace(listGetMethod))
                throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or whitespace.", nameof(listGetMethod));

            if (string.IsNullOrWhiteSpace(singleGetMethod))
                throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or whitespace.", nameof(singleGetMethod));

            var rawValues = oDataQueryOptions.RawValues;

            (long skip, long top) = _oDataQueryFactory.GetSkipAndTop(maxTop, rawValues);

            var currentPage = (skip == 0 ? 0 : skip / top) + 1;
            var totalPages = totalCount.HasValue ? (long)Math.Ceiling((double)totalCount / top) : default;
            var page = new Page { CurrentPage = currentPage, TotalPages = totalPages };

            var links = GetListNavigation(resources, rawValues, skip, top, totalCount);

            var resource = CreateForOdataListEndpointUsingSkipTopPaging(resources, keyAccessor, idAccessor, links, page, controller, listGetMethod, singleGetMethod);

            return resource;
        }

        /// <inheritdoc/>
        public Resource<Page> CreateForOdataListEndpointUsingSkipTopPaging<TDto, TKey, TId>(
            IEnumerable<TDto> resources,
            Func<TDto, TKey> keyAccessor,
            Func<TDto, TId> idAccessor,
            IPageLinks links,
            Page page,
            string controller = null,
            string listGetMethod = "GetList",
            string singleGetMethod = "Get")
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            if (keyAccessor is null)
                throw new ArgumentNullException(nameof(keyAccessor));

            if (idAccessor is null)
                throw new ArgumentNullException(nameof(idAccessor));

            if (links is null)
                throw new ArgumentNullException(nameof(links));

            if (page is null)
                throw new ArgumentNullException(nameof(page));

            if (string.IsNullOrWhiteSpace(listGetMethod))
                throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or whitespace.", nameof(listGetMethod));

            if (string.IsNullOrWhiteSpace(singleGetMethod))
                throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or whitespace.", nameof(singleGetMethod));

            return CreateForListEndpointWithPaging(resources, keyAccessor, idAccessor, links.FirstHref, links.PrevHref, links.NextHref, links.LastHref, page, controller, listGetMethod, singleGetMethod);
        }

        private IPageLinks GetListNavigation<TDto>(
            IEnumerable<TDto> resources,
            ODataRawQueryOptions rawValues,
            long skip,
            long top,
            long? totalCount)
        {
            return _oDataQueryFactory.GetListNavigation(resources, rawValues, LinkFactory.Create().Href, skip, top, totalCount);
        }
    }
}