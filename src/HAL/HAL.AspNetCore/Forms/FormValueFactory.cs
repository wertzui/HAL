using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HAL.AspNetCore.Forms;

/// <inheritdoc/>
public class FormValueFactory : IFormValueFactory
{
    /// <inheritdoc/>
    public FormTemplate FillWith(FormTemplate template, object? value)
    {
        var filled = new FormTemplate
        {
            ContentType = template.ContentType,
            Method = template.Method,
            Properties = Fillproperties(template.Properties, value),
            Target = template.Target,
            Title = template.Title
        };

        return filled;
    }

    private static FormTemplate? GetDefaultTemplate(IDictionary<string, FormTemplate>? templates)
    {
        if (templates is null)
            return null;

        if (templates.TryGetValue("default", out var defaultTemplate))
            return defaultTemplate;

        if (templates.Values.Count == 1)
            return templates.Values.First();

        throw new ArgumentOutOfRangeException(nameof(templates), "The templates dictionary must either contain a \"default\", or only one template.");
    }

    private ICollection<Property>? Fillproperties(ICollection<Property>? properties, object? value)
    {
        if (value is null || properties is null)
            return properties;

        var valueType = value.GetType();
        return properties
            .Select(p => FillProperty(p, value, valueType))
            .ToList();
    }

    private Property FillProperty(Property p, object value, Type valueType)
    {
        var filled = new Property(p.Name)
        {
            Cols = p.Cols,
            Extensions = p.Extensions,
            Max = p.Max,
            MaxLength = p.MaxLength,
            Min = p.Min,
            MinLength = p.MinLength,
            Options = p.Options,
            Placeholder = p.Placeholder,
            Prompt = p.Prompt,
            ReadOnly = p.ReadOnly,
            Regex = p.Regex,
            Required = p.Required,
            Rows = p.Rows,
            Step = p.Step,
            Templates = p.Templates,
            Templated = p.Templated,
            Type = p.Type,
            Value = GetValue(p, value, valueType)
        };

        // options are handled through their SelectedValues property.
        if (filled.Options is not null)
        {
            if (filled.Value is not null)
            {
                if (filled.Value is IEnumerable enumerable)
                    filled.Options.SelectedValues = new HashSet<object?>(enumerable.Cast<object>());
                else
                    filled.Options.SelectedValues = [filled.Value];

                filled.Value = null;
            }
        }

        // collection and object types are handled through their templates.
        if (filled.Type == PropertyType.Collection)
        {
            filled.Templates = FillWith(filled.Templates, filled.Value as IEnumerable);
            filled.Value = null;
        }
        else if (filled.Type == PropertyType.Object)
        {
            var defaultTemplate = GetDefaultTemplate(filled.Templates);
            if (defaultTemplate is not null && filled.Value is not null)
            {
                var filledTemplate = FillWith(defaultTemplate, filled.Value);
                filled.Templates = new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, filledTemplate } };
            }
            filled.Value = null;
        }

        return filled;
    }

    private IDictionary<string, FormTemplate>? FillWith(IDictionary<string, FormTemplate>? templates, IEnumerable? values)
    {
        if (values is null || templates is null)
            return templates;

        var defaultTemplate = GetDefaultTemplate(templates);
        if (defaultTemplate is null)
            return null;

        return FillWith(defaultTemplate, values);
    }

    private Dictionary<string, FormTemplate> FillWith(FormTemplate defaultTemplate, IEnumerable values)
    {
        var templates = new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, defaultTemplate } };

        if (values is null)
            return templates;

        int i = 0;
        foreach (var value in values)
        {
            var filledTemplate = FillWith(defaultTemplate, value);
            filledTemplate.Title = i.ToString();
            templates[filledTemplate.Title] = filledTemplate;
            i++;
        }

        return templates;
    }

    private static object? GetValue<TDto>(Property property, TDto value, Type valueType)
    {
        var propertyInfo = valueType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) ??
            throw new ArgumentException($"The property {property.Name} does not exist in the value {value}.", nameof(property));

        var propertyValue = propertyInfo.GetValue(value);

        return propertyValue;
    }
}