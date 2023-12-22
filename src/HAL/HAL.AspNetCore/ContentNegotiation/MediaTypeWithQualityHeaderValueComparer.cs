using System.Collections.Generic;
using System.Net.Http.Headers;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// Compares two <see cref="MediaTypeWithQualityHeaderValue"/>s according to RFC 9110.
    /// They are ordered by precedence, so the first item is the one the client wants the most.
    /// </summary>
    public class MediaTypeWithQualityHeaderValueComparer : IComparer<MediaTypeWithQualityHeaderValue>
    {
        /// <inheritdoc/>
        public int Compare(MediaTypeWithQualityHeaderValue? x, MediaTypeWithQualityHeaderValue? y)
        {
            if (x is null && y is null)
                return 0;
            else if (y is null && x is not null)
                return -1;
            else if (y is not null && x is null)
                return 1;

            // Check quality first
            var xQuality = x!.Quality ?? 1;
            var yQuality = y!.Quality ?? 1;
            if (xQuality > yQuality)
                return -1;
            else if (xQuality < yQuality)
                return 1;

            // Check if one media type is null
            var xMediaType = x.MediaType;
            var yMediaType = y.MediaType;
            var xMediaTypeIsNull = xMediaType is null;
            var yMediaTypeIsNull = yMediaType is null;
            if (xMediaTypeIsNull && yMediaTypeIsNull)
                return 0;
            else if (yMediaTypeIsNull && !xMediaTypeIsNull)
                return -1;
            else if (xMediaTypeIsNull && !yMediaTypeIsNull)
                return 1;

            // Check if one media type is a wild-card
            var xIsWildCard = xMediaType!.Equals("*/*", System.StringComparison.Ordinal);
            var yIsWildCard = yMediaType!.Equals("*/*", System.StringComparison.Ordinal);
            if (yIsWildCard && !xIsWildCard)
                return -1;
            else if (xIsWildCard && !yIsWildCard)
                return 1;

            // Check if one media type has a wild-card at the end
            var xContainsWildcardAtEnd = xMediaType!.EndsWith("/*", System.StringComparison.Ordinal);
            var yContainsWildcardAtEnd = yMediaType!.EndsWith("/*", System.StringComparison.Ordinal);
            if (yContainsWildcardAtEnd && !xContainsWildcardAtEnd)
                return -1;
            else if (xContainsWildcardAtEnd && !yContainsWildcardAtEnd)
                return 1;

            // Check if one header has more parameters than the other
            var xNumParameters = x.Parameters.Count;
            var yNumParameters = y.Parameters.Count;
            if (xNumParameters > yNumParameters)
                return -1;
            else if (xNumParameters < yNumParameters)
                return 1;

            // Both headers have the same quality, the same specificity and the same number of parameters
            // We use alphabetical sorting then to guarantee a stable sort
            return x.MediaType!.CompareTo(y.MediaType);
        }
    }
}
