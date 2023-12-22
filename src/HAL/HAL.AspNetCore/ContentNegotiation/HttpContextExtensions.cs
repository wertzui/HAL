using Microsoft.AspNetCore.Http;
using System;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// Contains extension methods for the <see cref="HttpContext"/> class.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the <see cref="IAcceptHeaderFeature"/> from the <see cref="HttpContext.Features"/> of the request.
        /// </summary>
        public static IAcceptHeaderFeature GetAcceptHeaders(this HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Features.Get<IAcceptHeaderFeature>() ?? throw new InvalidOperationException($"The {nameof(AcceptHeaderMiddleware)} was not registered.");
        }
    }
}
