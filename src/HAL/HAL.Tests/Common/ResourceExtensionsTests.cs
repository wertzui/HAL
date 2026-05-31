using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HAL.Tests.Common;

[TestClass]
public class ResourceExtensionsTests
{
    // -------------------------------------------------------------------------
    // AddLink(string rel, Link)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddLink_with_rel_adds_link_to_links_collection()
    {
        // Arrange
        var resource = new Resource();
        var link = new Link("http://example.org");

        // Act
        resource.AddLink("self", link);

        // Assert
        Assert.IsNotNull(resource.Links);
        Assert.IsTrue(resource.Links.ContainsKey("self"));
        Assert.HasCount(1, resource.Links["self"]);
    }

    [TestMethod]
    public void AddLink_multiple_links_to_same_rel_accumulates_links()
    {
        // Arrange
        var resource = new Resource();

        // Act
        resource.AddLink("items", new Link("http://example.org/1"));
        resource.AddLink("items", new Link("http://example.org/2"));

        // Assert
        Assert.HasCount(2, resource.Links!["items"]);
    }

    [TestMethod]
    public void AddLink_returns_same_resource_instance_for_chaining()
    {
        // Arrange
        var resource = new Resource();

        // Act
        var returned = resource.AddLink("self", new Link("http://example.org"));

        // Assert
        Assert.AreSame(resource, returned);
    }

    // -------------------------------------------------------------------------
    // AddLink(Link) — uses link.Name as rel
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddLink_by_name_uses_link_name_as_rel()
    {
        // Arrange
        var resource = new Resource();
        var link = new Link("http://example.org") { Name = "items" };

        // Act
        resource.AddLink(link);

        // Assert
        Assert.IsNotNull(resource.Links);
        Assert.IsTrue(resource.Links.ContainsKey("items"));
    }

