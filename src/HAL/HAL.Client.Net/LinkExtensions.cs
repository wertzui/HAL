using HAL.Client.Net;
using HAL.Common.Forms;

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
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<Resource<TState>>> FollowAsync<TRequest, TState>(
        this Link link,
        IHalClient client,
        HttpMethod method,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.SendAsync<TRequest, TState>(method, new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<Resource>> FollowAsync<TRequest>(
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
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<Resource<TState>>> FollowDeleteAsync<TState>(
        this Link link,
        IHalClient client,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync<TState>(new Uri(link.Href), eTag, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<Resource>> FollowDeleteAsync(
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
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<Resource<TState>>> FollowGetAsync<TState>(
                this Link link,
        IHalClient client,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.GetAsync<TState>(new Uri(link.Href), uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<Resource>> FollowGetAsync(
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
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<Resource<TState>>> FollowPostAsync<TRequest, TState>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PostAsync<TRequest, TState>(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<Resource>> FollowPostAsync<TRequest>(
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
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<Resource<TState>>> FollowPutAsync<TRequest, TState>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PutAsync<TRequest, TState>(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<Resource>> FollowPutAsync<TRequest>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PutAsync(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<FormsResource<TState>>> FollowFormAsync<TRequest, TState>(
        this Link link,
        IHalClient client,
        HttpMethod method,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.SendFormAsync<TRequest, TState>(method, new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<FormsResource>> FollowFormAsync<TRequest>(
        this Link link,
        IHalClient client,
        HttpMethod method,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.SendFormAsync<TRequest>(method, new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP DELETE request to the href of the link.
    /// </summary>
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<FormsResource<TState>>> FollowDeleteFormAsync<TState>(
        this Link link,
        IHalClient client,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.DeleteFormAsync<TState>(new Uri(link.Href), eTag, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<FormsResource>> FollowDeleteFormAsync(
        this Link link,
        IHalClient client,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.DeleteFormAsync(new Uri(link.Href), eTag, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP GET request to the href of the link.
    /// </summary>
    /// <typeparam name="TState">The type of the response content.</typeparam>
    /// <param name="link">The link to follow.</param>
    /// <param name="client">The <see cref="HttpClient"/> to use when following the link.</param>
    /// <param name="uriParameters">Parameters to use if the href of the link is templated.</param>
    /// <param name="headers">Additional headers to use in the request.</param>
    /// <param name="version">
    /// An optional version which is appended to the Accept header as v={version}.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response received after making the HTTP request.</returns>
    public static Task<HalResponse<FormsResource<TState>>> FollowGetFormAsync<TState>(
                this Link link,
        IHalClient client,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.GetFormAsync<TState>(new Uri(link.Href), uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<FormsResource>> FollowGetFormAsync(
                this Link link,
        IHalClient client,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.GetFormAsync(new Uri(link.Href), uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP POST request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<FormsResource<TState>>> FollowPostFormAsync<TRequest, TState>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PostFormAsync<TRequest, TState>(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<FormsResource>> FollowPostFormAsync<TRequest>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PostFormAsync(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

    /// <summary>
    /// Follows the link by making an HTTP PUT request to the href of the link.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TState">The type of the response content.</typeparam>
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
    public static Task<HalResponse<FormsResource<TState>>> FollowPutFormAsync<TRequest, TState>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PutFormAsync<TRequest, TState>(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);

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
    public static Task<HalResponse<FormsResource>> FollowPutFormAsync<TRequest>(
        this Link link,
        IHalClient client,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => client.PutFormAsync(new Uri(link.Href), content, uriParameters, headers, version, cancellationToken);
}