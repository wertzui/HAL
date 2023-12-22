using System.Collections.Generic;
using System.Net.Http.Headers;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// Contains the accept headers of the request.
    /// </summary>
    public interface IAcceptHeaderFeature
    {

        /// <summary>
        /// Gets the accept headers sorted as specified in RFC 9110.
        /// https://www.rfc-editor.org/rfc/rfc9110#name-accept
        /// They are ordered by precedence, so the first item is the one the client wants the most.
        /// </summary>
        IReadOnlyCollection<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; }
    }
}