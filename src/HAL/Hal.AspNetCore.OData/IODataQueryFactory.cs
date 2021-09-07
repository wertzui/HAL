using Microsoft.AspNet.OData.Query;
using System.Collections.Generic;

namespace HAL.AspNetCore.OData
{
    /// <summary>
    /// A factory to generate links using OData syntax. Mostly for navigating through paged lists.
    /// </summary>
    public interface IODataQueryFactory
    {
        /// <summary>
        /// Generates an OData query string.
        /// </summary>
        /// <param name="rawQueryOptions">The OData query options to provide initial parameters. It may be null in which case only the overrides are taken into account.</param>
        /// <param name="applyOverride">The $apply override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>.</param>
        /// <param name="countOverride">The $count override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="deltaTokenOverride">The $delta token override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="expandOverride">The $expand override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="filterOverride">The $filter override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="formatOverride">The $format override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="orderByOverride">The $order by override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="selectOverride">The $select override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="skipOverride">The $skip override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="skipTokenOverride">The $skip token override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <param name="topOverride">The $top override. If it is provided, it will override the value from <paramref name="rawQueryOptions"/>..</param>
        /// <returns>The query part of an URL which uses the given <paramref name="rawQueryOptions"/>.</returns>
        string GenerateQuery(ODataRawQueryOptions rawQueryOptions = null, string applyOverride = null, string countOverride = null, string deltaTokenOverride = null, string expandOverride = null, string filterOverride = null, string formatOverride = null, string orderByOverride = null, string selectOverride = null, string skipOverride = null, string skipTokenOverride = null, string topOverride = null);

        /// <summary>
        /// Gets the list navigation href values to be used on a list endpoint.
        /// </summary>
        /// <param name="numberOfResourcesOnThisPage">The number of resources on this page.</param>
        /// <param name="rawValues">The OData values which have been used to navigate to the current page.</param>
        /// <param name="baseHref">The href of this page without any query part in it.</param>
        /// <param name="skip">The parsed $skip value of the <paramref name="rawValues" />. Use <see cref="GetSkipAndTop(long, ODataRawQueryOptions)" /> to extract them.</param>
        /// <param name="top">The parsed $top value of the <paramref name="rawValues" />. Use <see cref="GetSkipAndTop(long, ODataRawQueryOptions)" /> to extract them.</param>
        /// <param name="totalCount">The total number of elements on all pages. If provided, the lastHref return value will be generated.</param>
        /// <returns>The links for navigating from the current page.</returns>
        IPageLinks GetListNavigation(long numberOfResourcesOnThisPage, ODataRawQueryOptions rawValues, string baseHref, long skip, long top, long? totalCount);

        /// <summary>
        /// Gets the list navigation href values to be used on a list endpoint.
        /// </summary>
        /// <typeparam name="TDto">The type of the dto.</typeparam>
        /// <param name="resources">The resources on this page.</param>
        /// <param name="rawValues">The OData values which have been used to navigate to the current page.</param>
        /// <param name="baseHref">The href of this page without any query part in it.</param>
        /// <param name="skip">The parsed $skip value of the <paramref name="rawValues"/>. Use <see cref="GetSkipAndTop(long, ODataRawQueryOptions)"/> to extract them.</param>
        /// <param name="top">The parsed $top value of the <paramref name="rawValues"/>. Use <see cref="GetSkipAndTop(long, ODataRawQueryOptions)"/> to extract them.</param>
        /// <param name="totalCount">The total number of elements on all pages. If provided, the lastHref return value will be generated.</param>
        /// <returns>The links for navigating from the current page.</returns>
        IPageLinks GetListNavigation<TDto>(IEnumerable<TDto> resources, ODataRawQueryOptions rawValues, string baseHref, long skip, long top, long? totalCount);

        /// <summary>
        /// Gets the skip and top values out of the OData values.
        /// </summary>
        /// <param name="maxTop">The maximum top (normally this comes from the appsettings).</param>
        /// <param name="rawValues">The OData values.</param>
        /// <returns>Parsed $skip and $top values.</returns>
        /// <exception cref="System.FormatException">
        /// '{rawValues.Skip}' is not valid for $skip because it cannot be parsed as a long.
        /// or
        /// '{rawValues.Top}' is not valid for $top because it cannot be parsed as a long.
        /// </exception>
        ODataParsedQueryOptions GetSkipAndTop(long maxTop, ODataRawQueryOptions rawValues);
    }
}