using HAL.Common.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Text.Json;

namespace HAL.Tests.Common.Converters
{
    [TestClass]
    public class TimenlyJsonConverterTests
    {
        [DataTestMethod]
        [DataRow("2021-01-01", "00:00")]
        [DataRow("2021-01-01T00:00:00.000+01:00", "00:00")]
        [DataRow("2021-01-01T00:00:00.000Z", "00:00")]
        [DataRow("2021-01-01T00:00:00.000", "00:00")]
        public void TimeOnlyJsonConverterCanReadDifferentDateTypes(string jsonDate, string timeString)
        {
            // Arrange
            var json = $"{{\"time\":\"{jsonDate}\"}}";
            var sut = new TimeOnlyJsonConverter();
            var expected = TimeOnly.Parse(timeString);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // {
            reader.Read(); // time
            reader.Read(); // timeString

            // Act
            var actual = sut.Read(ref reader, typeof(TimeOnly), JsonSerializerOptions.Default);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}