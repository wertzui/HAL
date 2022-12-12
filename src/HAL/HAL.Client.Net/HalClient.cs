using Microsoft.Net.Http.Headers;
using System.Net.Http.Json;
using Tavis.UriTemplates;

namespace HAL.Client.Net
{
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
        /// <param name="httpClientFactory">The factory which is used to create the clients for making the HTTP requests.</param>
        public HalClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HalClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The factory which is used to create the clients for making the HTTP requests.</param>
        /// <param name="name">The name that is used to retrieve the HTTPClient from the <paramref name="httpClientFactory"/>.</param>
        public HalClient(IHttpClientFactory httpClientFactory, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            _httpClientFactory = httpClientFactory;
            _name = name;
        }

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> DeleteAsync<TResponse>(
            Uri requestUri,
            string? eTag = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync<object, TResponse>(HttpMethod.Delete, requestUri, null, uriParameters, AddEtag(headers, eTag), version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> DeleteAsync(
            Uri requestUri,
            string? eTag = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Delete, requestUri, null, uriParameters, AddEtag(headers, eTag), version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> GetAsync<TResponse>(
            Uri requestUri,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync<object, TResponse>(HttpMethod.Get, requestUri, null, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> GetAsync(
            Uri requestUri,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync<object>(HttpMethod.Get, requestUri, null, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> PostAsync<TRequest, TResponse>(
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> PostAsync<TRequest>(
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync(HttpMethod.Post, requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> PutAsync<TRequest, TResponse>(
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync<TRequest, TResponse>(HttpMethod.Put, requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> PutAsync<TRequest>(
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
            => SendAsync(HttpMethod.Put, requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public async Task<HalResponse<TResponse>> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendHttpRequestAsync(method, requestUri, content, uriParameters, headers, version, cancellationToken);

            var halResponse = await HalResponse.FromHttpResponse<TResponse>(response, cancellationToken);

            return halResponse;
        }

        /// <inheritdoc/>
        public async Task<HalResponse> SendAsync<TRequest>(
            HttpMethod method,
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendHttpRequestAsync(method, requestUri, content, uriParameters, headers, version, cancellationToken);

            var halResponse = await HalResponse.FromHttpResponse(response, cancellationToken);

            return halResponse;
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

        private static IDictionary<string, IEnumerable<string>>? AddEtag(IDictionary<string, IEnumerable<string>>? headers, string? eTag)
        {
            if (headers is not null && eTag is not null)
            {
                headers[HeaderNames.ETag] = new[] { eTag };
            }

            return headers;
        }

        private static void AddHeadersToRequest(IDictionary<string, IEnumerable<string>>? headers, string? version, HttpRequestMessage request)
        {
            if (headers is null || !headers.ContainsKey(HeaderNames.Accept))
            {
                request.Headers.Add(HeaderNames.Accept, "application/hal+json");
                request.Headers.Add(HeaderNames.Accept, "application/json");
            }

            if (version is not null)
                request.Headers.Add(HeaderNames.Accept, $"v={version}");

            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }

        private static void AddParametersToRequest(Uri requestUri, IDictionary<string, object>? uriParameters, HttpRequestMessage request)
        {

            // The library does not correctly ignore the $ sign, but treats it in a special way.
            // As a workaround, we replace all $ signs.
            // See https://github.com/tavis-software/Tavis.UriTemplates/issues/74

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

        private async Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(
            HttpMethod method,
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(method, requestUri);

            AddHeadersToRequest(headers, version, request);

            AddParametersToRequest(requestUri, uriParameters, request);

            AddContentToRequest(method, requestUri, content, request);

            using var client = _httpClientFactory.CreateClient(_name);
            var response = await client.SendAsync(request, cancellationToken);

            return response;
        }
    }
}