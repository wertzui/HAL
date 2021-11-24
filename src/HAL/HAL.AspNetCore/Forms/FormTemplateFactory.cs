using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Binary;
using HAL.Common.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.AspNetCore.Forms
{
    /// <inheritdoc/>
    public class FormTemplateFactory : IFormTemplateFactory
    {
        private readonly IEnumerable<IForeignKeyLinkFactory> _foreignKeyLinkFactories;
        private readonly ILinkFactory _linkFactory;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Creates a new instance of the <see cref="FormTemplateFactory"/> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        /// <param name="foreignKeyLinkFactories">
        /// These factories are used to create a link for properties which are decorated with a
        /// [ForeignKey] attribute.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public FormTemplateFactory(
            ILinkFactory linkFactory,
            IEnumerable<IForeignKeyLinkFactory> foreignKeyLinkFactories)
        {
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
            _foreignKeyLinkFactories = foreignKeyLinkFactories ?? throw new ArgumentNullException(nameof(foreignKeyLinkFactories));
        }

        /// <inheritdoc/>
        public FormTemplate CreateTemplateFor(Type dtoType, string method, string title = null, string contentType = "application/json")
        {
            var properties = CreatePropertiesFor(dtoType);

            var formTemplate = new FormTemplate
            {
                ContentType = contentType,
                Method = method,
                Properties = properties,
                Title = title
            };

            return formTemplate;
        }

        /// <inheritdoc/>
        public FormTemplate CreateTemplateFor<TDto>(string method, string title = null, string contentType = "application/json") => CreateTemplateFor(typeof(TDto), method, title, contentType);

        /// <summary>
        /// Applies information to properties if they are decorated with a [DataType] attribute.
        /// </summary>
        /// <param name="template">The property template to enrich with information.</param>
        /// <param name="dataType">The attribute.</param>
        private static void AddTypeInformationFromAttribute(Property template, DataTypeAttribute dataType)
        {
            switch (dataType.DataType)
            {
                case DataType.Custom when Enum.TryParse<PropertyType>(dataType.CustomDataType, true, out var custom):
                    template.Type = custom;
                    break;

                case DataType.DateTime:
                    template.Type = PropertyType.DatetimeLocal;
                    break;

                case DataType.Date:
                    template.Type = PropertyType.Date;
                    break;

                case DataType.Time:
                    template.Type = PropertyType.Time;
                    break;

                case DataType.Duration:
                    template.Type = PropertyType.Duration;
                    break;

                case DataType.PhoneNumber:
                    template.Type = PropertyType.Tel;
                    break;

                case DataType.Currency:
                    template.Type = PropertyType.Number;
                    break;

                case DataType.Text:
                    template.Type = PropertyType.Text;
                    break;

                case DataType.Html:
                    template.Type = PropertyType.Textarea;
                    break;

                case DataType.MultilineText:
                    template.Type = PropertyType.Textarea;
                    break;

                case DataType.EmailAddress:
                    template.Type = PropertyType.Email;
                    template.Regex = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01 -\x08\x0b\x0c\x0e -\x1f\x21\x23 -\x5b\x5d -\x7f] |\\[\x01-\x09\x0b\x0c\x0e -\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";
                    break;

                case DataType.Password:
                    template.Type = PropertyType.Password;
                    break;

                case DataType.Url:
                    template.Type = PropertyType.Url;
                    break;

                case DataType.ImageUrl:
                    template.Type = PropertyType.Image;
                    break;

                case DataType.CreditCard:
                    template.Type = PropertyType.Text;
                    template.Regex = @"^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$";
                    break;

                case DataType.PostalCode:
                    template.Type = PropertyType.Text;
                    break;

                case DataType.Upload when dataType is FileExtensionsAttribute f:
                    template.Type = PropertyType.File;
                    template.Placeholder = f.Extensions;
                    break;

                case DataType.Upload:
                    template.Type = PropertyType.File;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Adds options with a link for a property which has been decorated with the [ForgeignKey] attribute.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="foreignKey">The [ForeignKey] attribute.</param>
        /// <param name="containingDtoType">The type containing the property.</param>
        private void AddForeignKeyLink(Property template, ForeignKeyAttribute foreignKey, Type containingDtoType)
        {
            AddForeignKeyLink(template, foreignKey.Name, containingDtoType);
        }

        /// <summary>
        /// Adds options with a link for a property which has a name that ends with "id" (Convention).
        /// </summary>
        /// <param name="template"></param>
        /// <param name="property">The property.</param>
        /// <param name="containingDtoType">The type containing the property.</param>
        private void AddForeignKeyLink(Property template, PropertyInfo property, Type containingDtoType)
        {
            if (property.Name.Length <= 2 || !property.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                return;

            var foreignKeyName = property.Name[..^2];

            AddForeignKeyLink(template, foreignKeyName, containingDtoType);
        }

        /// <summary>
        /// Adds options with a link for a property which has the given name and is IEnumerable{T}.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="foreignKeyName">The name of the foreign key property.</param>
        /// <param name="containingDtoType">The type containing the property.</param>
        private void AddForeignKeyLink(Property template, string foreignKeyName, Type containingDtoType)
        {
            if (foreignKeyName is null)
                return;

            var referencedListDtoProperty = containingDtoType.GetProperty(foreignKeyName);
            if (referencedListDtoProperty is null)
                return;

            var propertyType = referencedListDtoProperty.PropertyType;
            var isEnumerable = propertyType.IsGenericType && propertyType.IsAssignableTo(typeof(IEnumerable));
            var listDtoType = isEnumerable ? propertyType.GetGenericArguments()[0] : propertyType;
            var link = _foreignKeyLinkFactories
                .Where(f => f.CanCreateLink(listDtoType))
                .Take(1)
                .Select(f => f.CreateLink(listDtoType))
                .FirstOrDefault();

            if (link is null)
                return;

            template.Options = new Options<object> { Link = link };
            template.Options.ValueField = FindPrimaryKeyPropertyName(listDtoType);
            template.Options.PromptField = FindForeignDisplayColumn(listDtoType);
        }

        /// <summary>
        /// Tries to get the name of the property to display.
        /// First looks for a [DisplayColumn] attribute defined on the type of the referenced type.
        /// Then tries the following:
        /// 1. If the type has only one property defined, use it.
        /// 2. If the type has only one string property defined, use it.
        /// 3. If all base types have only one property defined, use it.
        /// 4. If all base types have only one string property defined, use it.
        /// 5. If no property could be found, throw an Exception.
        /// </summary>
        /// <param name="referencedType">The type of the referenced entity.</param>
        /// <returns>The name of the property in camel case.</returns>
        /// <exception cref="ArgumentException">If no property could be found.</exception>
        private string FindForeignDisplayColumn(Type referencedType)
        {
            var displayColumnAttribute = referencedType.GetCustomAttribute<DisplayColumnAttribute>(true);
            string propertyName = null;

            if (displayColumnAttribute is not null)
            {
                propertyName = displayColumnAttribute.DisplayColumn;
            }
            else
            {
                var properties = referencedType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var ownProperties = properties.Where(p => p.DeclaringType == referencedType).ToList();
                var numOwnProperties = ownProperties.Count;
                if (numOwnProperties == 1)
                {
                    propertyName = ownProperties[0].Name;
                }
                else if (numOwnProperties > 1)
                {
                    var ownStringProperties = ownProperties.Where(p => p.PropertyType == typeof(string)).ToList();
                    var numOwnStringProperties = ownStringProperties.Count;
                    if (numOwnStringProperties == 1)
                    {
                        propertyName = ownStringProperties[0].Name;
                    }
                }

                if (propertyName is null)
                {
                    var numProperties = properties.Length;
                    if (numProperties == 1)
                    {
                        propertyName = properties[0].Name;
                    }
                    else if (numProperties > 1)
                    {
                        var stringProperties = properties.Where(p => p.PropertyType == typeof(string)).ToList();
                        var numStringProperties = stringProperties.Count;
                        if (numStringProperties == 1)
                        {
                            propertyName = stringProperties[0].Name;
                        }
                    }
                }
            }

            if (propertyName is null)
                throw new ArgumentException($"Unable to determine the display column for {referencedType.Name}. No [DisplayColumn] Attribute was found.", nameof(referencedType));

            return _jsonSerializerOptions.PropertyNamingPolicy.ConvertName(propertyName);
        }

        private static string FindPrimaryKeyPropertyName(Type type)
        {
            var primaryKeyProperty = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => new
                {
                    p.Name,
                    InheritedKeyAttribute = p.GetCustomAttribute<KeyAttribute>(true),
                    OwnKeyAttribute = p.GetCustomAttribute<KeyAttribute>(false)
                })
                .Where(p => string.Equals("id", p.Name, StringComparison.OrdinalIgnoreCase) || p.OwnKeyAttribute is not null || p.InheritedKeyAttribute is not null)
                // First look for a [Key] Attribute defined on the class, then for an inherited [Key] attribute and at last for an Id property.
                .OrderBy(p => p.OwnKeyAttribute is not null ? 0 : p.InheritedKeyAttribute is not null ? 1 : 2)
                .FirstOrDefault();

            if (primaryKeyProperty is null)
                throw new ArgumentException($"Unable to determine the primary key for {type.Name}. No property with a [Key] Attribute or the name Id was found.", nameof(type));

            return _jsonSerializerOptions.PropertyNamingPolicy.ConvertName(primaryKeyProperty.Name);
        }

        /// <summary>
        /// Applies information to properties based on their type.
        /// </summary>
        /// <param name="template">The property template to enrich with information.</param>
        /// <param name="property">The property for which this template is for.</param>
        private void AddTypeInformation(Property template, PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var nullablePropertyType = Nullable.GetUnderlyingType(propertyType);

            if (property.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                template.ReadOnly = true;
                template.Required = true;
            }

            if (propertyType.IsAssignableTo(typeof(string)))
            {
                template.Type = PropertyType.Text;
            }
            else if (propertyType.IsAssignableTo(typeof(bool?)))
            {
                template.Type = PropertyType.Bool;
                template.Required = nullablePropertyType != null;
            }
            else if (propertyType.IsEnum || (nullablePropertyType?.IsEnum).GetValueOrDefault())
            {
                var isFlags = propertyType.IsDefined(typeof(FlagsAttribute), true);
                var isNullable = nullablePropertyType is not null;
                var enumType = isNullable ? nullablePropertyType : propertyType;
                template.Options = new Options<object>
                {
                    Inline = Enum.GetValues(enumType)
                        .Cast<object>()
                        .Select(v => new OptionsItem<object> { Prompt = Enum.GetName(enumType, v), Value = v })
                        .ToList(),
                    MaxItems = isFlags ? default : 1,
                    MinItems = isNullable ? 0 : 1
                };
            }
            else if (propertyType.IsAssignableTo(typeof(byte?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = byte.MinValue;
                template.Max = byte.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(short?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = short.MinValue;
                template.Max = short.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(int?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = int.MinValue;
                template.Max = int.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(long?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = long.MinValue;
                template.Max = long.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(float?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = float.MinValue;
                template.Max = float.MaxValue;
                template.Regex = $@"^[-+]?\d*\.?\d*$";
            }
            else if (propertyType.IsAssignableTo(typeof(double?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = double.MinValue;
                template.Max = double.MaxValue;
                template.Regex = $@"^[-+]?\d*\.?\d*$";
            }
            else if (propertyType.IsAssignableTo(typeof(ushort?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = ushort.MinValue;
                template.Max = ushort.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(uint?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = uint.MinValue;
                template.Max = uint.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(ulong?)))
            {
                template.Type = PropertyType.Number;
                template.Required = nullablePropertyType != null;
                template.Min = ulong.MinValue;
                template.Max = ulong.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
            }
            else if (propertyType.IsAssignableTo(typeof(DateTime?)))
            {
                template.Type = PropertyType.DatetimeLocal;
                template.Required = nullablePropertyType != null;
            }
            else if (propertyType.IsAssignableTo(typeof(DateTimeOffset?)))
            {
                template.Type = PropertyType.DatetimeOffset;
                template.Required = nullablePropertyType != null;
            }
            else if (propertyType.IsAssignableTo(typeof(TimeSpan?)))
            {
                template.Type = PropertyType.Duration;
                template.Required = nullablePropertyType != null;
            }
            else if (propertyType.IsAssignableTo(typeof(byte[])) && string.Equals(property.Name, "timestamp", StringComparison.OrdinalIgnoreCase))
            {
                template.Type = PropertyType.Hidden;
            }
            else if (propertyType.IsGenericType && propertyType.IsAssignableTo(typeof(IEnumerable)))
            {
                template.Type = PropertyType.Collection;
                var collectionDtoType = propertyType.GetGenericArguments()[0];
                template.Templates = CreateDefaultTemplateFor(collectionDtoType, null, template.Prompt, null);
            }
            else if (!propertyType.IsAssignableTo(typeof(HalFile)))
            {
                template.Type = PropertyType.Object;
                template.Templates = CreateDefaultTemplateFor(propertyType, null, template.Prompt, null);
            }
        }

        /// <summary>
        /// Creates a template collection with the "default" template for the given type.
        /// </summary>
        /// <param name="dtoType">The type to create a form for.</param>
        /// <param name="method">The method that is used when the form is submitted.</param>
        /// <param name="title">A human readable title for the form.</param>
        /// <param name="contentType">The content type that is used when the form is submitted.</param>
        /// <returns>A new dictionary with only a "default" form template.</returns>
        private IDictionary<string, FormTemplate> CreateDefaultTemplateFor(Type dtoType, string method, string title = null, string contentType = "application/json")
        {
            return new Dictionary<string, FormTemplate> { { "default", CreateTemplateFor(dtoType, method, title, contentType) } };
        }

        /// <summary>
        /// Creates all property templates for the given type.
        /// </summary>
        /// <param name="dtoType">The type to create properties for.</param>
        /// <returns>A collection with all the property templates.</returns>
        private ICollection<Property> CreatePropertiesFor(Type dtoType)
        {
            var properties = dtoType.GetProperties()
                .Where(IncludePropertyInTemplate)
                .OrderBy(property => (property.GetCustomAttribute<DisplayAttribute>(true)?.GetOrder()).GetValueOrDefault())
                .Select(property => CreatePropertyTemplate(property, dtoType))
                .ToList();
            return properties;
        }

        private static bool IncludePropertyInTemplate(PropertyInfo property)
        {
            var jsonIgnoreAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>(true);
            if (jsonIgnoreAttribute is not null && jsonIgnoreAttribute.Condition == JsonIgnoreCondition.Always)
                return false;

            var scaffoldAttribute = property.GetCustomAttribute<ScaffoldColumnAttribute>(true);
            if (scaffoldAttribute is not null && !scaffoldAttribute.Scaffold)
                return false;

            var ignoreDataMemberAttribute = property.GetCustomAttribute<IgnoreDataMemberAttribute>(true);
            if (ignoreDataMemberAttribute is not null)
                return false;

            return true;
        }

        /// <summary>
        /// Creates the template for the given property.
        /// </summary>
        /// <param name="property">The property to create the template for.</param>
        /// <param name="dtoType">The type containing the property.</param>
        /// <returns>The generated property template.</returns>
        private Property CreatePropertyTemplate(PropertyInfo property, Type dtoType)
        {
            var template = new Property
            {
                Name = _jsonSerializerOptions.PropertyNamingPolicy.ConvertName(property.Name),
                Prompt = property.Name
            };

            AddTypeInformation(template, property);
            AddForeignKeyLink(template, property, dtoType);

            foreach (var attribute in property.GetCustomAttributes(true))
            {
                switch (attribute)
                {
                    case DataTypeAttribute dataType:
                        AddTypeInformationFromAttribute(template, dataType);
                        break;

                    case ConcurrencyCheckAttribute:
                        template.Type = PropertyType.Hidden;
                        break;

                    case DisplayAttribute display:
                        template.Prompt = display.GetName();
                        template.Placeholder = display.GetPrompt();
                        if (display.GetAutoGenerateField() == false)
                            return null;
                        break;

                    case EditableAttribute editable:
                        template.ReadOnly = !editable.AllowEdit;
                        break;

                    case ForeignKeyAttribute foreignKey:
                        AddForeignKeyLink(template, foreignKey, dtoType);
                        break;

                    case KeyAttribute:
                        template.ReadOnly = true;
                        template.Required = true;
                        break;

                    case MaxLengthAttribute maxLength:
                        template.MaxLength = maxLength.Length;
                        break;

                    case MinLengthAttribute minLength:
                        template.MinLength = minLength.Length;
                        break;

                    case RangeAttribute range:
                        if (double.TryParse(range.Minimum?.ToString(), out var min))
                            template.Min = min;
                        if (double.TryParse(range.Maximum?.ToString(), out var max))
                            template.Max = max;
                        break;

                    case RegularExpressionAttribute regularExpression:
                        template.Regex = regularExpression.Pattern;
                        break;

                    case RequiredAttribute:
                        template.Required = true;
                        break;

                    case ScaffoldColumnAttribute scaffoldColumn when !scaffoldColumn.Scaffold:
                        return null;

                    case StringLengthAttribute stringLength:
                        template.MinLength = stringLength.MinimumLength;
                        template.MaxLength = stringLength.MinimumLength;
                        break;

                    case TimestampAttribute:
                        template.Type = PropertyType.Hidden;
                        break;

                    case UIHintAttribute uIHint when Enum.TryParse<PropertyType>(uIHint.UIHint, true, out var custom):
                        template.Type = custom;
                        break;

                    default:
                        break;
                }
            }

            return template;
        }
    }
}