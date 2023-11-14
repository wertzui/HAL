using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace HAL.Tests.Common.Converters;

[TestClass]
public class LinkSerializationTests
{
    private const string singleLinkJson = "{\"deprecation\":\"Deprecation\",\"href\":\"Href\",\"hreflang\":\"Hreflang\",\"name\":\"Name\",\"profile\":\"Profile\",\"templated\":true,\"title\":\"Title\",\"type\":\"Type\"}";

    private static readonly Link singleLink = new("Href")
    {
        Deprecation = "Deprecation",
        Hreflang = "Hreflang",
        Name = "Name",
        Profile = "Profile",
        Templated = true,
        Title = "Title",
        Type = "Type",
    };

    [TestMethod]
    public void Link_can_be_deserialized()
    {
        // Arrange

        // Act
        var actualLink = JsonSerializer.Deserialize<Link>(singleLinkJson, Constants.DefaultSerializerOptions);

        // Assert
        Assert.IsNotNull(actualLink);
        Assert.AreEqual(singleLink.Deprecation, actualLink!.Deprecation);
        Assert.AreEqual(singleLink.Href, actualLink.Href);
        Assert.AreEqual(singleLink.Hreflang, actualLink.Hreflang);
        Assert.AreEqual(singleLink.Name, actualLink.Name);
        Assert.AreEqual(singleLink.Profile, actualLink.Profile);
        Assert.AreEqual(singleLink.Templated, actualLink.Templated);
        Assert.AreEqual(singleLink.Title, actualLink.Title);
        Assert.AreEqual(singleLink.Type, actualLink.Type);
    }

    [TestMethod]
    public void Link_can_be_serialized()
    {
        // Arrange

        // Act
        var actualJson = JsonSerializer.Serialize(singleLink, Constants.DefaultSerializerOptions);

        // Assert
        Assert.AreEqual(singleLinkJson, actualJson);
    }
}