using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.Forms.Customization;
using HAL.Common.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms;

/// <inheritdoc/>
public class FormValueFactory : IFormValueFactory
{
    private readonly IEnumerable<IPropertyValueGenerationCustomization> _propertyValueGenerationCustomizations;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormValueFactory"/> class.
    /// </summary>
    /// <param name="propertyValueGenerationCustomizations">The customizations to apply during property value generation.</param>
    public FormValueFactory(IEnumerable<IPropertyValueGenerationCustomization> propertyValueGenerationCustomizations)
    {
        _propertyValueGenerationCustomizations = propertyValueGenerationCustomizations ?? throw new ArgumentNullException(nameof(propertyValueGenerationCustomizations));
    }

    /// <inheritdoc/>
    public async ValueTask<FormTemplate> FillWithAsync(FormTemplate template, object? value)
    {
        var filled = new FormTemplate
        {
            ContentType = template.ContentType,
            Method = template.Method,
            Properties = await FillPropertiesAsync(template.Properties, value),
            Target = template.Target,
            Title = template.Title
        };

        return filled;
    }

    private async ValueTask<ICollection<Property>?> FillPropertiesAsync(ICollection<Property>? properties, object? dtoValue)
    {
        if (dtoValue is null || properties is null)
            return properties;

        var dtoValueType = dtoValue.GetType();
        var filledProperties = new List<Property>(properties.Count);
        foreach (var property in properties)
        {
            if (!TryGetPropertyInfo(property, dtoValueType, out var propertyInfo))
            {
                filledProperties.Add(property);
                continue;
            }

            var value = propertyInfo.GetValue(dtoValue);
            var filledProperty = await FillPropertyAsync(propertyInfo, property, value, dtoValue);

            filledProperties.Add(filledProperty);
        }

        return filledProperties;
    }

    private async ValueTask<Property> FillPropertyAsync(PropertyInfo propertyInfo, Property halProperty, object? propertyValue, object? dtoValue)
    {
        var filled = new Property(halProperty.Name)
        {
            Cols = halProperty.Cols,
            Extensions = halProperty.Extensions,
            Max = halProperty.Max,
            MaxLength = halProperty.MaxLength,
            Min = halProperty.Min,
            MinLength = halProperty.MinLength,
            Options = halProperty.Options,
            Placeholder = halProperty.Placeholder,
            Prompt = halProperty.Prompt,
            ReadOnly = halProperty.ReadOnly,
            Regex = halProperty.Regex,
            Required = halProperty.Required,
            Rows = halProperty.Rows,
            Step = halProperty.Step,
            Templates = halProperty.Templates,
            Templated = halProperty.Templated,
            Type = halProperty.Type
        };

        foreach (var customization in _propertyValueGenerationCustomizations)
        {
            if (!customization.AppliesTo(propertyInfo, filled))
                continue;

            await customization.ApplyAsync(propertyInfo, filled, propertyValue, dtoValue, this);
        }

        return filled;
    }

    private static bool TryGetPropertyInfo(Property halProperty, Type valueType, [NotNullWhen(true)] out PropertyInfo? propertyInfo)
    {
        propertyInfo = valueType.GetProperty(halProperty.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        return propertyInfo is not null;
    }
}