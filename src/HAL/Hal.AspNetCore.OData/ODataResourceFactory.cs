using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore.OData;

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
    public Resource<Page> CreateForODataListEndpointUsingSkipTopPaging<TDto, TKey, TId>(
        IEnumerable<TDto> resources,
        Func<TDto, TKey> keyAccessor,
        Func<TDto, TId> idAccessor,
        ODataRawQueryOptions oDataQueryOptions,
        long maxTop = 50,
        long? totalCount = null,
        string? controller = null,
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

        (long skip, long top) = _oDataQueryFactory.GetSkipAndTop(maxTop, oDataQueryOptions);

        var currentPage = (skip == 0 ? 0 : skip / top) + 1;
        var totalPages = totalCount.HasValue ? (long)Math.Ceiling((double)totalCount / top) : default;
        var page = new Page { CurrentPage = currentPage, TotalPages = totalPages };

        var links = _oDataQueryFactory.GetListNavigation(resources, oDataQueryOptions, LinkFactory.Create(action: listGetMethod, controller: controller).Href, skip, top, totalCount);

        var resource = CreateForListEndpointWithPaging(resources, keyAccessor, idAccessor, links, page, controller, listGetMethod, singleGetMethod);

        return resource;
    }
}