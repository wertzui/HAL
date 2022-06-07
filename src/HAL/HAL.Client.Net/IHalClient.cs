namespace HAL.Client.Net
{
    /// <summary>
    /// A client for making requests to a HAL endpoint.
    /// </summary>
    public interface IHalClient
    {
        /// <summary>
        /// Sends an HTTP DELETE request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response content.</typeparam>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="eTag">The ETag header value to use to identify version conflicts.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse<TResponse>> DeleteAsync<TResponse>(Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP GET request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response content.</typeparam>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse<TResponse>> GetAsync<TResponse>(Uri requestUri, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP POST request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content.</typeparam>
        /// <typeparam name="TResponse">The type of the response content.</typeparam>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse<TResponse>> PostAsync<TRequest, TResponse>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP PUT request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content.</typeparam>
        /// <typeparam name="TResponse">The type of the response content.</typeparam>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse<TResponse>> PutAsync<TRequest, TResponse>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content.</typeparam>
        /// <typeparam name="TResponse">The type of the response content.</typeparam>
        /// <param name="method">The <see cref="HttpMethod"/> to use when following the link.</param>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse<TResponse>> SendAsync<TRequest, TResponse>(HttpMethod method, Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sends an HTTP DELETE request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="eTag">The ETag header value to use to identify version conflicts.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse> DeleteAsync(Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP GET request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse> GetAsync(Uri requestUri, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP POST request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content.</typeparam>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse> PostAsync<TRequest>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP PUT request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content.</typeparam>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse> PutAsync<TRequest>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request to the given <paramref name="requestUri"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content.</typeparam>
        /// <param name="method">The <see cref="HttpMethod"/> to use when following the link.</param>
        /// <param name="requestUri">The URI to make the request to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
        /// <param name="headers">Additional headers to use in the request.</param>
        /// <param name="version">
        /// An optional version which is appended to the Accept header as v={version}.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response received after making the HTTP request.</returns>
        Task<HalResponse> SendAsync<TRequest>(HttpMethod method, Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);
    }
}