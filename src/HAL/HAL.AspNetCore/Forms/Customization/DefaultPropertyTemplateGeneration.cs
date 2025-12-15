using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Attributes;
using HAL.Common.Binary;
using HAL.Common.Forms;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// This class implements the default property template generation for HAL-Forms properties.
    /// It generates a property template for a property based on the property's type, name and attributes.
    /// It reacts to the following attributes:
    /// <list type="bullet">
    ///     <item><c>[AllowedValues]</c></item>
    ///     <item><c>[ConcurrencyCheck]</c></item>
    ///     <item><c>[DataType]</c></item>
    ///     <item><c>[DeniedValues]</c></item>
    ///     <item><c>[Display]</c></item>
    ///     <item><c>[DisplayName]</c></item>
    ///     <item><c>[Editable]</c></item>
    ///     <item><c>[FileExtensions]</c></item>
    ///     <item><c>[ForeignKey]</c></item>
    ///     <item><c>[IgnoreDataMember]</c></item>
    ///     <item><c>[JsonIgnore]</c></item>
    ///     <item><c>[Key]</c></item>
    ///     <item><c>[Length]</c></item>
    ///     <item><c>[MaxLength]</c></item>
    ///     <item><c>[MinLength]</c></item>
    ///     <item><c>[PromptDisplayType]</c></item>
    ///     <item><c>[Range]</c></item>
    ///     <item><c>[RegularExpression]</c></item>
    ///     <item><c>[Required]</c></item>
    ///     <item><c>[ScaffoldColumn]</c></item>
    ///     <item><c>[Step]</c></item>
    ///     <item><c>[StringLength]</c></item>
    ///     <item><c>[Timestamp]</c></item>
    ///     <item><c>[UIHint]</c></item>
    /// </list>
    /// </summary>
    public class DefaultPropertyTemplateGeneration : IPropertyTemplateGenerationCustomization
    {
        private static readonly JsonNamingPolicy _propertyNamingPolicy = new JsonSerializerOptions(JsonSerializerDefaults.Web).PropertyNamingPolicy!;
        private static readonly ConcurrentDictionary<Type, object?> _defaultDtos = new();
        private readonly IEnumerable<IForeignKeyLinkFactory> _foreignKeyLinkFactories;

        [field: ThreadStatic]
        private static NullabilityInfoContext NullabilityContext => field ??= new NullabilityInfoContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPropertyTemplateGeneration"/> class.
        /// </summary>
        /// <param name="foreignKeyLinkFactories">
        /// These factories are used to create a link for properties which are decorated with a
        /// [ForeignKey] attribute.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultPropertyTemplateGeneration(IEnumerable<IForeignKeyLinkFactory> foreignKeyLinkFactories)
        {
            _foreignKeyLinkFactories = foreignKeyLinkFactories ?? throw new ArgumentNullException(nameof(foreignKeyLinkFactories));
        }

        /// <inheritdoc/>
        public bool Exclusive => false;

        /// <inheritdoc/>
        public int Order => 0;

        /// <inheritdoc/>
        public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty) => true;

        /// <inheritdoc/>
        public async ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory)
        {
            halFormsProperty.Prompt = propertyInfo.Name;

            await AddTypeInformationAsync(halFormsProperty, propertyInfo, formTemplateFactory);
            AddForeignKeyLink(halFormsProperty, propertyInfo, propertyInfo.DeclaringType!);

            AddAttributeInformation(propertyInfo, halFormsProperty);

            AddDefaultValueIfUserCannotEditProperty(halFormsProperty, propertyInfo, GetOrCreateDefaultDto(propertyInfo.DeclaringType!));
        }

        /// <inheritdoc/>
        public ValueTask<bool> IncludeAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory)
        {
            var jsonIgnoreAttribute = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>(true);
            if (jsonIgnoreAttribute is not null && jsonIgnoreAttribute.Condition == JsonIgnoreCondition.Always)
                return ValueTask.FromResult(false);

            var scaffoldAttribute = propertyInfo.GetCustomAttribute<ScaffoldColumnAttribute>(true);
            if (scaffoldAttribute is not null && !scaffoldAttribute.Scaffold)
                return ValueTask.FromResult(false);

            var ignoreDataMemberAttribute = propertyInfo.GetCustomAttribute<IgnoreDataMemberAttribute>(true);
            if (ignoreDataMemberAttribute is not null)
                return ValueTask.FromResult(false);

            var displayProperty = propertyInfo.GetCustomAttribute<DisplayAttribute>(true);
            if (displayProperty is not null && displayProperty.GetAutoGenerateField() == false)
                return ValueTask.FromResult(false);

            return ValueTask.FromResult(true);
        }

        /// <summary>
        /// When the user cannot edit a property, that is if it is required and either read-only or
        /// hidden, the default value is set, so the user can at least post back the form without
        /// validation triggering. This is mostly used when adding elements to a collection which
        /// have required and read-only properties, like an ID.
        /// </summary>
        /// <param name="halProperty">The property template to set a default value for.</param>
        /// <param name="property">The property info to get the value for.</param>
        /// <param name="defaultDto">The object to get the value from.</param>
        private static void AddDefaultValueIfUserCannotEditProperty(Property halProperty, PropertyInfo property, object? defaultDto)
        {
            if (defaultDto is null)
                return;

            if (halProperty.Required && halProperty.Value is null && (halProperty.ReadOnly || halProperty.Type.HasValue && halProperty.Type == PropertyType.Hidden))
            {
                halProperty.Value = property.GetValue(defaultDto);
            }
        }

        private static void AddExtensionDataToProperty(Property template, object attribute)
        {
            if (attribute is IPropertyExtensionData)
            {
                var type = attribute.GetType();
                var nameWithSuffix = type.Name;
                var name = nameWithSuffix.EndsWith(nameof(Attribute)) ? nameWithSuffix[0..^9] : nameWithSuffix;
                var casedName = _propertyNamingPolicy.ConvertName(name);

                template.Extensions ??= new Dictionary<string, object>();

                template.Extensions[casedName] = attribute;
            }
        }

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
                    template.Regex = @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$";
                    break;

                case DataType.Currency:
                    template.Type = PropertyType.Currency;
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
                    template.Regex = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01.\x08\x0b\x0c\x0e.\x1f\x21\x23.\x5b\x5d.\x7f] |\\[\x01-\x09\x0b\x0c\x0e.\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";
                    break;

                case DataType.Password:
                    template.Type = PropertyType.Password;
                    break;

                case DataType.Url:
                    template.Type = PropertyType.Url;
                    template.Regex = @"^[a-z][a-z0-9+\-.]*:\S+$";
                    break;

                case DataType.ImageUrl:
                    template.Type = PropertyType.Image;
                    template.Regex = @"^[a-z][a-z0-9+\-.]*:\S+$";
                    break;

                case DataType.CreditCard:
                    template.Type = PropertyType.Text;
                    template.Regex = @"^(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$";
                    break;

                case DataType.PostalCode:
                    template.Type = PropertyType.Text;
                    template.Regex = @"^[A-Z0-9][A-Z0-9\- ]{0,10}[A-Z0-9]$";
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

        private static object? CreateDefaultDto(Type dtoType)
        {
            try
            {
                return Activator.CreateInstance(dtoType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to get the name of the property to display. First looks for a [DisplayColumn]
        /// attribute defined on the type of the referenced type. Then tries the following:
        /// 1. If the type has only one property defined, use it.
        /// 2. If the type has only one string property defined, use it.
        /// 3. If all base types have only one property defined, use it.
        /// 4. If all base types have only one string property defined, use it.
        /// 5. If no property could be found, throw an Exception.
        /// </summary>
        /// <param name="referencedType">The type of the referenced entity.</param>
        /// <returns>The name of the property in camel case.</returns>
        /// <exception cref="ArgumentException">If no property could be found.</exception>
        private static string FindForeignDisplayColumn(Type referencedType)
        {
            var displayColumnAttribute = referencedType.GetCustomAttribute<DisplayColumnAttribute>(true);
            string? propertyName = null;

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

            return _propertyNamingPolicy.ConvertName(propertyName);
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
                // First look for a [Key] Attribute defined on the class, then for an inherited
                // [Key] attribute and at last for an Id property.
                .OrderBy(p => p.OwnKeyAttribute is not null ? 0 : p.InheritedKeyAttribute is not null ? 1 : 2)
                .FirstOrDefault() ?? throw new ArgumentException($"Unable to determine the primary key for {type.Name}. No property with a [Key] Attribute or the name Id was found.", nameof(type));
            return _propertyNamingPolicy.ConvertName(primaryKeyProperty.Name);
        }

        private static object? GetOrCreateDefaultDto(Type dtoType) => _defaultDtos.GetOrAdd(dtoType, CreateDefaultDto);

        private void AddAttributeInformation(PropertyInfo propertyInfo, Property halFormsProperty)
        {
            var attributes = propertyInfo.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case AllowedValuesAttribute allowedValues:
                        if (halFormsProperty.Options?.Inline is not null)
                        {
                            // Remove all Options.Inline items that are not in the allowed values.
                            var itemsToRemove = halFormsProperty.Options.Inline
                                .Where(i => !allowedValues.Values.Contains(i.Value))
                                .ToHashSet();
                            foreach (var item in itemsToRemove)
                            {
                                halFormsProperty.Options.Inline?.Remove(item);
                            }
                        }
                        else if (halFormsProperty.Options is null)
                        {
                            // If there are no inline options, create them from the allowed values.
                            halFormsProperty.Type = null;
                            halFormsProperty.Options = new Options<object?>(allowedValues.Values.Select(v => new OptionsItem<object?>(v?.ToString() ?? "null", v)).ToList());
                        }
                        break;

                    case DeniedValuesAttribute deniedValues:
                        if (halFormsProperty.Options?.Inline is not null)
                        {
                            // Remove all Options.Inline items that are denied
                            var itemsToRemove = halFormsProperty.Options.Inline
                                .Where(i => deniedValues.Values.Contains(i.Value))
                                .ToHashSet();
                            foreach (var item in itemsToRemove)
                            {
                                halFormsProperty.Options.Inline?.Remove(item);
                            }
                        }
                        break;

                    case DataTypeAttribute dataType:
                        AddTypeInformationFromAttribute(halFormsProperty, dataType);
                        break;

                    case ConcurrencyCheckAttribute:
                        halFormsProperty.Type = PropertyType.Hidden;
                        break;

                    case DisplayAttribute display:
                        halFormsProperty.Prompt = display.GetName();
                        halFormsProperty.Placeholder = display.GetPrompt();
                        break;

                    case DisplayNameAttribute displayName:
                        halFormsProperty.Prompt = displayName.DisplayName;
                        break;

                    case PromptDisplayTypeAttribute promptDisplayType:
                        halFormsProperty.PromptDisplay = promptDisplayType.PromptDisplay;
                        break;

                    case EditableAttribute editable:
                        halFormsProperty.ReadOnly = !editable.AllowEdit;
                        break;

                    case ForeignKeyAttribute foreignKey:
                        AddForeignKeyLink(halFormsProperty, propertyInfo, foreignKey, propertyInfo.DeclaringType!);
                        break;

                    case KeyAttribute:
                        halFormsProperty.ReadOnly = true;
                        halFormsProperty.Required = true;
                        break;

                    case LengthAttribute length:
                        halFormsProperty.MaxLength = length.MaximumLength;
                        halFormsProperty.MinLength = length.MinimumLength;
                        break;

                    case MaxLengthAttribute maxLength:
                        halFormsProperty.MaxLength = maxLength.Length;
                        break;

                    case MinLengthAttribute minLength:
                        halFormsProperty.MinLength = minLength.Length;
                        break;

                    case RangeAttribute range:
                        if (double.TryParse(range.Minimum?.ToString(), out var min))
                            halFormsProperty.Min = min;
                        if (double.TryParse(range.Maximum?.ToString(), out var max))
                            halFormsProperty.Max = max;
                        break;

                    case ReadOnlyAttribute readOnly:
                        halFormsProperty.ReadOnly = readOnly.IsReadOnly;
                        break;

                    case RegularExpressionAttribute regularExpression:
                        halFormsProperty.Regex = regularExpression.Pattern;
                        break;

                    case RequiredAttribute:
                        halFormsProperty.Required = true;
                        break;

                    case StringLengthAttribute stringLength:
                        halFormsProperty.MinLength = stringLength.MinimumLength;
                        halFormsProperty.MaxLength = stringLength.MaximumLength;
                        break;

                    case TimestampAttribute:
                        halFormsProperty.Type = PropertyType.Hidden;
                        break;

                    case UIHintAttribute uIHint when Enum.TryParse<PropertyType>(uIHint.UIHint, true, out var custom):
                        halFormsProperty.Type = custom;
                        break;

                    case StepAttribute step:
                        halFormsProperty.Step = step.Step;
                        break;

                    default:
                        break;
                }

                AddExtensionDataToProperty(halFormsProperty, attribute);
            }
        }

        /// <summary>
        /// Adds options with a link for a property which has been decorated with the [ForeignKey] attribute.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="property">The property.</param>
        /// <param name="foreignKey">The [ForeignKey] attribute.</param>
        /// <param name="containingDtoType">The type containing the property.</param>
        private void AddForeignKeyLink(Property template, PropertyInfo property, ForeignKeyAttribute foreignKey, Type containingDtoType)
        {
            AddForeignKeyLink(template, foreignKey.Name, property, containingDtoType);
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

            AddForeignKeyLink(template, foreignKeyName, property, containingDtoType);
        }

        /// <summary>
        /// Adds options with a link for a property which has the given name and is IEnumerable{T}.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="foreignKeyName">The name of the foreign key property.</param>
        /// <param name="property">The property.</param>
        /// <param name="containingDtoType">The type containing the property.</param>
        private void AddForeignKeyLink(Property template, string foreignKeyName, PropertyInfo property, Type containingDtoType)
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

            bool isMultiSelect = property.PropertyType.IsAssignableTo(typeof(IEnumerable));

            template.Type = null; // Either type or options can be set, but not both.
            template.Options = new Options<object?>(link)
            {
                ValueField = FindPrimaryKeyPropertyName(listDtoType),
                PromptField = FindForeignDisplayColumn(listDtoType),
                MinItems = template.Required ? 1 : 0,
                MaxItems = isMultiSelect ? long.MaxValue : 1,
            };
        }

        /// <summary>
        /// Applies information to properties based on their type.
        /// </summary>
        /// <param name="template">The property template to enrich with information.</param>
        /// <param name="property">The property for which this template is for.</param>
        /// <param name="formTemplateFactory">The factory which is currently calling this customization.</param>
        private static async ValueTask AddTypeInformationAsync(Property template, PropertyInfo property, IFormTemplateFactory formTemplateFactory)
        {
            Type propertyType = property.PropertyType;
            var nullablePropertyType = Nullable.GetUnderlyingType(propertyType);
            var nullabilityInfo = NullabilityContext.Create(property);
            template.Required = nullabilityInfo.WriteState is NullabilityState.NotNull;

            if (property.Name == nameof(KeyValuePair<object, object>.Key) &&
                property.DeclaringType is not null &&
                property.DeclaringType.IsGenericType &&
                property.DeclaringType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                // For whatever reason the key of KeyValueType<object, object> comes out as nullable
                // when using NullabilityInfoContext.
                template.Required = true;
            }

            if (property.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                template.ReadOnly = true;
            }

            if (propertyType.IsAssignableTo(typeof(string)))
            {
                template.Type = PropertyType.Text;
            }
            else if (propertyType.IsAssignableTo(typeof(bool?)))
            {
                template.Type = PropertyType.Bool;
            }
            else if (propertyType.IsEnum || (nullablePropertyType?.IsEnum).GetValueOrDefault())
            {
                var isFlags = propertyType.IsDefined(typeof(FlagsAttribute), true);
                var isNullable = nullablePropertyType is not null;
                var enumType = isNullable ? nullablePropertyType! : propertyType;
                template.Options = new Options<object?>
                    (Enum.GetValues(enumType)
                        .Cast<object>()
                        .Select(v => new OptionsItem<object?>(Enum.GetName(enumType, v)!, v))
                        .ToList())
                {
                    MaxItems = isFlags ? long.MaxValue : 1,
                    MinItems = isNullable ? 0 : 1
                };
            }
            else if (propertyType.IsAssignableTo(typeof(sbyte?)))
            {
                template.Type = PropertyType.Number;
                template.Min = sbyte.MinValue;
                template.Max = sbyte.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(byte?)))
            {
                template.Type = PropertyType.Number;
                template.Min = byte.MinValue;
                template.Max = byte.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(short?)))
            {
                template.Type = PropertyType.Number;
                template.Min = short.MinValue;
                template.Max = short.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(char?)))
            {
                template.Type = PropertyType.Text;
                template.MinLength = template.Required ? 1 : 0;
                template.MaxLength = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(int?)))
            {
                template.Type = PropertyType.Number;
                template.Min = int.MinValue;
                template.Max = int.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(long?)))
            {
                template.Type = PropertyType.Number;
                template.Min = long.MinValue;
                template.Max = long.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(nint?)))
            {
                template.Type = PropertyType.Number;
                template.Min = nint.MinValue;
                template.Max = nint.MaxValue;
                template.Regex = $@"^(\+|-)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(float?)))
            {
                template.Type = PropertyType.Number;
                template.Min = float.MinValue;
                template.Max = float.MaxValue;
                template.Regex = $@"^[-+]?\d*\.?\d*$";
                template.Step = 0.01;
            }
            else if (propertyType.IsAssignableTo(typeof(double?)))
            {
                template.Type = PropertyType.Number;
                template.Min = double.MinValue;
                template.Max = double.MaxValue;
                template.Regex = $@"^[-+]?\d*\.?\d*$";
                template.Step = 0.01;
            }
            else if (propertyType.IsAssignableTo(typeof(decimal?)))
            {
                template.Type = PropertyType.Number;
                template.Min = (double)decimal.MinValue;
                template.Max = (double)decimal.MaxValue;
                template.Regex = $@"^[-+]?\d*\.?\d*$";
                template.Step = 0.01;
            }
            else if (propertyType.IsAssignableTo(typeof(ushort?)))
            {
                template.Type = PropertyType.Number;
                template.Min = ushort.MinValue;
                template.Max = ushort.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(uint?)))
            {
                template.Type = PropertyType.Number;
                template.Min = uint.MinValue;
                template.Max = uint.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(ulong?)))
            {
                template.Type = PropertyType.Number;
                template.Min = ulong.MinValue;
                template.Max = ulong.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(nuint?)))
            {
                template.Type = PropertyType.Number;
                template.Min = nuint.MinValue;
                template.Max = nuint.MaxValue;
                template.Regex = $@"^(\+)?\d{(template.Required ? "+" : "*")}$";
                template.Step = 1;
            }
            else if (propertyType.IsAssignableTo(typeof(DateTime?)))
            {
                template.Type = PropertyType.DatetimeLocal;
            }
            else if (propertyType.IsAssignableTo(typeof(DateTimeOffset?)))
            {
                template.Type = PropertyType.DatetimeOffset;
            }
            else if (propertyType.IsAssignableTo(typeof(DateOnly?)))
            {
                template.Type = PropertyType.Date;
            }
            else if (propertyType.IsAssignableTo(typeof(TimeSpan?)))
            {
                template.Type = PropertyType.Duration;
            }
            else if (propertyType.IsAssignableTo(typeof(TimeOnly?)))
            {
                template.Type = PropertyType.Time;
            }
            else if (propertyType.IsAssignableTo(typeof(byte[])) && string.Equals(property.Name, "timestamp", StringComparison.OrdinalIgnoreCase))
            {
                template.Type = PropertyType.Hidden;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(IDictionary<,>)))
            {
                template.Type = PropertyType.Collection;
                var collectionKeyType = propertyType.GetGenericArguments()[0];
                var collectionValueType = propertyType.GetGenericArguments()[1];
                var kvpType = typeof(KeyValuePair<,>).MakeGenericType(collectionKeyType, collectionValueType);
                template.Templates = await formTemplateFactory.CreateTemplatesWithDefaultEntryAsync(kvpType, null, template.Prompt);
            }
            else if (propertyType.IsGenericType && propertyType.IsAssignableTo(typeof(IEnumerable)))
            {
                template.Type = PropertyType.Collection;

                // If a collection has a foreign key, it is a multi select input and we do not want
                // templates for those.
                if (property.GetCustomAttribute<ForeignKeyAttribute>() is null)
                {
                    var collectionDtoType = propertyType.GetGenericArguments()[0];
                    template.Templates = await formTemplateFactory.CreateTemplatesWithDefaultEntryAsync(collectionDtoType, null, template.Prompt);
                }
            }
            else if (!propertyType.IsAssignableTo(typeof(HalFile)))
            {
                template.Type = PropertyType.Object;
                template.Templates = await formTemplateFactory.CreateTemplatesWithDefaultEntryAsync(propertyType, null, template.Prompt);
            }
        }
    }
}