using HAL.Client.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HAL.Tests.Client;

[TestClass]
public class HalClientTests
{
    // -------------------------------------------------------------------------
    // GetAsync
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_sends_GET_request_to_given_uri()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var uri = new Uri("http://example.org/items");

        // Act
        await sut.GetAsync(uri, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest.Method);
        Assert.AreEqual(uri, handler.LastRequest.RequestUri);
    }

    [TestMethod]
    public async Task GetAsync_includes_hal_accept_header()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);

        // Act
        await sut.GetAsync(new Uri("http://example.org/items"), cancellationToken: TestContext.CancellationToken);

        // Assert
        var acceptHeaders = handler.LastRequest!.Headers.Accept.Select(h => h.MediaType).ToList();
        CollectionAssert.Contains(acceptHeaders, HAL.Common.Constants.MediaTypes.Hal);
    }

    [TestMethod]
    public async Task GetAsync_typed_returns_succeeded_response_with_resource()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse("{\"name\":\"Alice\"}"));
        var sut = CreateSut(handler);

        // Act
        var response = await sut.GetAsync<TestState>(new Uri("http://example.org/items"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual("Alice", response.Resource.State.Name);
    }

    // -------------------------------------------------------------------------
    // PostAsync
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task PostAsync_sends_POST_request_with_json_body()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var content = new TestState { Name = "Bob" };

        // Act
        await sut.PostAsync(new Uri("http://example.org/items"), content, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Post, handler.LastRequest.Method);
        Assert.IsNotNull(handler.LastRequestBody);
        Assert.Contains("Bob", handler.LastRequestBody);
    }

    // -------------------------------------------------------------------------
    // PutAsync
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task PutAsync_sends_PUT_request_with_json_body()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var content = new TestState { Name = "Charlie" };

        // Act
        await sut.PutAsync(new Uri("http://example.org/items/1"), content, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Put, handler.LastRequest.Method);
        Assert.IsNotNull(handler.LastRequestBody);
        Assert.Contains("Charlie", handler.LastRequestBody);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task DeleteAsync_sends_DELETE_request_to_given_uri()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var uri = new Uri("http://example.org/items/1");

        // Act
        await sut.DeleteAsync(uri, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.AreEqual(uri, handler.LastRequest.RequestUri);
    }

    [TestMethod]
    public async Task DeleteAsync_with_eTag_includes_ETag_header()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var headers = new Dictionary<string, IEnumerable<string>>();

        // Act
        await sut.DeleteAsync(new Uri("http://example.org/items/1"), eTag: "\"abc123\"", headers: headers, cancellationToken: TestContext.CancellationToken);

        // Assert
        // ETag is added to the headers dictionary before sending
        Assert.IsTrue(headers.ContainsKey("ETag"));
        CollectionAssert.Contains(headers["ETag"].ToList(), "\"abc123\"");
    }

    // -------------------------------------------------------------------------
    // URI template resolution
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_resolves_uri_template_parameters()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var templateUri = new Uri("http://example.org/items/{id}");
        var parameters = new Dictionary<string, object> { { "id", 42 } };

        // Act
        await sut.GetAsync(templateUri, uriParameters: parameters, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual("http://example.org/items/42", handler.LastRequest.RequestUri!.ToString());
    }

    // -------------------------------------------------------------------------
    // Version in Accept header
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_with_version_adds_version_parameter_to_accept_header()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);

        // Act
        await sut.GetAsync(new Uri("http://example.org/items"), version: "2", cancellationToken: TestContext.CancellationToken);

        // Assert
        var acceptHeaders = handler.LastRequest!.Headers.Accept.ToList();
        Assert.Contains(
            h => h.Parameters.Any(p => p.Name == "v" && p.Value == "2"), acceptHeaders,
            "Expected 'v=2' parameter on at least one Accept header entry.");
    }

    // -------------------------------------------------------------------------
    // Custom headers
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_with_custom_headers_includes_them_in_the_request()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);
        var customHeaders = new Dictionary<string, IEnumerable<string>>
        {
            { "X-Custom-Header", ["custom-value"] }
        };

        // Act
        await sut.GetAsync(new Uri("http://example.org/items"), headers: customHeaders, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(handler.LastRequest!.Headers.Contains("X-Custom-Header"));
    }

    // -------------------------------------------------------------------------
    // Non-success response
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_returns_failed_response_when_server_returns_error()
    {
        // Arrange
        var errorResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{\"title\":\"Not Found\",\"status\":404}", Encoding.UTF8, "application/problem+json")
        };
        var handler = new FakeHttpMessageHandler(errorResponse);
        var sut = CreateSut(handler);

        // Act
        var response = await sut.GetAsync(new Uri("http://example.org/items/99"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsFalse(response.Succeeded);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.IsNotNull(response.ProblemDetails);
    }

    [TestMethod]
    public async Task GetAsync_EnsureSuccessStatusCode_throws_when_not_successful()
    {
        // Arrange
        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"title\":\"Bad Request\",\"status\":400}", Encoding.UTF8, "application/problem+json")
        };
        var handler = new FakeHttpMessageHandler(errorResponse);
        var sut = CreateSut(handler);

        // Act
        var response = await sut.GetAsync(new Uri("http://example.org/items"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.ThrowsExactly<HttpRequestException>(() => response.EnsureSuccessStatusCode());
    }

    // -------------------------------------------------------------------------
    // HalForms accept header (GetFormAsync)
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetFormAsync_sends_GET_request_with_hal_forms_accept_header()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkFormsResourceResponse());
        var sut = CreateSut(handler);

        // Act
        await sut.GetFormAsync(new Uri("http://example.org/items"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest.Method);
        var acceptHeaders = handler.LastRequest.Headers.Accept.Select(h => h.MediaType).ToList();
        CollectionAssert.Contains(acceptHeaders, HAL.Common.Constants.MediaTypes.HalForms);
    }

    [TestMethod]
    public async Task GetFormAsync_typed_returns_succeeded_response_with_resource()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkFormsResourceResponse("{\"name\":\"Dave\"}"));
        var sut = CreateSut(handler);

        // Act
        var response = await sut.GetFormAsync<TestState>(new Uri("http://example.org/items"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual("Dave", response.Resource.State.Name);
    }

    // -------------------------------------------------------------------------
    // PostAsync typed overload
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task PostAsync_typed_returns_succeeded_response_with_resource()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse("{\"name\":\"Eve\"}"));
        var sut = CreateSut(handler);

        // Act
        var response = await sut.PostAsync<TestState, TestState>(new Uri("http://example.org/items"), new TestState { Name = "Eve" }, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual("Eve", response.Resource.State.Name);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync typed overload
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task DeleteAsync_typed_returns_succeeded_response()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse("{\"name\":\"Frank\"}"));
        var sut = CreateSut(handler);

        // Act
        var response = await sut.DeleteAsync<TestState>(new Uri("http://example.org/items/1"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual("Frank", response.Resource.State.Name);
    }

    // -------------------------------------------------------------------------
    // SendAsync with custom HTTP method
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task SendAsync_with_PATCH_method_sends_PATCH_request()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(OkResourceResponse());
        var sut = CreateSut(handler);

        // Act
        await sut.SendAsync<TestState>(HttpMethod.Patch, new Uri("http://example.org/items/1"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.AreEqual("http://example.org/items/1", handler.LastRequest.RequestUri!.ToString());
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static HalClient CreateSut(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var factory = new FakeHttpClientFactory(httpClient);
        return new HalClient(factory);
    }

    private static HttpResponseMessage OkResourceResponse(string json = "{}")
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/hal+json")
        };

    private static HttpResponseMessage OkFormsResourceResponse(string json = "{}")
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/hal-forms+json")
        };

    private class TestState
    {
        public string? Name { get; set; }
    }

    /// <summary>
    /// A fake <see cref="HttpMessageHandler"/> that returns a pre-configured
    /// <see cref="HttpResponseMessage"/> and captures the last request for assertions.
    /// The request body is read eagerly before the caller can dispose it.
    /// </summary>
    private class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response = response;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            return _response;
        }
    }

    /// <summary>
    /// A minimal <see cref="IHttpClientFactory"/> that always returns the same
    /// pre-configured <see cref="HttpClient"/>.
    /// </summary>
    private class FakeHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        private readonly HttpClient _client = client;

        public HttpClient CreateClient(string name) => _client;
    }

    public TestContext TestContext { get; set; }
}
