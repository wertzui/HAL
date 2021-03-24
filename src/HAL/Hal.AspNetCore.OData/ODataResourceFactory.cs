using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAL.AspNetCore.OData
{
    /// <summary>
    /// A factory to create resources based on OData queries.
    /// </summary>
    /// <seealso cref="ResourceFactory" />
    /// <seealso cref="IODataResourceFactory" />
    public class ODataResourceFactory : ResourceFactory, IODataResourceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataResourceFactory"/> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        public ODataResourceFactory(ILinkFactory linkFactory, IApiDescriptionGroupCollectionProvider apiExplorer) : base(linkFactory, apiExplorer)
        {
        }

        /// <inheritdoc/>
        public Resource CreateForOdataListEndpointUsingSkipTopPaging<TDto, TEntity, TId>(IEnumerable<TDto> resources, Func<TDto, TId> idAccessor, ODataQueryOptions<TEntity> oDataQueryOptions, long maxTop = 50, long? totalCount = null, string getMethod = "Get")
        {
            var rawValues = oDataQueryOptions.RawValues;

            (long skip, long top) = GetSkipAndTop(maxTop, rawValues);

            var page = new Page { CurrentPage = skip == 0 ? 0 : skip / top, TotalPages = totalCount / top };

            (string prevHref, string nextHref) = GetPrevAndNextHref(resources, rawValues, skip, top);

            var resource = CreateForListEndpointWithPaging(resources, idAccessor, prevHref, nextHref, page, getMethod);

            return resource;
        }

        private static void AppendParameter(StringBuilder builder, string key, string value, string overrideValue)
        {
            if (value is not null || overrideValue is not null)
            {
                if (builder.Length == 0)
                    builder.Append("?$");
                else
                    builder.Append("&$");

                builder.Append(key);
                builder.Append('=');
                builder.Append(overrideValue ?? value);
            }
        }

        private static string GenerateQuery(
            ODataRawQueryOptions rawQueryOptions,
            string applyOverride = null,
            string countOverride = null,
            string deltaTokenOverride = null,
            string expandOverride = null,
            string filterOverride = null,
            string formatOverride = null,
            string orderByOverride = null,
            string selectOverride = null,
            string skipOverride = null,
            string skipTokenOverride = null,
            string topOverride = null)
        {
            var sb = new StringBuilder(50);
            AppendParameter(sb, "apply", rawQueryOptions.Apply, applyOverride);
            AppendParameter(sb, "count", rawQueryOptions.Count, countOverride);
            AppendParameter(sb, "deltatoken", rawQueryOptions.DeltaToken, deltaTokenOverride);
            AppendParameter(sb, "expand", rawQueryOptions.Expand, expandOverride);
            AppendParameter(sb, "filter", rawQueryOptions.Filter, filterOverride);
            AppendParameter(sb, "format", rawQueryOptions.Format, formatOverride);
            AppendParameter(sb, "orderby", rawQueryOptions.OrderBy, orderByOverride);
            AppendParameter(sb, "select", rawQueryOptions.Select, selectOverride);
            AppendParameter(sb, "skip", rawQueryOptions.Skip, skipOverride);
            AppendParameter(sb, "skiptoken", rawQueryOptions.SkipToken, skipTokenOverride);
            AppendParameter(sb, "top", rawQueryOptions.Top, topOverride);

            return Uri.EscapeUriString(sb.ToString());
        }

        private static (long skip, long top) GetSkipAndTop(long maxTop, ODataRawQueryOptions rawValues)
        {
            if (!long.TryParse(rawValues.Skip, out var skip))
            {
                if (string.IsNullOrWhiteSpace(rawValues.Skip))
                    skip = 0;
                else
                    throw new FormatException($"'{rawValues.Skip}' is not valid for $skip because it cannot be parsed as a long.");
            }
            if (!long.TryParse(rawValues.Top, out var top))
            {
                if (string.IsNullOrWhiteSpace(rawValues.Top))
                    top = maxTop;
                else
                    throw new FormatException($"'{rawValues.Top}' is not valid for $top because it cannot be parsed as a long.");
            }
            top = Math.Min(top, maxTop);

            return (skip, top);
        }

        private (string prevHref, string nextHref) GetPrevAndNextHref<TDto>(IEnumerable<TDto> resources, ODataRawQueryOptions rawValues, long skip, long top)
        {
            var baseHref = _linkFactory.Create().Href;
            string prevHref = null;
            string nextHref = null;
            if (skip > 0)
                prevHref = baseHref + GenerateQuery(rawValues, skipOverride: Math.Max(0, skip - top).ToString());

            if (resources.Count() == top)
                nextHref = baseHref + GenerateQuery(rawValues, skipOverride: (skip + top).ToString());

            return (prevHref, nextHref);
        }
    }
}