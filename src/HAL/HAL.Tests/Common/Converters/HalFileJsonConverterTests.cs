using HAL.Common.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.Json;

namespace HAL.Tests.Common.Converters
{
    [TestClass]
    public class HalFileJsonConverterTests
    {
        [TestMethod]
        public void HalFileJsonConverter_can_roundtrip()
        {
            // Arrange
            var testBytes = Enumerable.Range(1, 100).Select(i => (byte)i).ToArray();
            var testMimeType = "application/octet-stream";
            var expected = new HalFile(testMimeType, testBytes);

            // Act
            var json = JsonSerializer.Serialize(expected);
            var actual = JsonSerializer.Deserialize<HalFile>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.MimeType, actual.MimeType);
            CollectionAssert.AreEqual(expected.Content, actual.Content);
            Assert.AreEqual(expected.HasDataUri, actual.HasDataUri);
            Assert.AreEqual(expected.Uri, actual.Uri);
        }
    }
}