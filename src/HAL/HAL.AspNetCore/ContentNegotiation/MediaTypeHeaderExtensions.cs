using HAL.Common;
using System;
using System.Net.Http.Headers;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// Contains extension methods for the <see cref="MediaTypeHeaderValue"/> class.
    /// </summary>
    public static class MediaTypeHeaderValueExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the media type is HAL.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns><c>true</c> if the media type is HAL; <c>false</c> otherwise.</returns>
        public static bool IsHal(this MediaTypeHeaderValue header)
            => string.Equals(header.MediaType, Constants.MediaTypes.Hal, StringComparison.Ordinal);

        /// <summary>
        /// Gets a value indicating whether the media type is HAL-Forms.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns><c>true</c> if the media type is HAL-Forms; <c>false</c> otherwise.</returns>
        public static bool IsHalForms(this MediaTypeHeaderValue header)
            => string.Equals(header.MediaType, Constants.MediaTypes.HalForms, StringComparison.Ordinal)
            || string.Equals(header.MediaType, Constants.MediaTypes.HalFormsPrs, StringComparison.Ordinal);
    }
}
