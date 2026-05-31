using HAL.Client.Net;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HAL.Tests.Client;

[TestClass]
public class HalResponseTests
{
    // -------------------------------------------------------------------------
    // Constructor — resource path
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Constructor_with_resource_and_success_status_sets_properties()
    {
        // Arrange
        var resource = new Resource();

        // Act
        var response = new HalResponse<Resource>(resource, HttpStatusCode.OK);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.AreSame(resource, response.Resource);
        Assert.IsNull(response.ProblemDetails);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void Constructor_with_resource_and_error_status_throws()
    {
        // Arrange
        var resource = new Resource();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new HalResponse<Resource>(resource, HttpStatusCode.NotFound));
    }

    [TestMethod]
    public void Constructor_with_null_resource_throws()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new HalResponse<Resource>((Resource)null!, HttpStatusCode.OK));
    }

    // -------------------------------------------------------------------------
    // Constructor — problem details path
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Constructor_with_problem_details_and_error_status_sets_properties()
    {
        // Arrange
        var problemDetails = new ProblemDetails { Title = "Not found", Status = 404 };

        // Act
        var response = new HalResponse<Resource>(problemDetails, HttpStatusCode.NotFound);

        // Assert
        Assert.IsFalse(response.Succeeded);
        Assert.AreSame(problemDetails, response.ProblemDetails);
        Assert.IsNull(response.Resource);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public void Constructor_with_problem_details_and_success_status_throws()
    {
        // Arrange
        var problemDetails = new ProblemDetails { Title = "Oops", Status = 200 };

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new HalResponse<Resource>(problemDetails, HttpStatusCode.OK));
    }

    [TestMethod]
    public void Constructor_with_null_problem_details_throws()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new HalResponse<Resource>((ProblemDetails)null!, HttpStatusCode.BadRequest));
    }

    // -------------------------------------------------------------------------
    // Succeeded
    // -------------------------------------------------------------------------

    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.Created)]
    [DataRow(HttpStatusCode.NoContent)]
    public void Succeeded_is_true_for_2xx_status_codes(HttpStatusCode statusCode)
    {
        // Arrange
        var response = new HalResponse<Resource>(new Resource(), statusCode);

        // Assert
        Assert.IsTrue(response.Succeeded);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public void Succeeded_is_false_for_non_2xx_status_codes(HttpStatusCode statusCode)
    {
        // Arrange
        var response = new HalResponse<Resource>(new ProblemDetails(), statusCode);

        // Assert
        Assert.IsFalse(response.Succeeded);
    }

    // -------------------------------------------------------------------------
    // EnsureSuccessStatusCode
    // -------------------------------------------------------------------------

    [TestMethod]
    public void EnsureSuccessStatusCode_does_not_throw_when_request_succeeded()
    {
        // Arrange
        var response = new HalResponse<Resource>(new Resource(), HttpStatusCode.OK);

        // Act & Assert — no exception expected
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public void EnsureSuccessStatusCode_throws_HttpRequestException_when_request_failed()
    {
        // Arrange
        var response = new HalResponse<Resource>(new ProblemDetails { Title = "Error" }, HttpStatusCode.BadRequest);

        // Act & Assert
        Assert.ThrowsExactly<HttpRequestException>(() => response.EnsureSuccessStatusCode());
    }

    // -------------------------------------------------------------------------
    // FromHttpResponse — success
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task FromHttpResponse_returns_succeeded_response_with_resource_for_200_with_valid_json()
    {
        // Arrange
        var json = "{\"_links\":{\"self\":[{\"href\":\"http://example.org\"}]}}";
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/hal+json")
        };

        // Act
        var response = await HalResponse.FromHttpResponse<Resource>(httpResponse, TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.IsNotNull(response.Resource);
        Assert.IsNotNull(response.Resource.Links);
    }

    [TestMethod]
    public async Task FromHttpResponse_returns_succeeded_response_with_typed_state_for_200()
    {
        // Arrange
        var json = "{\"name\":\"Alice\"}";
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/hal+json")
        };

        // Act
        var response = await HalResponse.FromHttpResponse<Resource<TestState>>(httpResponse, TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(response.Succeeded);
        Assert.IsNotNull(response.Resource?.State);
        Assert.AreEqual("Alice", response.Resource.State.Name);
    }

    // -------------------------------------------------------------------------
    // FromHttpResponse — failure
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task FromHttpResponse_returns_failed_response_with_problem_details_for_4xx()
    {
        // Arrange
        var json = "{\"title\":\"Not Found\",\"status\":404}";
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/problem+json")
        };

        // Act
        var response = await HalResponse.FromHttpResponse<Resource>(httpResponse, TestContext.CancellationToken);

        // Assert
        Assert.IsFalse(response.Succeeded);
        Assert.IsNotNull(response.ProblemDetails);
        Assert.AreEqual("Not Found", response.ProblemDetails.Title);
    }

    [TestMethod]
    public async Task FromHttpResponse_returns_failed_response_with_fallback_problem_details_for_non_json_4xx()
    {
        // Arrange
        using var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain")
        };

        // Act
        var response = await HalResponse.FromHttpResponse<Resource>(httpResponse, TestContext.CancellationToken);

        // Assert
        Assert.IsFalse(response.Succeeded);
        Assert.IsNotNull(response.ProblemDetails);
    }

    private class TestState
    {
        public string? Name { get; set; }
    }

    public TestContext TestContext { get; set; }
}
