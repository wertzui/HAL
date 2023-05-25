using HAL.Common.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Text.Json;

namespace HAL.Tests.Common.Converters
{
    [TestClass]
    public class DateOnlyJsonConverterTests
    {
        [DataTestMethod]
        [DataRow("2021-01-01", "2021-01-01")]
        [DataRow("2021-01-01T00:00:00.000+01:00", "2021-01-01")]
        [DataRow("2021-01-01T00:00:00.000Z", "2021-01-01")]
        [DataRow("2021-01-01T00:00:00.000", "2021-01-01")]
        public void DateOnlyJsonConverterCanReadDifferentDateTypes(string jsonDate, string dateString)
        {
            // Arrange
            var json = $"{{\"date\":\"{jsonDate}\"}}";
            var sut = new DateOnlyJsonConverter();
            var expected = DateOnly.Parse(dateString);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // {
            reader.Read(); // date
            reader.Read(); // dateString

            // Act
            var actual = sut.Read(ref reader, typeof(DateOnly), JsonSerializerOptions.Default);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}