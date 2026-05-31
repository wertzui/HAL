using HAL.Client.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAL.Tests.Integration;

/// <summary>
/// End-to-end integration tests that start a real in-process ASP.NET Core host,
/// register AddHAL(), and drive requests through the real <see cref="HalClient"/>.
/// This validates the full serialization/deserialization pipeline that unit tests
/// with fake handlers cannot cover.
/// </summary>
[TestClass]
public class HalIntegrationTests
{
    // -------------------------------------------------------------------------
    // GET /api/items  — collection
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_returns_hal_resource_with_self_link()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var sut = CreateHalClient(app);

        // Act
        var response = await sut.GetAsync(new Uri("http://localhost/api/items"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded, $"Unexpected status {response.StatusCode}");
        Assert.IsNotNull(response.Resource);
        Assert.IsNotNull(response.Resource.Links, "Resource should contain links");
        Assert.IsTrue(response.Resource.Links.ContainsKey("self"), "Resource should contain a 'self' link");
        // Items are embedded under the 'items' key
        Assert.IsNotNull(response.Resource.Embedded, "Resource should contain embedded items");
        Assert.IsTrue(response.Resource.Embedded.ContainsKey("items"), "Embedded should contain 'items'");
    }

    [TestMethod]
    public async Task GetAsync_returns_hal_content_type_from_server()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var httpClient = app.GetTestClient();

        // Act
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/hal+json");
        var response = await httpClient.GetAsync("/api/items", TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/hal+json", response.Content.Headers.ContentType?.MediaType);
    }

    // -------------------------------------------------------------------------
    // GET /api/items/{id}  — single item
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task GetAsync_typed_returns_item_with_self_link()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var sut = CreateHalClient(app);

        // Act
        var response = await sut.GetAsync<ItemsController.ItemDto>(new Uri("http://localhost/api/items/1"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded, $"Unexpected status {response.StatusCode}");
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual(1, response.Resource.State.Id);
        Assert.AreEqual("Apple", response.Resource.State.Name);
        Assert.IsTrue(response.Resource.Links?.ContainsKey("self") ?? false, "Item resource should have a 'self' link");
    }

    [TestMethod]
    public async Task GetAsync_returns_not_found_for_missing_item()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var sut = CreateHalClient(app);

        // Act
        var response = await sut.GetAsync<ItemsController.ItemDto>(new Uri("http://localhost/api/items/999"), cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsFalse(response.Succeeded);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.IsNotNull(response.ProblemDetails);
    }

    // -------------------------------------------------------------------------
    // POST /api/items  — create
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task PostAsync_creates_item_and_returns_created_resource()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var sut = CreateHalClient(app);
        var newItem = new ItemsController.ItemDto(42, "Durian");

        // Act
        var response = await sut.PostAsync<ItemsController.ItemDto, ItemsController.ItemDto>(
            new Uri("http://localhost/api/items"), newItem, cancellationToken: TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded, $"Unexpected status {response.StatusCode}");
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual(42, response.Resource.State.Id);
        Assert.AreEqual("Durian", response.Resource.State.Name);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/items/{id}  — delete
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task DeleteAsync_removes_item_and_returns_no_content()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.DeleteAsync("/api/items/1", TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteAsync_returns_not_found_for_missing_item()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.DeleteAsync("/api/items/999", TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Content negotiation
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task Request_without_accept_header_returns_hal_json()
    {
        // Arrange
        await using var app = BuildApp();
        await app.StartAsync(TestContext.CancellationToken);
        var httpClient = app.GetTestClient();

        // Act
        var response = await httpClient.GetAsync("/api/items", TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // The controller declares [Produces("application/hal+json")] so it always returns HAL JSON
        Assert.AreEqual("application/hal+json", response.Content.Headers.ContentType?.MediaType);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a minimal ASP.NET Core <see cref="WebApplication"/> with HAL registered
    /// and the <see cref="ItemsController"/> as an application part.
    /// The caller is responsible for starting and disposing the app.
    /// </summary>
    private static WebApplication BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddHttpContextAccessor();
        var mvcBuilder = builder.Services.AddControllers();
        mvcBuilder.AddHAL();
        mvcBuilder.PartManager.ApplicationParts.Add(
            new AssemblyPart(typeof(ItemsController).Assembly));

        var app = builder.Build();
        app.MapControllers();
        return app;
    }

    private static HalClient CreateHalClient(WebApplication app)
    {
        var httpClient = app.GetTestClient();
        return new HalClient(new SingletonHttpClientFactory(httpClient));
    }

    private class SingletonHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    public TestContext TestContext { get; set; }
}

