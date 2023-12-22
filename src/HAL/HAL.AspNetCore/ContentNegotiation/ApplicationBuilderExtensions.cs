using Microsoft.AspNetCore.Builder;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// Contains extension methods for the <see cref="IApplicationBuilder"/> interface.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="IAcceptHeaderFeature"/> to each request.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseAcceptHeaders(this IApplicationBuilder app)
            => app.UseMiddleware<AcceptHeaderMiddleware>();
    }
}
