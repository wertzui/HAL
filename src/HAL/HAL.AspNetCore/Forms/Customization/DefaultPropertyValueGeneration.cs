using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// This class implements the default property value generation for HAL-Forms.
    /// It will set the value for normal properties, and handle collection and object types.
    /// </summary>
    public class DefaultPropertyValueGeneration : IPropertyValueGenerationCustomization
    {
        /// <inheritdoc/>
        public bool Exclusive => false;

        /// <inheritdoc/>
        public int Order => 0;

        /// <inheritdoc/>
        public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty) => true;

        /// <inheritdoc/>
        public async ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, object? propertyValue, object? dtoValue, IFormValueFactory formValueFactory)
        {
            // Set the value for normal properties.
            halFormsProperty.Value = propertyValue;

            // options are handled through their SelectedValues property.
            if (halFormsProperty.Options is not null)
            {
                if (halFormsProperty.Value is not null)
                {
                    if (halFormsProperty.Value is IEnumerable enumerable)
                    {
                        // is enumerable
                        halFormsProperty.Options = halFormsProperty.Options with { SelectedValues = new HashSet<object?>(enumerable.Cast<object>()) };
                    }
                    else if (propertyInfo.PropertyType.IsEnum && propertyInfo.PropertyType.IsDefined(typeof(FlagsAttribute)))
                    {
                        // is flags enum
                        var underlyingPropertyValue = Convert.ToUInt64(halFormsProperty.Value);
                        halFormsProperty.Options = halFormsProperty.Options with
                        {
                            SelectedValues = Enum.GetValues(propertyInfo.PropertyType)
                                .Cast<object?>()
                                .Where(f => (Convert.ToUInt64(f) & underlyingPropertyValue) != 0 && !f!.Equals(0))
                                .ToHashSet()
                        };
                    }
                    else
                    {
                        // is single value
                        halFormsProperty.Options = halFormsProperty.Options with { SelectedValues = [halFormsProperty.Value] };
                    }

                    halFormsProperty.Value = null;
                }
            }

            // collection and object types are handled through their templates.
            if (halFormsProperty.Type == PropertyType.Collection)
            {
                halFormsProperty.Templates = await FillWith(halFormsProperty.Templates, halFormsProperty.Value as IEnumerable, formValueFactory);
                halFormsProperty.Value = null;
            }
            else if (halFormsProperty.Type == PropertyType.Object)
            {
                var defaultTemplate = GetDefaultTemplate(halFormsProperty.Templates);
                if (defaultTemplate is not null && halFormsProperty.Value is not null)
                {
                    var filledTemplate = await formValueFactory.FillWithAsync(defaultTemplate, halFormsProperty.Value);
                    halFormsProperty.Templates = new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, filledTemplate } };
                }
                halFormsProperty.Value = null;
            }
        }

        private static ValueTask<IDictionary<string, FormTemplate>?> FillWith(IDictionary<string, FormTemplate>? templates, IEnumerable? values, IFormValueFactory formValueFactory)
        {
            if (values is null || templates is null)
                return ValueTask.FromResult(templates);

            var defaultTemplate = GetDefaultTemplate(templates);
            if (defaultTemplate is null)
                return ValueTask.FromResult<IDictionary<string, FormTemplate>?>(null);

            return FillWithAsync(defaultTemplate, values, formValueFactory);
        }

        private static async ValueTask<IDictionary<string, FormTemplate>?> FillWithAsync(FormTemplate defaultTemplate, IEnumerable values, IFormValueFactory formValueFactory)
        {
            var templates = new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, defaultTemplate } };

            if (values is null)
                return templates;

            int i = 0;
            foreach (var value in values)
            {
                var filledTemplate = await formValueFactory.FillWithAsync(defaultTemplate, value);
                filledTemplate.Title = i.ToString();
                templates[filledTemplate.Title] = filledTemplate;
                i++;
            }

            return templates;
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
    }
}