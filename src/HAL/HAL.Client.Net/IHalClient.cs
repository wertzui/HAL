using HAL.Common;
using HAL.Common.Forms;

namespace HAL.Client.Net;

/// <summary>
/// A client for making requests to a HAL endpoint.
/// </summary>
public interface IHalClient
{
    /// <summary>
    /// Sends an HTTP DELETE request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="eTag">The ETag header value to use to identify version conflicts.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<Resource<TState>>> DeleteAsync<TState>(Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<Resource>> DeleteAsync(Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP GET request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<Resource<TState>>> GetAsync<TState>(Uri requestUri, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<Resource>> GetAsync(Uri requestUri, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP POST request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<Resource<TState>>> PostAsync<TRequest, TState>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<Resource>> PostAsync<TRequest>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP PUT request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<Resource<TState>>> PutAsync<TRequest, TState>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<Resource>> PutAsync<TRequest>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    Task<HalResponse<Resource<TState>>> SendAsync<TRequest, TState>(HttpMethod method, Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<Resource>> SendAsync<TRequest>(HttpMethod method, Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP DELETE request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="eTag">The ETag header value to use to identify version conflicts.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<FormsResource<TState>>> DeleteFormAsync<TState>(Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<FormsResource>> DeleteFormAsync(Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP GET request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<FormsResource<TState>>> GetFormAsync<TState>(Uri requestUri, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<FormsResource>> GetFormAsync(Uri requestUri, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP POST request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<FormsResource<TState>>> PostFormAsync<TRequest, TState>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<FormsResource>> PostFormAsync<TRequest>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP PUT request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HalResponse<FormsResource<TState>>> PutFormAsync<TRequest, TState>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<FormsResource>> PutFormAsync<TRequest>(Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    Task<HalResponse<FormsResource<TState>>> SendFormAsync<TRequest, TState>(HttpMethod method, Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

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
    Task<HalResponse<FormsResource>> SendFormAsync<TRequest>(HttpMethod method, Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request to the given <paramref name="requestUri"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <param name="method">The <see cref="HttpMethod"/> to use when following the link.</param>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="accepts">Whether the requests expects HAL or HAL-Forms as response type.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="completionOption">When the operation should complete (as soon as a response is available or after reading the whole response content).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(HttpMethod method, Uri requestUri, Accepts accepts, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the given <paramref name="request"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <param name="request">The request to send out.</param>
    /// <param name="accepts">Whether the requests expects HAL or HAL-Forms as response type.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="completionOption">When the operation should complete (as soon as a response is available or after reading the whole response content).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(HttpRequestMessage request, Accepts accepts, TRequest? content = default, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);
}