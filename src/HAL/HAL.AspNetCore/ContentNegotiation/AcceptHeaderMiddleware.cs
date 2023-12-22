using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// This filter adds an <see cref="IAcceptHeaderFeature"/> to the <see cref="HttpContext.Features"/> of the request.
    /// </summary>
    public class AcceptHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Creates a new instance of the <see cref="AcceptHeaderMiddleware"/> class.
        /// </summary>
        public AcceptHeaderMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new System.ArgumentNullException(nameof(next));
        }
        /// <inheritdoc/>
        public Task InvokeAsync(HttpContext context)
        {
            var feature = new AcceptHeaderFeature(context.Request.Headers.Accept);
            context.Features.Set<IAcceptHeaderFeature>(feature);

            return _next(context);
        }
    }
}
