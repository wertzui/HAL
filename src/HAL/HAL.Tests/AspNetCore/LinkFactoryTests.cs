using Asp.Versioning;
using HAL.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Linq;

namespace HAL.Tests.AspNetCore;

[TestClass]
public class LinkFactoryTests
{
    /// <summary>
    /// Creates a <see cref="LinkFactory"/> whose underlying <see cref="LinkGenerator"/> always
    /// returns <paramref name="generatedUri"/> regardless of the route values passed.
    /// </summary>
    private static LinkFactory CreateSut(string generatedUri = "http://mockuri", IApiDescriptionGroupCollectionProvider? apiExplorer = null)
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var linkGeneratorMock = Substitute.For<LinkGenerator>();
        linkGeneratorMock
            .GetUriByAddress(httpContextAccessor.HttpContext, Arg.Any<RouteValuesAddress>(), Arg.Any<RouteValueDictionary>(), Arg.Any<RouteValueDictionary?>(), Arg.Any<string?>(), Arg.Any<HostString?>(), Arg.Any<PathString?>(), Arg.Any<FragmentString>(), Arg.Any<LinkOptions?>())
            .Returns(generatedUri);
        return new LinkFactory(linkGeneratorMock, httpContextAccessor, apiExplorer ?? Substitute.For<IApiDescriptionGroupCollectionProvider>());
    }

    private static ApiDescription CreateApiDescription(string controllerName, string actionName, string routeTemplate, ApiVersion version)
    {
        var description = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = controllerName,
                ActionName = actionName,
                EndpointMetadata = [new RouteAttribute(routeTemplate)],
                Parameters = []
            },
            HttpMethod = "GET"
        };
        description.SetProperty(version);
        return description;
    }

    private static IApiDescriptionGroupCollectionProvider CreateApiExplorer(params ApiDescriptionGroup[] groups)
    {
        var apiExplorer = Substitute.For<IApiDescriptionGroupCollectionProvider>();
        apiExplorer.ApiDescriptionGroups.Returns(new ApiDescriptionGroupCollection(groups, 1));
        return apiExplorer;
    }

    [TestMethod]
    public void CreateTemplated_without_version_finds_controller_that_only_exists_in_an_older_version()
    {
        // Arrange – "Foo" only exists in v1 while "Bar" only exists in v2 (the last group).
        var apiExplorer = CreateApiExplorer(
            new ApiDescriptionGroup("v1", [CreateApiDescription("Foo", "Get", "v1/Foo", new ApiVersion(1))]),
            new ApiDescriptionGroup("v2", [CreateApiDescription("Bar", "Get", "v2/Bar", new ApiVersion(2))]));
        var sut = CreateSut(apiExplorer: apiExplorer);

        // Act
        var links = sut.CreateTemplated("Get", "Foo", version: null);

        // Assert
        Assert.HasCount(1, links);
        Assert.Contains("v1/Foo", links.First().Href);
    }

    [TestMethod]
    public void CreateTemplated_without_version_uses_latest_version_of_the_endpoint()
    {
        // Arrange – "Foo" exists in v1 and v2.
        var apiExplorer = CreateApiExplorer(
            new ApiDescriptionGroup("v1", [CreateApiDescription("Foo", "Get", "v1/Foo", new ApiVersion(1))]),
            new ApiDescriptionGroup("v2", [CreateApiDescription("Foo", "Get", "v2/Foo", new ApiVersion(2))]));
        var sut = CreateSut(apiExplorer: apiExplorer);

        // Act
        var links = sut.CreateTemplated("Get", "Foo", version: null);

        // Assert
        Assert.HasCount(1, links);
        Assert.Contains("v2/Foo", links.First().Href);
    }

    [TestMethod]
    public void CreateTemplated_with_version_only_returns_endpoints_of_that_version()
    {
        // Arrange – "Foo" exists in v1 and v2.
        var apiExplorer = CreateApiExplorer(
            new ApiDescriptionGroup("v1", [CreateApiDescription("Foo", "Get", "v1/Foo", new ApiVersion(1))]),
            new ApiDescriptionGroup("v2", [CreateApiDescription("Foo", "Get", "v2/Foo", new ApiVersion(2))]));
        var sut = CreateSut(apiExplorer: apiExplorer);

        // Act
        var links = sut.CreateTemplated("Get", "Foo", new ApiVersion(1));

        // Assert
        Assert.HasCount(1, links);
        Assert.Contains("v1/Foo", links.First().Href);
    }

    [TestMethod]
    public void CreateTemplated_with_null_collection_parameter_appends_explode_modifier()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var link = sut.CreateTemplated("action", "controller", new { collection = null as int[] }, "http", "host");

        // Assert
        Assert.AreEqual("http://mockuri/{?collection*}", link.Href);
    }

    [TestMethod]
    public void CreateTemplated_with_null_scalar_parameter_appends_templated_query()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var link = sut.CreateTemplated("action", "controller", new { id = (int?)null }, "http", "host");

        // Assert
        Assert.AreEqual("http://mockuri/{?id}", link.Href);
    }

    [TestMethod]
    public void CreateTemplated_with_concrete_value_appends_concrete_query_parameter()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var link = sut.CreateTemplated("action", "controller", new { id = 42 }, "http", "host");

        // Assert
        Assert.Contains("id=42", link.Href);
    }

    [TestMethod]
    public void CreateTemplated_with_null_values_returns_non_templated_link()
    {
        // Arrange
        var sut = CreateSut("http://mockuri/items");

        // Act
        var link = sut.CreateTemplated("GetList", "Items", null, "http", "host");

        // Assert
        Assert.IsFalse(link.Templated);
        Assert.AreEqual("http://mockuri/items", link.Href);
    }

    [TestMethod]
    public void Create_with_href_returns_link_with_given_href()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var link = sut.Create("http://example.org/custom");

        // Assert
        Assert.AreEqual("http://example.org/custom", link.Href);
    }

    [TestMethod]
    public void Create_with_name_and_href_returns_link_with_name_and_href()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var link = sut.Create("myName", "http://example.org/custom");

        // Assert
        Assert.AreEqual("myName", link.Name);
        Assert.AreEqual("http://example.org/custom", link.Href);
    }

    [TestMethod]
    public void Create_with_name_title_and_href_returns_link_with_all_properties()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var link = sut.Create("myName", "My Title", "http://example.org/custom");

        // Assert
        Assert.AreEqual("myName", link.Name);
        Assert.AreEqual("My Title", link.Title);
        Assert.AreEqual("http://example.org/custom", link.Href);
    }

    [TestMethod]
    public void TryCreate_returns_true_and_link_when_generator_returns_uri()
    {
        // Arrange
        var sut = CreateSut("http://mockuri/resource");

        // Act
        var result = sut.TryCreate(out var link, "action", "controller");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(link);
        Assert.AreEqual("http://mockuri/resource", link.Href);
    }

    [TestMethod]
    public void TryCreate_with_name_propagates_name_to_link()
    {
        // Arrange
        var sut = CreateSut("http://mockuri/resource");

        // Act
        var result = sut.TryCreate("namedLink", out var link);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(link);
        Assert.AreEqual("namedLink", link.Name);
        Assert.AreEqual("http://mockuri/resource", link.Href);
    }

    [TestMethod]
    public void AddSelfLinkTo_adds_self_link_with_correct_href()
    {
        // Arrange
        var sut = CreateSut("http://mockuri/self");
        var resource = new HAL.Common.Resource();

        // Act
        sut.AddSelfLinkTo(resource);

        // Assert – AddSelfLinkTo calls GetUriByAddress which our mock returns "http://mockuri/self"
        Assert.IsNotNull(resource.Links);
        Assert.IsTrue(resource.Links.ContainsKey("self"));
        var selfLink = resource.Links["self"].First();
        Assert.AreEqual("http://mockuri/self", selfLink.Href);
    }

    [TestMethod]
    public void AddSelfLinkTo_throws_for_null_resource()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => sut.AddSelfLinkTo<HAL.Common.Resource>(null!));
    }

    [TestMethod]
    public void AddFormLinkForExistingLinkTo_throws_when_rel_not_present()
    {
        // Arrange
        var sut = CreateSut();
        var resource = new HAL.Common.Resource();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            sut.AddFormLinkForExistingLinkTo(resource, "nonexistent"));
    }

    [TestMethod]
    public void AddFormLinkForExistingLinkTo_throws_for_null_resource()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            sut.AddFormLinkForExistingLinkTo<HAL.Common.Resource>(null!, "self"));
    }
}
