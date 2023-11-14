using HAL.Client.Net;

namespace HAL.Common;

/// <summary>
/// Contains extension methods for <see cref="Link"/>.
/// </summary>
public static class LinkExtensions
{
    /// <summary>
    /// Follows the link by making an HTTP request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="method">The <see cref="HttpMethod"/> to use when following the link.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<TResponse>> FollowAsync<TRequest, TResponse>(
        this Link link,
        IHalClient client,
        HttpMethod method,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.SendAsync<TRequest, TResponse>(method, new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="method">The <see cref="HttpMethod"/> to use when following the link.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse> FollowAsync<TRequest>(
        this Link link,
        IHalClient client,
        HttpMethod method,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.SendAsync<TRequest>(method, new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP DELETE request to the href of the link.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="eTag">The ETag header value to use to identify version conflicts.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<TResponse>> FollowDeleteAsync<TResponse>(
        this Link link,
        IHalClient client,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync<TResponse>(new Uri(link.Href), eTag, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP DELETE request to the href of the link.
    /// </summary>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="eTag">The ETag header value to use to identify version conflicts.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse> FollowDeleteAsync(
        this Link link,
        IHalClient client,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync(new Uri(link.Href), eTag, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP GET request to the href of the link.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<TResponse>> FollowGetAsync<TResponse>(
                this Link link,
        IHalClient client,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.GetAsync<TResponse>(new Uri(link.Href), uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP GET request to the href of the link.
    /// </summary>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse> FollowGetAsync(
                this Link link,
        IHalClient client,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.GetAsync(new Uri(link.Href), uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP POST request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<TResponse>> FollowPostAsync<TRequest, TResponse>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PostAsync<TRequest, TResponse>(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP POST request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse> FollowPostAsync<TRequest>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PostAsync(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP PUT request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<TResponse>> FollowPutAsync<TRequest, TResponse>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PutAsync<TRequest, TResponse>(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP PUT request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse> FollowPutAsync<TRequest>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PutAsync(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);
}