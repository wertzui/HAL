using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms;
using HAL.AspNetCore.Forms.Customization;
using HAL.Common.Forms;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAL.Tests.AspNetCore
{
    [TestClass]
    public class FormFactoryTests
    {
        [TestMethod]
        public async Task FormFactory_can_Generate_a_simple_Form()
        {
            // Arrange
            var templateFactory = new FormTemplateFactory([new DefaultPropertyTemplateGeneration([])]);
            var valueFactory = new FormValueFactory([new DefaultPropertyValueGeneration()]);
            var linkFactory = Substitute.For<ILinkFactory>();
            using var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var dto = new SimpleDto { Age = 42, Name = "John Doe" };
            var sut = new FormFactory(templateFactory, valueFactory, linkFactory, cache);

            // Act
            var form = await sut.CreateFormAsync(dto, "target", HttpMethod.Post, "title", "contentType");

            // Assert
            Assert.IsNotNull(form);
            Assert.AreEqual("target", form.Target);
            Assert.AreEqual(HttpMethod.Post.ToString(), form.Method);
            Assert.AreEqual("title", form.Title);
            Assert.AreEqual("contentType", form.ContentType);
            Assert.IsNotNull(form.Properties);
            Assert.AreEqual(2, form.Properties.Count);

            var nameProperty = form.Properties.SingleOrDefault(p => p.Name == "name");
            Assert.IsNotNull(nameProperty);
            Assert.AreEqual("name", nameProperty.Name);
            Assert.AreEqual(dto.Name, nameProperty.Value);
            Assert.AreEqual(PropertyType.Text, nameProperty.Type);
            Assert.IsTrue(nameProperty.Required);
            Assert.IsFalse(nameProperty.ReadOnly);

            var ageProperty = form.Properties.SingleOrDefault(p => p.Name == "age");
            Assert.IsNotNull(ageProperty);
            Assert.AreEqual("age", ageProperty.Name);
            Assert.AreEqual(dto.Age, ageProperty.Value);
            Assert.AreEqual(PropertyType.Number, ageProperty.Type);
            Assert.IsTrue(ageProperty.Required);
            Assert.IsFalse(ageProperty.ReadOnly);
        }

        private class SimpleDto
        {
            public required string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