    [TestMethod]
    public void AddLink_by_name_throws_when_link_name_is_null()
    {
        // Arrange
        var resource = new Resource();
        var link = new Link("http://example.org"); // Name is null

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => resource.AddLink(link));
    }

    // -------------------------------------------------------------------------
    // AddEmbedded
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddEmbedded_single_adds_resource_to_embedded_collection()
    {
        // Arrange
        var resource = new Resource();
        var embedded = new Resource();

        // Act
        resource.AddEmbedded("items", embedded);

        // Assert
        Assert.IsNotNull(resource.Embedded);
        Assert.IsTrue(resource.Embedded.ContainsKey("items"));
        Assert.HasCount(1, resource.Embedded["items"]);
    }

    [TestMethod]
    public void AddEmbedded_multiple_items_to_same_key_accumulates_resources()
    {
        // Arrange
        var resource = new Resource();

        // Act
        resource.AddEmbedded("items", new Resource());
        resource.AddEmbedded("items", new Resource());

        // Assert
        Assert.HasCount(2, resource.Embedded!["items"]);
    }

    [TestMethod]
    public void AddEmbedded_from_collection_adds_all_resources_under_correct_keys()
    {
        // Arrange
        var resource = new Resource();
        var items = new[]
        {
            new TestItem { Key = "a", Value = 1 },
            new TestItem { Key = "b", Value = 2 }
        };

        // Act
        resource.AddEmbedded(items, i => i.Key, i => new Resource<int> { State = i.Value });

        // Assert
        Assert.IsNotNull(resource.Embedded);
        Assert.HasCount(2, resource.Embedded);
        Assert.IsTrue(resource.Embedded.ContainsKey("a"));
        Assert.IsTrue(resource.Embedded.ContainsKey("b"));
    }

    // -------------------------------------------------------------------------
    // AddSelfLink / GetSelfLink
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddSelfLink_adds_link_under_self_rel()
    {
        // Arrange
        var resource = new Resource();

        // Act
        resource.AddSelfLink("http://example.org/self");

        // Assert
        Assert.IsNotNull(resource.Links);
        Assert.IsTrue(resource.Links.ContainsKey("self"));
        Assert.AreEqual("http://example.org/self", resource.Links["self"].First().Href);
    }

    [TestMethod]
    public void GetSelfLink_returns_the_self_link()
    {
        // Arrange
        var resource = new Resource();
        resource.AddSelfLink("http://example.org/self");

        // Act
        var selfLink = resource.GetSelfLink();

        // Assert
        Assert.IsNotNull(selfLink);
        Assert.AreEqual("http://example.org/self", selfLink.Href);
    }

    [TestMethod]
    public void GetSelfLink_throws_when_no_self_link_exists()
    {
        // Arrange
        var resource = new Resource();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => resource.GetSelfLink());
    }

    // -------------------------------------------------------------------------
    // AddCurieLink
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddCurieLink_adds_link_under_curies_rel_with_correct_name()
    {
        // Arrange
        var resource = new Resource();

        // Act
        resource.AddCurieLink("acme", "http://docs.example.org/{rel}");

        // Assert
        Assert.IsNotNull(resource.Links);
        Assert.IsTrue(resource.Links.ContainsKey("curies"));
        var curie = resource.Links["curies"].First();
        Assert.AreEqual("acme", curie.Name);
        Assert.IsTrue(curie.Templated);
    }

    [TestMethod]
    public void AddCurieLink_throws_when_href_is_missing_rel_placeholder()
    {
        // Arrange
        var resource = new Resource();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            resource.AddCurieLink("acme", "http://docs.example.org/no-placeholder"));
    }

    // -------------------------------------------------------------------------
    // ChangeStateTo
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ChangeStateTo_creates_new_resource_with_new_state()
    {
        // Arrange
        var resource = new Resource<string> { State = "old" };
        resource.AddSelfLink("http://example.org/self");

        // Act
        var newResource = resource.ChangeStateTo(42);

        // Assert
        Assert.AreEqual(42, newResource.State);
    }

    [TestMethod]
    public void ChangeStateTo_preserves_links_from_original_resource()
    {
        // Arrange
        var resource = new Resource<string> { State = "old" };
        resource.AddSelfLink("http://example.org/self");

        // Act
        var newResource = resource.ChangeStateTo(42);

        // Assert
        Assert.IsNotNull(newResource.Links);
        Assert.IsTrue(newResource.Links.ContainsKey("self"));
    }

    [TestMethod]
    public void ChangeStateTo_preserves_embedded_from_original_resource()
    {
        // Arrange
        var resource = new Resource<string> { State = "old" };
        resource.AddEmbedded("items", new Resource());

        // Act
        var newResource = resource.ChangeStateTo(42);

        // Assert
        Assert.IsNotNull(newResource.Embedded);
        Assert.IsTrue(newResource.Embedded.ContainsKey("items"));
    }

    // -------------------------------------------------------------------------
    // RemoveState
    // -------------------------------------------------------------------------

    [TestMethod]
    public void RemoveState_returns_base_resource_not_typed_resource()
    {
        // Arrange
        var resource = new Resource<string> { State = "some state" };

        // Act
        var result = resource.RemoveState();

        // Assert
        Assert.IsNotInstanceOfType<Resource<string>>(result);
    }

    [TestMethod]
    public void RemoveState_preserves_links()
    {
        // Arrange
        var resource = new Resource<string> { State = "some state" };
        resource.AddSelfLink("http://example.org/self");

        // Act
        var result = resource.RemoveState();

        // Assert
        Assert.IsNotNull(result.Links);
        Assert.IsTrue(result.Links.ContainsKey("self"));
    }

    // -------------------------------------------------------------------------
    // CastState
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CastState_from_untyped_resource_returns_resource_with_null_state()
    {
        // Arrange
        var resource = new Resource();
        resource.AddSelfLink("http://example.org");

        // Act
        var casted = resource.CastState<string>();

        // Assert
        Assert.IsNull(casted.State);
        Assert.IsNotNull(casted.Links);
    }

    // -------------------------------------------------------------------------
    // AddLinks from dictionary
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddLinks_from_dictionary_adds_all_links()
    {
        // Arrange
        var resource = new Resource();
        var source = new Dictionary<string, ICollection<Link>>
        {
            { "items", [new Link("http://example.org/1")] },
            { "related", [new Link("http://example.org/2")] }
        };

        // Act
        resource.AddLinks(source);

        // Assert
        Assert.IsNotNull(resource.Links);
        Assert.HasCount(2, resource.Links);
        Assert.IsTrue(resource.Links.ContainsKey("items"));
        Assert.IsTrue(resource.Links.ContainsKey("related"));
    }

    private class TestItem
    {
        public required string Key { get; set; }
        public int Value { get; set; }
    }
}
