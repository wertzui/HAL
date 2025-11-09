using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Tests.Common.Serialization;

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

    [TestMethod]
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
            JsonIgnoreCondition.Never => "{\"foo\":0,\"_embedded\":null,\"_links\":null}",
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
                { "foo", new List<Resource> { new() } }
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
                { "foo", new List<Link> { new("bar") } }
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
                { "foo", new List<Link> { new("bar") } }
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

    [TestMethod]
    public void Resource_with_immutable_state_can_be_deserialized()
    {
        // Arrange
        var expectedResource = new Resource<ImmutableState>
        {
            State = new ImmutableState("Bar")
        };
        var resourceJson = JsonSerializer.Serialize(expectedResource, Constants.DefaultSerializerOptions);

        // Act
        var actualResource = JsonSerializer.Deserialize<Resource<ImmutableState>>(resourceJson, Constants.DefaultSerializerOptions);

        // Assert
        Assert.IsNotNull(actualResource);
        Assert.AreEqual(expectedResource.State, actualResource!.State);
    }

    [TestMethod]
    public void Resource_with_a_state_where_constructor_parameter_and_property_have_the_same_name_with_different_casing_can_be_deserialized()
    {
        // Arrange
        var expectedResource = new Resource<ConstructorAndPropertyStateWithDifferentCasing>
        {
            State = new ConstructorAndPropertyStateWithDifferentCasing("Bar")
        };
        var resourceJson = JsonSerializer.Serialize(expectedResource, Constants.DefaultSerializerOptions);

        // Act
        var actualResource = JsonSerializer.Deserialize<Resource<ConstructorAndPropertyStateWithDifferentCasing>>(resourceJson, Constants.DefaultSerializerOptions);

        // Assert
        Assert.IsNotNull(actualResource);
        Assert.AreEqual(expectedResource.State, actualResource!.State);
    }

    [TestMethod]
    public void Resource_with_a_state_with_one_unusable_constructor_can_be_deserialized()
    {
        // Arrange
        var expectedResource = new Resource<OptionalConstructorState>
        {
            State = new OptionalConstructorState()
        };
        var resourceJson = JsonSerializer.Serialize(expectedResource, Constants.DefaultSerializerOptions);

        // Act
        var actualResource = JsonSerializer.Deserialize<Resource<OptionalConstructorState>>(resourceJson, Constants.DefaultSerializerOptions);

        // Assert
        Assert.IsNotNull(actualResource);
        Assert.AreEqual(expectedResource.State, actualResource!.State);
    }

    [TestMethod]
    public void Resource_with_a_constructor_with_a_nested_record_can_be_deserialized()
    {
        // Arrange
        var expectedResource = new Resource<ImmutableStateWithConstructorAndNestedRecord>
        {
            State = new ImmutableStateWithConstructorAndNestedRecord(42, new("Bar"))
        };
        var resourceJson = JsonSerializer.Serialize(expectedResource, Constants.DefaultSerializerOptions);

        // Act
        var actualResource = JsonSerializer.Deserialize<Resource<ImmutableStateWithConstructorAndNestedRecord>>(resourceJson, Constants.DefaultSerializerOptions);

        // Assert
        Assert.IsNotNull(actualResource);
        Assert.AreEqual(expectedResource.State, actualResource.State);
        Assert.AreEqual(expectedResource.State.NestedState, actualResource.State?.NestedState);
        Assert.AreEqual(expectedResource.State.X, actualResource.State?.X);
    }

    [TestMethod]
    public void Resource_with_a_state_with_private_constructor_throws_a_meaningfull_exception()
    {
        // Arrange
        var expectedResource = new Resource<StateWithPrivateConstructor>
        {
            State = StateWithPrivateConstructor.Create()
        };
        var resourceJson = JsonSerializer.Serialize(expectedResource, Constants.DefaultSerializerOptions);

        // Act
        // Assert
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Resource<StateWithPrivateConstructor>>(resourceJson, Constants.DefaultSerializerOptions));
    }

    private record TestState<T>
    {
        public T? Foo { get; set; }
    }

    private record ImmutableState(string Foo);

    private record ConstructorAndPropertyStateWithDifferentCasing
    {
        public ConstructorAndPropertyStateWithDifferentCasing(string foo)
        {
            Foo = foo;
        }

        public string Foo { get; }
    }

    private record OptionalConstructorState
    {
        public OptionalConstructorState(string? foo)
        {
            Foo = foo ?? "Invalid";
        }

        public OptionalConstructorState()
        {
            Foo = "Valid";
        }

        public string Foo { get; }
    }

    private class StateWithPrivateConstructor
    {
        private StateWithPrivateConstructor()
        {

        }

        public static StateWithPrivateConstructor Create() => new();
    }

    private record ImmutableStateWithConstructorAndNestedRecord
    {
        public ImmutableStateWithConstructorAndNestedRecord(int x, ImmutableState? nestedState = default)
        {
            X = x;
            NestedState = nestedState;
        }

        public int X { get; }
        public ImmutableState? NestedState { get; }
    }
}