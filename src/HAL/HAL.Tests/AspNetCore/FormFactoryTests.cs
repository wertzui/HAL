using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms;
using HAL.AspNetCore.Forms.Customization;
using HAL.Common.Forms;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAL.Tests.AspNetCore;

[TestClass]
public class FormFactoryTests
{
    [TestMethod]
    public async Task FormFactory_can_generate_a_simple_Form()
    {
        // Arrange
        var templateFactory = new FormTemplateFactory([new DefaultPropertyTemplateGeneration([])]);
        var valueFactory = new FormValueFactory([new DefaultPropertyValueGeneration()]);
        var linkFactory = Substitute.For<ILinkFactory>();
        using var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var dto = new SimpleDto { Age = 42, Name = "John Doe" };
        var sut = new FormFactory(templateFactory, valueFactory, linkFactory, [new DefaultFormsResourceGenerationCustomization(linkFactory)], cache);

        // Act
        var form = await sut.CreateFormAsync<SimpleDto, SimpleDto>(dto, "target", HttpMethod.Post, "title", "contentType");

        // Assert
        Assert.IsNotNull(form);
        Assert.AreEqual("target", form.Target);
        Assert.AreEqual(HttpMethod.Post.ToString(), form.Method);
        Assert.AreEqual("title", form.Title);
        Assert.AreEqual("contentType", form.ContentType);
        Assert.IsNotNull(form.Properties);
        Assert.HasCount(2, form.Properties);

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

    [TestMethod]
    public async Task FormFactory_can_generate_a_form_with_values_from_one_type_and_validation_from_another()
    {
        var templateFactory = new FormTemplateFactory([new DefaultPropertyTemplateGeneration([])]);
        var valueFactory = new FormValueFactory([new DefaultPropertyValueGeneration()]);
        var linkFactory = Substitute.For<ILinkFactory>();
        using var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var dto = new ReadDto { Id = 42, Name = "John Doe", Description = "Visible but not editable" };
        var sut = new FormFactory(templateFactory, valueFactory, linkFactory, [new DefaultFormsResourceGenerationCustomization(linkFactory)], cache);

        var form = await sut.CreateFormAsync<ReadDto, UpdateDto>(dto, "target", HttpMethod.Put, "title", "contentType");

        Assert.IsNotNull(form.Properties);
        Assert.HasCount(4, form.Properties);

        var idProperty = form.Properties.Single(p => p.Name == "id");
        Assert.AreEqual(42, idProperty.Value);
        Assert.IsFalse(idProperty.Required);
        Assert.IsNull(idProperty.Min);

        var nameProperty = form.Properties.Single(p => p.Name == "name");
        Assert.AreEqual("John Doe", nameProperty.Value);
        Assert.IsTrue(nameProperty.Required);
        Assert.AreEqual(3L, nameProperty.MinLength);
        Assert.AreEqual(10L, nameProperty.MaxLength);

        var descriptionProperty = form.Properties.Single(p => p.Name == "description");
        Assert.AreEqual("Visible but not editable", descriptionProperty.Value);
        Assert.IsFalse(descriptionProperty.Required);
        Assert.IsNull(descriptionProperty.MaxLength);

        var emailProperty = form.Properties.Single(p => p.Name == "email");
        Assert.IsNull(emailProperty.Value);
        Assert.IsTrue(emailProperty.Required);
        Assert.AreEqual(PropertyType.Email, emailProperty.Type);
    }

    [TestMethod]
    public async Task FormFactory_can_generate_a_form_with_values_from_one_type_and_validation_from_another_when_value_is_null()
    {
        var templateFactory = new FormTemplateFactory([new DefaultPropertyTemplateGeneration([])]);
        var valueFactory = new FormValueFactory([new DefaultPropertyValueGeneration()]);
        var linkFactory = Substitute.For<ILinkFactory>();
        using var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        ReadDto? dto = null;
        var sut = new FormFactory(templateFactory, valueFactory, linkFactory, [new DefaultFormsResourceGenerationCustomization(linkFactory)], cache);

        var form = await sut.CreateFormAsync<ReadDto?, UpdateDto>(dto, "target", HttpMethod.Put, "title", "contentType");

        Assert.IsNotNull(form.Properties);
        Assert.HasCount(4, form.Properties);
        Assert.Contains(p => p.Name == "id", form.Properties);
        Assert.Contains(p => p.Name == "name", form.Properties);
        Assert.Contains(p => p.Name == "description", form.Properties);
        Assert.Contains(p => p.Name == "email", form.Properties);

        Assert.IsTrue(form.Properties.Single(p => p.Name == "name").Required);
        Assert.IsTrue(form.Properties.Single(p => p.Name == "email").Required);
        Assert.AreEqual(0, form.Properties.Single(p => p.Name == "id").Value);
        Assert.IsNull(form.Properties.Single(p => p.Name == "email").Value);
    }

    private class SimpleDto
    {
        public required string Name { get; set; }
        public int Age { get; set; }
    }

    private class ReadDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }

    private class UpdateDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
