using Microsoft.AspNet.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAL.AspNetCore.OData
{
    /// <inheritdoc/>
    public class ODataQueryFactory : IODataQueryFactory
    {
        /// <inheritdoc/>
        public string GenerateQuery(
            ODataRawQueryOptions rawQueryOptions = null,
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
            AppendParameter(sb, "apply", rawQueryOptions?.Apply, applyOverride);
            AppendParameter(sb, "count", rawQueryOptions?.Count, countOverride);
            AppendParameter(sb, "deltatoken", rawQueryOptions?.DeltaToken, deltaTokenOverride);
            AppendParameter(sb, "expand", rawQueryOptions?.Expand, expandOverride);
            AppendParameter(sb, "filter", rawQueryOptions?.Filter, filterOverride);
            AppendParameter(sb, "format", rawQueryOptions?.Format, formatOverride);
            AppendParameter(sb, "orderby", rawQueryOptions?.OrderBy, orderByOverride);
            AppendParameter(sb, "select", rawQueryOptions?.Select, selectOverride);
            AppendParameter(sb, "skip", rawQueryOptions?.Skip, skipOverride);
            AppendParameter(sb, "skiptoken", rawQueryOptions?.SkipToken, skipTokenOverride);
            AppendParameter(sb, "top", rawQueryOptions?.Top, topOverride);

            return Uri.EscapeUriString(sb.ToString());
        }

        /// <inheritdoc/>
        public IPageLinks GetListNavigation<TDto>(
            IEnumerable<TDto> resources,
            ODataRawQueryOptions rawValues,
            string baseHref,
            long skip,
            long top,
            long? totalCount)
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            return GetListNavigation(resources.LongCount(), rawValues, baseHref, skip, top, totalCount);
        }

        /// <inheritdoc/>
        public IPageLinks GetListNavigation(long numberOfResourcesOnThisPage, ODataRawQueryOptions rawValues, string baseHref, long skip, long top, long? totalCount)
        {
            string prevHref = null;
            string nextHref = null;
            string lastHref = null;

            string firstHref = baseHref + GenerateQuery(rawValues, skipOverride: "0");

            if (skip > 0)
                prevHref = baseHref + GenerateQuery(rawValues, skipOverride: Math.Max(0, skip - top).ToString());

            if (numberOfResourcesOnThisPage == top)
                nextHref = baseHref + GenerateQuery(rawValues, skipOverride: (skip + top).ToString());

            if (totalCount.HasValue)
                lastHref = baseHref + GenerateQuery(rawValues, skipOverride: Math.Max(0, top * Math.Ceiling((double)totalCount / top) - top).ToString());

            return new PageLinks(firstHref, prevHref, nextHref, lastHref);
        }

        /// <inheritdoc/>
        public ODataParsedQueryOptions GetSkipAndTop(long maxTop, ODataRawQueryOptions rawValues)
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

            return new ODataParsedQueryOptions(skip, top);
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
    }
}