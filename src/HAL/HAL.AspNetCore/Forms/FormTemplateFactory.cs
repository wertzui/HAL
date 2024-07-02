using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.Forms.Customization;
using HAL.Common;
using HAL.Common.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms;

/// <inheritdoc/>
public class FormTemplateFactory : IFormTemplateFactory
{
    private static readonly JsonNamingPolicy _propertyNamingPolicy = new JsonSerializerOptions(JsonSerializerDefaults.Web).PropertyNamingPolicy!;
    private readonly IEnumerable<IPropertyTemplateGenerationCustomization> _propertyTemplateGenerationCustomizations;

    /// <summary>
    /// Creates a new instance of the <see cref="FormTemplateFactory"/> class.
    /// </summary>
    /// <param name="propertyTemplateGenerationCustomizations">The property generations customizations.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FormTemplateFactory(IEnumerable<IPropertyTemplateGenerationCustomization> propertyTemplateGenerationCustomizations)
    {
        _propertyTemplateGenerationCustomizations = propertyTemplateGenerationCustomizations;
    }

    /// <inheritdoc/>
    public async ValueTask<FormTemplate> CreateTemplateForAsync(Type dtoType, string? method, string? title = null, string contentType = Constants.MediaTypes.Json)
    {
        var properties = await CreatePropertiesForAsync(dtoType);

        var formTemplate = new FormTemplate
        {
            ContentType = contentType,
            Properties = properties,
            Title = title
        };

        if (!string.IsNullOrWhiteSpace(method))
            formTemplate.Method = method;

        return formTemplate;
    }

    /// <inheritdoc/>
    public ValueTask<FormTemplate> CreateTemplateForAsync<TDto>(string method, string? title = null, string contentType = Constants.MediaTypes.Json) => CreateTemplateForAsync(typeof(TDto), method, title, contentType);

    /// <inheritdoc/>
    public async ValueTask<IDictionary<string, FormTemplate>> CreateTemplatesWithDefaultEntryAsync(Type dtoType, string? method, string? title = null, string contentType = Constants.MediaTypes.Json)
    {
        var template = await CreateTemplateForAsync(dtoType, method, title, contentType);
        return new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, template } };
    }

    /// <summary>
    /// Creates all property templates for the given type.
    /// </summary>
    /// <param name="dtoType">The type to create properties for.</param>
    /// <returns>A collection with all the property templates.</returns>
    private async ValueTask<ICollection<Property>> CreatePropertiesForAsync(Type dtoType)
    {
        var propertyInfos = dtoType.GetProperties();
        var propertiesTasks = propertyInfos
            .OrderBy(property => (property.GetCustomAttribute<DisplayAttribute>(true)?.GetOrder()).GetValueOrDefault())
            .Select(CreatePropertyTemplateAsync);

        var properties = new List<Property>(propertyInfos.Length);
        foreach (var propertyTask in propertiesTasks)
        {
            var propertyResult = await propertyTask;
            if (propertyResult.Include)
                properties.Add(propertyResult.Property!);
        }

        return properties;
    }

    /// <summary>
    /// Creates the template for the given property.
    /// </summary>
    /// <param name="propertyInfo">The property to create the template for.</param>
    /// <returns>The generated property template.</returns>
    private async ValueTask<PropertyCreationResult> CreatePropertyTemplateAsync(PropertyInfo propertyInfo)
    {
        var halProperty = new Property(_propertyNamingPolicy.ConvertName(propertyInfo.Name));

        var customizations = _propertyTemplateGenerationCustomizations.Where(c => c.AppliesTo(propertyInfo, halProperty)).OrderBy(c => c.Order).ToList();
        var exclusiveCustomizations = customizations.Where(a => a.Exclusive).ToList();
        if (exclusiveCustomizations.Count > 1)
            throw new InvalidOperationException($"Multiple {nameof(IPropertyTemplateGenerationCustomization)}s with {nameof(IPropertyTemplateGenerationCustomization.Exclusive)} set to true found on property {propertyInfo.Name} of type {propertyInfo.DeclaringType?.Name}. Only one exclusive customization can be used. The found customizations are: {string.Join(", ", exclusiveCustomizations.Select(c => c.GetType().Name))}");

        if (exclusiveCustomizations.Count == 1)
        {
            var customization = exclusiveCustomizations[0];
            if (!await customization.IncludeAsync(propertyInfo, halProperty, this))
                return PropertyCreationResult.NotCreated();

            await customization.ApplyAsync(propertyInfo, halProperty, this);
        }
        else
        {
            foreach (var customization in customizations)
            {
                if (!await customization.IncludeAsync(propertyInfo, halProperty, this))
                    return PropertyCreationResult.NotCreated();
            }

            foreach (var customization in customizations)
            {
                await customization.ApplyAsync(propertyInfo, halProperty, this);
            }
        }

        return PropertyCreationResult.Created(halProperty);
    }

    private class PropertyCreationResult
    {
        private PropertyCreationResult(bool include, Property? property)
        {
            Include = include;
            Property = property;
        }

        [MemberNotNullWhen(true, nameof(Property))]
        public bool Include { get; }

        public Property? Property { get; }

        internal static PropertyCreationResult Created(Property property) => new(true, property);

        internal static PropertyCreationResult NotCreated() => new(false, null);
    }
}