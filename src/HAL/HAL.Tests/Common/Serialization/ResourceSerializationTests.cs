using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;

namespace HAL.Tests.Common.Serialization
{
    [TestClass]
    public class ResourceSerializationTests
    {
        [TestMethod]
        public void Empty_resource_can_be_deserialized()
        {
            // Arrange
            var expectedResource = new Resource();
            var resourceJson = "{}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource>(resourceJson, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResource, actualResource);
        }

        [TestMethod]
        public void Empty_resource_can_be_serialized()
        {
            // Arrange
            var resource = new Resource();
            var expectedResourceJson = "{}";

            // Act
            var actualResourceJson = JsonSerializer.Serialize(resource, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResourceJson, actualResourceJson);
        }

        [TestMethod]
        public void Resource_with_embedded_can_be_deserialized()
        {
            // Arrange
            var expectedResource = new Resource
            {
                Embedded = new Dictionary<string, ICollection<Resource>>
                {
                    { "foo", new List<Resource> { /*new Resource()*/ } }
                }
            };
            var resourceJson = "{\"_embedded\":{\"foo\":[{}]}}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource>(resourceJson, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResource, actualResource);
        }

        [TestMethod]
        public void Resource_with_embedded_can_be_serialized()
        {
            // Arrange
            var resource = new Resource
            {
                Embedded = new Dictionary<string, ICollection<Resource>>
                {
                    { "foo", new List<Resource> { new Resource() } }
                }
            };
            var expectedResourceJson = "{\"_embedded\":{\"foo\":[{}]}}";

            // Act
            var actualResourceJson = JsonSerializer.Serialize(resource, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResourceJson, actualResourceJson);
        }

        [TestMethod]
        public void Resource_with_link_can_be_deserialized()
        {
            // Arrange
            var expectedResource = new Resource
            {
                Links = new Dictionary<string, ICollection<Link>>
                {
                    { "foo", new List<Link> { new Link { Href = "bar" } } }
                }
            };
            var resourceJson = "{\"_links\":{\"foo\":[{\"href\":\"bar\"}]}}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource>(resourceJson, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResource, actualResource);
        }

        [TestMethod]
        public void Resource_with_link_can_be_serialized()
        {
            // Arrange
            var resource = new Resource
            {
                Links = new Dictionary<string, ICollection<Link>>
                {
                    { "foo", new List<Link> { new Link { Href = "bar" } } }
                }
            };
            var expectedResourceJson = "{\"_links\":{\"foo\":[{\"href\":\"bar\"}]}}";

            // Act
            var actualResourceJson = JsonSerializer.Serialize(resource, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResourceJson, actualResourceJson);
        }

        [TestMethod]
        public void Resource_with_state_can_be_deserialized()
        {
            // Arrange
            var expectedResource = new Resource<TestState>
            {
                State = new TestState { Foo = "Bar" }
            };
            var resourceJson = "{\"foo\":\"Bar\"}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource<TestState>>(resourceJson, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResource, actualResource);
        }

        [TestMethod]
        public void Resource_with_state_can_be_deserialized_without_type_parameter()
        {
            // Arrange
            var expectedResource = new Resource<TestState>
            {
                State = new TestState { Foo = "Bar" }
            };
            var resourceJson = "{\"foo\":\"Bar\"}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource>(resourceJson, Constants.DefaultSerializerOptions);
            var dynamicResource = actualResource as Resource<dynamic>;

            // Assert
            Assert.AreEqual(expectedResource.State.Foo, dynamicResource.State.foo);
        }

        [TestMethod]
        public void Resource_with_state_can_be_serialized()
        {
            // Arrange

            var resource = new Resource<TestState>
            {
                State = new TestState { Foo = "Bar" }
            };
            var expectedResourceJson = "{\"foo\":\"Bar\"}";

            // Act
            var actualResourceJson = JsonSerializer.Serialize(resource, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResourceJson, actualResourceJson);
        }

        private record TestState
        {
            public string Foo { get; set; }
        }
    }
}