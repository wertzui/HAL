using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <inheritdoc/>
    public class AcceptHeaderFeature : IAcceptHeaderFeature
    {
        private static readonly MediaTypeWithQualityHeaderValueComparer _comparer = new();

        /// <summary>
        /// Creates a new instance of the <see cref="AcceptHeaderFeature"/> class.
        /// </summary>
        /// <param name="acceptHeaders">The accept headers from the request.</param>
        public AcceptHeaderFeature(IEnumerable<string> acceptHeaders)
        {
            if (!acceptHeaders.TryGetNonEnumeratedCount(out var count))
                count = 0;

            var parsedHeaders = new List<MediaTypeWithQualityHeaderValue>(count);
            foreach (var header in acceptHeaders)
            {
                if (MediaTypeWithQualityHeaderValue.TryParse(header, out var parsedHeader))
                    parsedHeaders.Add(parsedHeader);
            }
            parsedHeaders.Sort(_comparer);

            AcceptHeaders = parsedHeaders;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; }
    }
}