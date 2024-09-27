using HAL.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Tests.Common.Converters
{
    [TestClass]
    public class JsonEnumConverterTests
    {
        [TestMethod]
        public void Flags_Enum_should_be_serialized_as_JSON_array_when_no_serialization_method_is_defined()
        {
            // Arrange
            var flags = TestEnum.Flag1 | TestEnum.Flag2;
            var expected = "[\"Flag1\",\"Flag2\"]";
            var converter = new JsonEnumConverter();
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(converter);

            // Act
            var actual = JsonSerializer.Serialize(flags, options);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Flags_Enum_should_be_serialized_as_JSON_array_when_serialization_method_is_defined_as_array()
        {
            // Arrange
            var flags = TestEnum.Flag1 | TestEnum.Flag2;
            var expected = "[\"Flag1\",\"Flag2\"]";
            var converter = new JsonEnumConverter(new JsonEnumConverterOptions { JsonFlagsEnumSerializationHandling = HAL.Common.JsonFlagsEnumSerializationHandling.Array });
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(converter);

            // Act
            var actual = JsonSerializer.Serialize(flags, options);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Flags_Enum_should_be_serialized_as_JSON_string_when_serialization_method_is_defined_as_string()
        {
            // Arrange
            var flags = TestEnum.Flag1 | TestEnum.Flag2;
            var expected = "\"Flag1, Flag2\"";
            var converter = new JsonEnumConverter(new JsonEnumConverterOptions { JsonFlagsEnumSerializationHandling = HAL.Common.JsonFlagsEnumSerializationHandling.String });
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(converter);

            // Act
            var actual = JsonSerializer.Serialize(flags, options);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("\"Flag1, Flag2\"", null, DisplayName = "JSON string and null")]
        [DataRow("[\"Flag1\",\"Flag2\"]", null, DisplayName = "JSON array and null")]
        [DataRow("\"Flag1, Flag2\"", JsonFlagsEnumSerializationHandling.Array, DisplayName = "JSON string and array handling")]
        [DataRow("[\"Flag1\",\"Flag2\"]", JsonFlagsEnumSerializationHandling.Array, DisplayName = "JSON array and array handling")]
        [DataRow("\"Flag1, Flag2\"", JsonFlagsEnumSerializationHandling.String, DisplayName = "JSON string and string handling")]
        [DataRow("[\"Flag1\",\"Flag2\"]", JsonFlagsEnumSerializationHandling.String, DisplayName = "JSON array and string handling")]
        public void Flags_Enum_should_be_deserialized_from_JSON_regardless_of_the_serialization_method(string json, JsonFlagsEnumSerializationHandling? jsonFlagsEnumSerializationHandling)
        {
            // Arrange
            var expected = TestEnum.Flag1 | TestEnum.Flag2;
            var converter = jsonFlagsEnumSerializationHandling.HasValue ? new JsonEnumConverter(jsonFlagsEnumSerializationHandling: jsonFlagsEnumSerializationHandling.Value) : new JsonEnumConverter();
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(converter);

            // Act
            var actual = JsonSerializer.Deserialize<TestEnum>(json, options);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Enum_serialization_respects_casing()
        {
            // Arrange
            var value = TestEnum.Flag1;
            var expected = "\"flag1\"";
            var converter = new JsonEnumConverter(new JsonEnumConverterOptions { NamingPolicy = JsonNamingPolicy.CamelCase });
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(converter);

            // Act
            var actual = JsonSerializer.Serialize(value, options);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}

[Flags]
internal enum TestEnum
{
    None,
    Flag1,
    Flag2,
}