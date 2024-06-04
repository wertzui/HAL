using HAL.Common;
using HAL.Common.Forms;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Json;
using Tavis.UriTemplates;

namespace HAL.Client.Net;

/// <summary>
/// A client for making requests to a HAL endpoint.
/// </summary>
public class HalClient : IHalClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _name = nameof(HalClient);

    /// <summary>
    /// Initializes a new instance of the <see cref="HalClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">
    /// The factory which is used to create the clients for making the HTTP requests.
    /// </param>
    public HalClient(IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HalClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">
    /// The factory which is used to create the clients for making the HTTP requests.
    /// </param>
    /// <param name="name">The name that is used to retrieve the HTTPClient from the <paramref name="httpClientFactory"/>.</param>
    public HalClient(IHttpClientFactory httpClientFactory, string name)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _httpClientFactory = httpClientFactory;
        _name = name;
    }

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> DeleteAsync<TState>(
        Uri requestUri,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync<object, TState>(HttpMethod.Delete, requestUri, null, uriParameters, AddETag(headers, eTag), version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> DeleteAsync(
        Uri requestUri,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync<object>(HttpMethod.Delete, requestUri, null, uriParameters, AddETag(headers, eTag), version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> GetAsync<TState>(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync<object, TState>(HttpMethod.Get, requestUri, null, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> GetAsync(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync<object>(HttpMethod.Get, requestUri, null, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> PostAsync<TRequest, TState>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync<TRequest, TState>(HttpMethod.Post, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> PostAsync<TRequest>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> PutAsync<TRequest, TState>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync<TRequest, TState>(HttpMethod.Put, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> PutAsync<TRequest>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Put, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public async Task<HalResponse<Resource<TState>>> SendAsync<TRequest, TState>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendHttpRequestAsync(method, requestUri, Accepts.Hal, content, uriParameters, headers, version, HttpCompletionOption.ResponseContentRead, cancellationToken);

        var halResponse = await HalResponse.FromHttpResponse<Resource<TState>>(response, cancellationToken);

        return halResponse;
    }

    /// <inheritdoc/>
    public async Task<HalResponse<Resource>> SendAsync<TRequest>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendHttpRequestAsync(method, requestUri, Accepts.Hal, content, uriParameters, headers, version, HttpCompletionOption.ResponseContentRead, cancellationToken);

        var halResponse = await HalResponse.FromHttpResponse<Resource>(response, cancellationToken);

        return halResponse;
    }

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> DeleteFormAsync<TState>(
        Uri requestUri,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync<object, TState>(HttpMethod.Delete, requestUri, null, uriParameters, AddETag(headers, eTag), version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> DeleteFormAsync(
        Uri requestUri,
        string? eTag = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync<object>(HttpMethod.Delete, requestUri, null, uriParameters, AddETag(headers, eTag), version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> GetFormAsync<TState>(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync<object, TState>(HttpMethod.Get, requestUri, null, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> GetFormAsync(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync<object>(HttpMethod.Get, requestUri, null, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> PostFormAsync<TRequest, TState>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync<TRequest, TState>(HttpMethod.Post, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> PostFormAsync<TRequest>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync(HttpMethod.Post, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> PutFormAsync<TRequest, TState>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync<TRequest, TState>(HttpMethod.Put, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> PutFormAsync<TRequest>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
        => SendFormAsync(HttpMethod.Put, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public async Task<HalResponse<FormsResource<TState>>> SendFormAsync<TRequest, TState>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendHttpRequestAsync(method, requestUri, Accepts.HalForms, content, uriParameters, headers, version, HttpCompletionOption.ResponseContentRead, cancellationToken);

        var halResponse = await HalResponse.FromHttpResponse<FormsResource<TState>>(response, cancellationToken);

        return halResponse;
    }

    /// <inheritdoc/>
    public async Task<HalResponse<FormsResource>> SendFormAsync<TRequest>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendHttpRequestAsync(method, requestUri, Accepts.HalForms, content, uriParameters, headers, version, HttpCompletionOption.ResponseContentRead, cancellationToken);

        var halResponse = await HalResponse.FromHttpResponse<FormsResource>(response, cancellationToken);

        return halResponse;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(
        HttpMethod method,
        Uri requestUri,
        Accepts accepts,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        var response = await SendHttpRequestAsync(request, accepts, content, uriParameters, headers, version, completionOption, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(
        HttpRequestMessage request,
        Accepts accepts,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        if (request.RequestUri is null)
            throw new ArgumentException("The request must have a RequestUri.", nameof(request));

        AddHeadersToRequest(headers, accepts, version, request);

        AddParametersToRequest(request.RequestUri, uriParameters, request);

        AddContentToRequest(request.Method, request.RequestUri, content, request);

        using var client = _httpClientFactory.CreateClient(_name);
        var response = await client.SendAsync(request, completionOption, cancellationToken);

        return response;
    }

    private static void AddContentToRequest<TRequest>(HttpMethod method, Uri requestUri, TRequest? content, HttpRequestMessage request)
    {
        if (content is not null)
        {
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                request.Content = JsonContent.Create(content);
            }
            else
            {
                request.RequestUri = new UriBuilder(requestUri)
                    .AppendQuery(content)
                    .Uri;
            }
        }
    }

    private static IDictionary<string, IEnumerable<string>>? AddETag(IDictionary<string, IEnumerable<string>>? headers, string? eTag)
    {
        if (headers is not null && eTag is not null)
        {
            headers[HeaderNames.ETag] = [eTag];
        }

        return headers;
    }

    private static void AddHeadersToRequest(IDictionary<string, IEnumerable<string>>? headers, Accepts accepts, string? version, HttpRequestMessage request)
    {
        // Add default accept headers if the original response does not already contain any headers
        // which are used in conjunction with HAL.
        if (headers is null || !headers.TryGetValue(HeaderNames.Accept, out var userAcceptHeaders) ||
            !(userAcceptHeaders.Contains(Constants.MediaTypes.Hal) || userAcceptHeaders.Contains(Constants.MediaTypes.HalForms) || userAcceptHeaders.Contains(Constants.MediaTypes.HalFormsPrs) || userAcceptHeaders.Contains(Constants.MediaTypes.Json)))
        {
            switch (accepts)
            {
                case Accepts.Hal:
                    request.Headers.Accept.Add(new(Constants.MediaTypes.Hal, 0.9));
                    request.Headers.Accept.Add(new(Constants.MediaTypes.Json, 0.8));
                    break;
                case Accepts.HalForms:
                    request.Headers.Accept.Add(new(Constants.MediaTypes.HalForms, 0.9));
                    request.Headers.Accept.Add(new(Constants.MediaTypes.HalFormsPrs, 0.8));
                    request.Headers.Accept.Add(new(Constants.MediaTypes.Hal, 0.7));
                    request.Headers.Accept.Add(new(Constants.MediaTypes.Json, 0.6));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(accepts), accepts, "The value is not supported.");
            }
        }

        // Add the headers provided by the user.
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Add a version to all accept headers.
        if (version is not null)
        {
            foreach (var header in request.Headers.Accept)
            {
                header.Parameters.Add(new("v", version));
            }
        }
    }

    private static void AddParametersToRequest(Uri requestUri, IDictionary<string, object>? uriParameters, HttpRequestMessage request)
    {
        // The library does not correctly ignore the $ sign, but treats it in a special way. As
        // a workaround, we replace all $ signs. See https://github.com/tavis-software/Tavis.UriTemplates/issues/74

        var template = new UriTemplate(requestUri.ToString().Replace("$", "__DOLLAR__"));
        if (uriParameters is not null && uriParameters.Count > 0)
        {
            foreach (var parameter in uriParameters)
            {
                template.AddParameter(parameter.Key.Replace("$", "__DOLLAR__"), parameter.Value);
            }
        }

        var resolvedString = template.Resolve().Replace("__DOLLAR__", "$");
        var resolvedUri = new Uri(resolvedString);

        request.RequestUri = resolvedUri;
    }
}