using Asp.Versioning;
using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HAL.Tests.AspNetCore;

[TestClass]
public class ResourceFactoryTests
{
    // -------------------------------------------------------------------------
    // Create()
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Create_returns_empty_resource_with_no_links_or_embedded()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());

        // Act
        var result = sut.Create();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Links);
        Assert.IsNull(result.Embedded);
    }

    // -------------------------------------------------------------------------
    // Create<T>(state)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Create_with_state_returns_resource_containing_that_state()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var state = new TestModel { Id = 1, Name = "Test" };

        // Act
        var result = sut.Create(state);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(state, result.State);
    }

    // -------------------------------------------------------------------------
    // CreateForEndpoint
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CreateForEndpoint_returns_resource_with_state_and_self_link()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var state = new TestModel { Id = 1, Name = "Test" };

        // Act
        var result = sut.CreateForEndpoint(state, "Get");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(state, result.State);
        Assert.IsNotNull(result.Links);
        Assert.IsTrue(result.Links.ContainsKey("self"));
    }

    // -------------------------------------------------------------------------
    // CreateForListEndpoint
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CreateForListEndpoint_embeds_all_items_under_given_key()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var models = new[]
        {
            new TestModel { Id = 1, Name = "Test1" },
            new TestModel { Id = 2, Name = "Test2" }
        };

        // Act
        var result = sut.CreateForListEndpoint(models, _ => "items", m => m.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Embedded);
        Assert.IsTrue(result.Embedded.ContainsKey("items"));
        Assert.HasCount(2, result.Embedded["items"]);
    }

    [TestMethod]
    public void CreateForListEndpoint_with_empty_list_returns_resource_without_embedded()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());

        // Act
        var result = sut.CreateForListEndpoint(Array.Empty<TestModel>(), _ => "items", m => m.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Embedded);
    }

    [TestMethod]
    public void CreateForListEndpoint_adds_self_link_to_list_resource()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var models = new[] { new TestModel { Id = 1, Name = "Test" } };

        // Act
        var result = sut.CreateForListEndpoint(models, _ => "items", m => m.Id);

        // Assert
        Assert.IsNotNull(result.Links);
        Assert.IsTrue(result.Links.ContainsKey("self"));
    }

    [TestMethod]
    public void CreateForListEndpoint_embedded_items_have_id_in_self_link_href()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var models = new[] { new TestModel { Id = 42, Name = "Test" } };

        // Act
        var result = sut.CreateForListEndpoint(models, _ => "items", m => m.Id);

        // Assert
        var embedded = result.Embedded!["items"].First();
        Assert.IsNotNull(embedded.Links);
        var selfLink = embedded.Links["self"].First();
        Assert.Contains("42", selfLink.Href);
    }

    [TestMethod]
    public void CreateForListEndpoint_throws_when_resources_is_null()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            sut.CreateForListEndpoint<TestModel, string, int>(null!, _ => "items", m => m.Id));
    }

    [TestMethod]
    public void CreateForListEndpoint_throws_when_keyAccessor_is_null()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            sut.CreateForListEndpoint<TestModel, string, int>([], null!, m => m.Id));
    }

    [TestMethod]
    public void CreateForListEndpoint_throws_when_idAccessor_is_null()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            sut.CreateForListEndpoint<TestModel, string, int>([], _ => "items", null!));
    }

    // -------------------------------------------------------------------------
    // CreateForListEndpointWithPaging
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CreateForListEndpointWithPaging_returns_paged_resource_with_provided_state()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var models = new[]
        {
            new TestModel { Id = 1, Name = "Test1" },
            new TestModel { Id = 2, Name = "Test2" }
        };
        var page = new Page { TotalPages = 5, CurrentPage = 2 };

        // Act
        var result = sut.CreateForListEndpointWithPaging(models, _ => "items", m => m.Id, state: page);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.State);
        Assert.AreEqual(5, result.State.TotalPages);
        Assert.AreEqual(2, result.State.CurrentPage);
    }

    [TestMethod]
    public void CreateForListEndpointWithPaging_embeds_all_items()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());
        var models = new[]
        {
            new TestModel { Id = 1, Name = "Test1" },
            new TestModel { Id = 2, Name = "Test2" }
        };

        // Act
        var result = sut.CreateForListEndpointWithPaging(models, _ => "items", m => m.Id);

        // Assert
        Assert.IsNotNull(result.Embedded);
        Assert.IsTrue(result.Embedded.ContainsKey("items"));
        Assert.HasCount(2, result.Embedded["items"]);
    }

    [TestMethod]
    public void CreateForListEndpointWithPaging_uses_default_Page_when_state_not_provided()
    {
        // Arrange
        var sut = new ResourceFactory(new StubLinkFactory());

        // Act
        var result = sut.CreateForListEndpointWithPaging(Array.Empty<TestModel>(), _ => "items", m => m.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.State);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private class TestModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    /// <summary>
    /// Minimal stub that implements only the ILinkFactory methods called by ResourceFactory,
    /// avoiding the complexity of mocking generic-constrained methods with NSubstitute.
    /// </summary>
    private class StubLinkFactory : ILinkFactory
    {
        public TResource AddSelfLinkTo<TResource>(TResource resource, string? action = null, string? controller = null, object? routeValues = null)
            where TResource : Resource
        {
            resource.AddSelfLink("http://example.org/stub");
            return resource;
        }

        public ICollection<Link> CreateTemplated(string action, string? controller = null, ApiVersion? version = null)
            => [new Link($"http://example.org/{{id}}") { Name = "self" }];

        // Members not exercised by ResourceFactory — throw to make unexpected calls visible.
        public TResource AddFormLinkForExistingLinkTo<TResource>(TResource resource, string existingRel, string? existingName = null, string? action = null, string? controller = null, object? routeValues = null) where TResource : Resource => throw new NotImplementedException();
        public TResource AddSwaggerUiCurieLinkTo<TResource>(TResource resource, string name) where TResource : Resource => throw new NotImplementedException();
        public Link Create(string href) => throw new NotImplementedException();
        public Link Create(string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null) => throw new NotImplementedException();
        public Link Create(string name, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null) => throw new NotImplementedException();
        public Link Create(string name, string href) => throw new NotImplementedException();
        public Link Create(string name, string title, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null) => throw new NotImplementedException();
        public Link Create(string name, string title, string href) => throw new NotImplementedException();
        public IDictionary<string, ICollection<Link>> CreateAllLinks(string? prefix = null, ApiVersion? version = null) => throw new NotImplementedException();
        public ICollection<Link> CreateAllLinksWithoutParameters(ApiVersion? version = null) => throw new NotImplementedException();
        public string GetSelfHref(string? action = null, string? controller = null, object? routeValues = null, QueryString? queryString = null) => throw new NotImplementedException();
        public bool TryCreate([NotNullWhen(true)] out Link? link, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null) => throw new NotImplementedException();
        public bool TryCreate(string? name, [NotNullWhen(true)] out Link? link, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null) => throw new NotImplementedException();
        public bool TryCreate(string? name, string? title, [NotNullWhen(true)] out Link? link, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null) => throw new NotImplementedException();
    }
}
