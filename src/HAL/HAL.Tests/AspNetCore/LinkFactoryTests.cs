using HAL.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
    private static LinkFactory CreateSut(string generatedUri = "http://mockuri")
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var linkGeneratorMock = Substitute.For<LinkGenerator>();
        linkGeneratorMock
            .GetUriByAddress(httpContextAccessor.HttpContext, Arg.Any<RouteValuesAddress>(), Arg.Any<RouteValueDictionary>(), Arg.Any<RouteValueDictionary?>(), Arg.Any<string?>(), Arg.Any<HostString?>(), Arg.Any<PathString?>(), Arg.Any<FragmentString>(), Arg.Any<LinkOptions?>())
            .Returns(generatedUri);
        return new LinkFactory(linkGeneratorMock, httpContextAccessor, Substitute.For<IApiDescriptionGroupCollectionProvider>());
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
