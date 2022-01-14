using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Tests.Common.Serialization
{
    [TestClass]
    public class ResourceSerializationTests
    {
        private enum TestEnum
        {
            One,
            Two
        }

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

        [DataTestMethod]
        [DataRow(JsonIgnoreCondition.Never)]
        [DataRow(JsonIgnoreCondition.WhenWritingDefault)]
        [DataRow(JsonIgnoreCondition.WhenWritingNull)]
        public void Options_are_taken_into_account_when_writing_non_nullable_default_values(JsonIgnoreCondition ignoreCondition)
        {
            // Arrange
            var resource = new Resource<TestState<int>>
            {
                State = new TestState<int> { Foo = default }
            };
            var expectedResourceJson = ignoreCondition switch
            {
                JsonIgnoreCondition.Never => "{\"foo\":0}",
                JsonIgnoreCondition.WhenWritingDefault => "{}",
                JsonIgnoreCondition.WhenWritingNull => "{\"foo\":0}",
                _ => throw new System.Exception("Unhandled condition.")
            };
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = ignoreCondition };

            // Act
            var actualResourceJson = JsonSerializer.Serialize(resource, options);

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
        public void Resource_with_enum_can_be_serialized_with_enum_as_string()
        {
            // Arrange

            var resource = new Resource<TestState<TestEnum>>
            {
                State = new TestState<TestEnum> { Foo = TestEnum.Two }
            };
            var expectedResourceJson = "{\"foo\":\"two\"}";

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
                    { "foo", new List<Link> { new Link("bar") } }
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
                    { "foo", new List<Link> { new Link("bar") } }
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
            var expectedResource = new Resource<TestState<string>>
            {
                State = new TestState<string> { Foo = "Bar" }
            };
            var resourceJson = "{\"foo\":\"Bar\"}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource<TestState<string>>>(resourceJson, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResource, actualResource);
        }

        [TestMethod]
        public void Resource_with_state_can_be_deserialized_without_type_parameter()
        {
            // Arrange
            var expectedResource = new Resource<TestState<string>>
            {
                State = new TestState<string> { Foo = "Bar" }
            };
            var resourceJson = "{\"foo\":\"Bar\"}";

            // Act
            var actualResource = JsonSerializer.Deserialize<Resource>(resourceJson, Constants.DefaultSerializerOptions);
            var dynamicResource = actualResource as Resource<dynamic>;

            // Assert
            Assert.AreEqual(expectedResource.State.Foo, dynamicResource?.State?.foo);
        }

        [TestMethod]
        public void Resource_with_state_can_be_serialized()
        {
            // Arrange

            var resource = new Resource<TestState<string>>
            {
                State = new TestState<string> { Foo = "Bar" }
            };
            var expectedResourceJson = "{\"foo\":\"Bar\"}";

            // Act
            var actualResourceJson = JsonSerializer.Serialize(resource, Constants.DefaultSerializerOptions);

            // Assert
            Assert.AreEqual(expectedResourceJson, actualResourceJson);
        }

        private record TestState<T>
        {
            public T? Foo { get; set; }
        }
    }
}