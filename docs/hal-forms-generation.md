# HAL-Forms Generation

This document explains how HAL-Forms templates are generated from C# types, which attributes control the output, and how to plug in custom generation logic.

## Overview

When you call `IFormFactory.CreateFormAsync<T>(...)` or `CreateResourceForEndpointAsync<T>(...)`, the library reflects over every public instance property of `T` and builds a [`FormTemplate`](../src/HAL/HAL.Common/Forms/FormTemplate.cs) with one [`Property`](../src/HAL/HAL.Common/Forms/Property.cs) per field.

The pipeline has three extension points, each with a dedicated interface:

| Interface | Purpose |
|-----------|---------|
| `IFormsResourceGenerationCustomization` | Modify the `FormsResource` as a whole after it is created (e.g. add extra templates). |
| `IPropertyTemplateGenerationCustomization` | Control **which** properties are included and **what metadata** each property carries (type, validators, options, …). |
| `IPropertyValueGenerationCustomization` | Control the **runtime value** that is pre-filled into each property (e.g. reading from a DTO). |

The default implementations (`DefaultPropertyTemplateGeneration`, `DefaultPropertyValueGeneration`) are always registered and run at `Order = 0` unless you replace them with an exclusive customization.

---

## Property inclusion

`DefaultPropertyTemplateGeneration.IncludeAsync` excludes a property when **any** of the following is true:

| Condition | Attribute / convention |
|-----------|------------------------|
| `[JsonIgnore(Condition = JsonIgnoreCondition.Always)]` | Property is excluded from serialization entirely. |
| `[ScaffoldColumn(false)]` | Property should not appear in generated UI. |
| `[IgnoreDataMember]` | Property is marked for data-contract exclusion. |
| `[Display(AutoGenerateField = false)]` | Property is hidden from auto-generated UI. |

Any property that passes all four checks is included.

---

## Type inference from C# types

`DefaultPropertyTemplateGeneration` maps C# types to `PropertyType` values automatically. Nullable variants (`T?`) are handled identically — nullability only affects whether `Required` is set.

| C# type | `PropertyType` | Notes |
|---------|---------------|-------|
| `string` | `Text` | |
| `bool` / `bool?` | `Bool` | |
| `enum` | *(no type set)* | Generates inline `Options` from `Enum.GetValues`. Single-select unless `[Flags]`. |
| `[Flags] enum` | *(no type set)* | Multi-select options. |
| `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint` | `Number` | `Min`/`Max` set to type bounds; `Step = 1`; integer `Regex`. |
| `float`, `double`, `decimal` | `Number` | `Min`/`Max` set to type bounds; `Step = 0.01`; decimal `Regex`. |
| `char` | `Text` | `MinLength = 0` (or `1` if required), `MaxLength = 1`. |
| `DateTime` | `DatetimeLocal` | |
| `DateTimeOffset` | `DatetimeOffset` | |
| `DateOnly` | `Date` | |
| `TimeOnly` | `Time` | |
| `TimeSpan` | `Duration` | |
| `byte[]` with name `"Timestamp"` (case-insensitive) | `Hidden` | EF Core row-version pattern. |
| `IDictionary<TKey, TValue>` | `Collection` | Nested templates use `KeyValuePair<TKey,TValue>`. |
| `IEnumerable<T>` (without `[ForeignKey]`) | `Collection` | Nested templates use `T`. |
| `IEnumerable<T>` with `[ForeignKey]` | *(options link)* | Multi-select foreign-key list; no nested templates. |
| Any other complex type | `Object` | Nested templates generated recursively for that type. |
| `HalFile` | *(no type, no templates)* | Treated as a binary file upload; handled separately. |

**Nullability**: A non-nullable property (`string name`, `int id`) gets `Required = true`. A nullable property (`string? name`, `int? id`) gets `Required = false`.

**`id` / `Id` properties** automatically get `ReadOnly = true` regardless of type.

> **JSON serialization of `PropertyType`**: enum values are serialized as camelCase strings (e.g. `Number` → `"number"`, `Bool` → `"bool"`, `Collection` → `"collection"`). Two values have `[EnumMember]` overrides: `DatetimeLocal` → `"datetime-local"` and `DatetimeOffset` → `"datetime-offset"`.

---

## Attribute reference

All attributes below are read by `DefaultPropertyTemplateGeneration.AddAttributeInformation`. They are applied **after** the type-based defaults, so they can override them.

### `[Required]`

Sets `Property.Required = true`.

```csharp
[Required]
public string Name { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "name", "type": "text", "required": true }
```

### `[Display]`

Sets `Property.Prompt` (the label shown to the user) from `Display.GetName()` and `Property.Placeholder` from `Display.GetPrompt()`.

```csharp
[Display(Name = "Full name", Prompt = "Enter your full name")]
public string Name { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "name", "type": "text", "prompt": "Full name", "placeholder": "Enter your full name" }
```

### `[DisplayName]`

Alternative to `[Display]`. Sets `Property.Prompt` from `DisplayName.DisplayName`.

```csharp
[DisplayName("Full name")]
public string Name { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "name", "type": "text", "prompt": "Full name" }
```

### `[DataType]`

Maps the `DataType` enum value to a `PropertyType`. Also sets a sensible `Regex` for several types.

| `DataType` value | `PropertyType` | Regex added? |
|-----------------|---------------|--------------|
| `DateTime` | `DatetimeLocal` | No |
| `Date` | `Date` | No |
| `Time` | `Time` | No |
| `Duration` | `Duration` | No |
| `PhoneNumber` | `Tel` | Yes |
| `Currency` | `Currency` | No |
| `Text` | `Text` | No |
| `Html` | `Textarea` | No |
| `MultilineText` | `Textarea` | No |
| `EmailAddress` | `Email` | Yes |
| `Password` | `Password` | No |
| `Url` | `Url` | Yes |
| `ImageUrl` | `Image` | Yes |
| `CreditCard` | `Text` | Yes |
| `PostalCode` | `Text` | Yes |
| `Upload` | `File` | No |
| `Custom` | parsed as `PropertyType` enum name | No |

```csharp
[DataType(DataType.MultilineText)]
public string Bio { get; set; } = "";

[DataType(DataType.Password)]
public string Password { get; set; } = "";
```

For `DataType.Custom`, the custom string is parsed case-insensitively as a `PropertyType` enum name:

```csharp
[DataType("Currency")]     // → PropertyType.Currency
public decimal Price { get; set; }
```

**JSON outcome** (example for `DataType.MultilineText` and `DataType.EmailAddress`):

```json
{ "name": "bio", "type": "textarea" }
{ "name": "email", "type": "email", "regex": "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"...)@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[...\\])" }
```

The full RFC-5321 email pattern is injected verbatim; the abbreviated form above is for readability only.

### `[UIHint]`

Alternative to `[DataType(DataType.Custom, ...)]`. The hint string is parsed case-insensitively as a `PropertyType` enum name.

```csharp
[UIHint("Textarea")]
public string Notes { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "notes", "type": "textarea" }
```

### `[StringLength]`

Sets both `Property.MinLength` and `Property.MaxLength`.

```csharp
[StringLength(100, MinimumLength = 3)]
public string Username { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "username", "type": "text", "minLength": 3, "maxLength": 100 }
```

### `[MaxLength]`

Sets `Property.MaxLength`.

```csharp
[MaxLength(500)]
public string Description { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "description", "type": "text", "maxLength": 500 }
```

### `[MinLength]`

Sets `Property.MinLength`.

```csharp
[MinLength(8)]
public string Password { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "password", "type": "text", "minLength": 8 }
```

### `[Length]` (.NET 6+)

Sets both `Property.MinLength` and `Property.MaxLength` from a single attribute.

```csharp
[Length(2, 50)]
public string Code { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "code", "type": "text", "minLength": 2, "maxLength": 50 }
```

### `[Range]`

Sets `Property.Min` and `Property.Max`.

```csharp
[Range(1, 120)]
public int Age { get; set; }

[Range(0.01, 9999.99)]
public decimal Price { get; set; }
```

**JSON outcome:**

```json
{ "name": "age", "type": "number", "min": 1, "max": 120, "step": 1 }
{ "name": "price", "type": "number", "min": 0.01, "max": 9999.99, "step": 0.01 }
```

### `[RegularExpression]`

Sets `Property.Regex` to the given pattern, overriding any regex that was inferred from the type.

```csharp
[RegularExpression(@"^[A-Z]{2}\d{4}$")]
public string ProductCode { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "productCode", "type": "text", "regex": "^[A-Z]{2}\\d{4}$" }
```

### `[Step]`

Sets `Property.Step` — the increment between legal numeric values.

```csharp
using HAL.Common.Attributes;

[Step(0.5)]
public double Rating { get; set; }
```

This is a custom HAL attribute from `HAL.Common.Attributes`. For integer properties the step defaults to `1`; for floating-point it defaults to `0.01`.

**JSON outcome:**

```json
{ "name": "rating", "type": "number", "step": 0.5 }
```

### `[PromptDisplayType]`

Sets `Property.PromptDisplay`, controlling how the prompt label is rendered. Uses the `PropertyPromptDisplayType` enum.

| Value | Effect |
|-------|--------|
| `Visible` | Prompt is shown (default if omitted). |
| `Hidden` | Prompt is hidden — the field is present but unlabelled. |
| `Collapsed` | Prompt starts collapsed and can be expanded by the user. |

```csharp
using HAL.AspNetCore.Forms;

[PromptDisplayType(PropertyPromptDisplayType.Collapsed)]
public string Details { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "details", "type": "text", "promptDisplay": "collapsed" }
```

### `[Editable]`

Sets `Property.ReadOnly` to `!AllowEdit`.

```csharp
[Editable(false)]
public string CreatedBy { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "createdBy", "type": "text", "readOnly": true }
```

### `[ReadOnly]`

Sets `Property.ReadOnly` from `ReadOnlyAttribute.IsReadOnly` (`System.ComponentModel.ReadOnlyAttribute`).

```csharp
using System.ComponentModel;

[ReadOnly(true)]
public string CreatedBy { get; set; } = "";
```

**JSON outcome:**

```json
{ "name": "createdBy", "type": "text", "readOnly": true }
```

> Prefer `[Editable(false)]` for UI-driven scenarios; `[ReadOnly(true)]` is the lower-level `System.ComponentModel` equivalent.

### `[Key]`

Marks the property as `ReadOnly = true` and `Required = true`. A property named `Id` or `id` is also made read-only by the type-inference step without this attribute.

```csharp
[Key]
public int Id { get; set; }
```

**JSON outcome:**

```json
{ "name": "id", "type": "number", "readOnly": true, "required": true }
```

The actual `type` in the output depends on the property's C# type. For `Guid Id` the result would be `"type": "object"` because `Guid` has no dedicated `PropertyType` mapping.

### `[ConcurrencyCheck]`

Sets `Property.Type = PropertyType.Hidden`. The field is sent back with the form submission but is not shown to the user.

```csharp
[ConcurrencyCheck]
public int Version { get; set; }
```

**JSON outcome:**

```json
{ "name": "version", "type": "hidden" }
```

### `[Timestamp]`

Also sets `Property.Type = PropertyType.Hidden` (same as `[ConcurrencyCheck]`). Typically used on EF Core row-version `byte[]` columns.

```csharp
[Timestamp]
public byte[] RowVersion { get; set; } = [];
```

**JSON outcome:**

```json
{ "name": "rowVersion", "type": "hidden" }
```

### `[AllowedValues]`

Filters the inline `Options` of the property to only the listed values. If no inline options exist yet (e.g. the property is not an enum), creates them from the allowed values directly.

```csharp
[AllowedValues("small", "medium", "large")]
public string Size { get; set; } = "medium";
```

**JSON outcome:**

```json
{
  "name": "size",
  "options": {
    "inline": [
      { "prompt": "small", "value": "small" },
      { "prompt": "medium", "value": "medium" },
      { "prompt": "large", "value": "large" }
    ]
  }
}
```

> When `[AllowedValues]` is applied to an **enum** property it filters the options that were already created by type inference, which sets `maxItems` and `minItems`. When applied to a non-enum (e.g. `string`), it creates new inline options with no `maxItems` or `minItems` constraint.

### `[DeniedValues]`

Removes specific values from the inline `Options` of the property.

```csharp
public enum Status { Active, Inactive, Legacy }

[DeniedValues("legacy")]
public Status Status { get; set; }
```

**JSON outcome** (assuming `Status` enum; `Legacy` option is removed):

```json
{
  "name": "status",
  "options": {
    "inline": [
      { "prompt": "Active", "value": 0 },
      { "prompt": "Inactive", "value": 1 }
    ],
    "maxItems": 1
  }
}
```

### `[ForeignKey]`

Turns the property into a foreign-key selector. The library looks for a registered `IForeignKeyLinkFactory` that can serve the referenced type and, if found, replaces `Type` with a linked `Options` (containing `ValueField`, `PromptField`, and a `Link` to the list endpoint).

```csharp
// Convention: property name ending in "Id" also triggers this automatically
public int CategoryId { get; set; }

// Explicit via attribute
[ForeignKey(nameof(Category))]
public int CategoryId { get; set; }
public CategoryDto? Category { get; set; }
```

**JSON outcome** (when a matching `IForeignKeyLinkFactory` is registered):

```json
{
  "name": "categoryId",
  "options": {
    "link": { "href": "/api/categories" },
    "valueField": "id",
    "promptField": "name",
    "maxItems": 1
  }
}
```

If no factory is found, the property falls back to a plain `number` input.

See [Implementing `IForeignKeyLinkFactory`](#implementing-iforeignkeylinkfactory) below.

### `[FileExtensions]`

Sets `Property.Type = PropertyType.File` and `Property.Placeholder` to the allowed extensions string. `FileExtensionsAttribute` already extends `DataTypeAttribute(DataType.Upload)`, so adding `[DataType(DataType.Upload)]` is redundant.

```csharp
[FileExtensions(Extensions = "jpg,png,gif")]
public IFormFile? Avatar { get; set; }
```

**JSON outcome:**

```json
{ "name": "avatar", "type": "file", "placeholder": "jpg,png,gif" }
```

### Extension data attributes (`IPropertyExtensionData`)

Any attribute that implements `HAL.Common.Forms.IPropertyExtensionData` has its data automatically serialized into `Property.Extensions`. The key is the attribute class name (without the `Attribute` suffix), camel-cased. This lets you attach domain-specific metadata to a property without modifying the HAL-Forms spec.

```csharp
// Define a custom attribute
[AttributeUsage(AttributeTargets.Property)]
public class HelpTextAttribute : Attribute, IPropertyExtensionData
{
    public HelpTextAttribute(string text) => Text = text;
    public string Text { get; }
}

// Apply it to a property
[HelpText("Enter your date of birth in ISO format.")]
public DateOnly BirthDate { get; set; }
```

**JSON outcome:**

```json
{
  "name": "birthDate",
  "type": "date",
  "helpText": { "text": "Enter your date of birth in ISO format." }
}
```

The extension key is the attribute class name without the `Attribute` suffix, camel-cased. Here `HelpTextAttribute` → `"helpText"`.

---

## Default value for non-editable required properties

When a property is both `Required` and either `ReadOnly` or `Hidden`, and no value has been set yet, the library instantiates the DTO type with `Activator.CreateInstance` and reads the default value from it. This ensures the client can post the form back without triggering server-side validation errors for fields the user cannot see or edit (e.g. a required `Id` field on an update form for a newly created item inside a collection).

---

## Implementing `IForeignKeyLinkFactory`

Foreign-key properties need a link that tells the client where to fetch the list of selectable options. Implement `IForeignKeyLinkFactory` and register it in DI:

```csharp
public class CategoryForeignKeyLinkFactory : IForeignKeyLinkFactory
{
    private readonly ILinkFactory _linkFactory;

    public CategoryForeignKeyLinkFactory(ILinkFactory linkFactory)
        => _linkFactory = linkFactory;

    public bool CanCreateLink(Type listDtoType) => listDtoType == typeof(CategoryDto);

    public OptionsLink? CreateLink(Type listDtoType)
    {
        var link = _linkFactory.Create(action: "GetList", controller: "Categories");
        return new OptionsLink(link.Href);
    }
}

// Program.cs
builder.Services.AddSingleton<IForeignKeyLinkFactory, CategoryForeignKeyLinkFactory>();
```

The library picks the first factory whose `CanCreateLink` returns `true`. If none is found for a foreign-key property, the options link is omitted and the property falls back to a plain text/number input.

---

## Custom template generation

There are three customization interfaces. Register them in `Program.cs` using the host-builder extensions:

```csharp
builder.AddPropertyTemplateGenerationCustomization<MyTemplateCustomization>();
builder.AddPropertyValueGenerationCustomization<MyValueCustomization>();
builder.AddFormsResourceGenerationCustomization<MyResourceCustomization>();
```

Multiple customizations of the same type run in `Order` order (ascending). Set `Exclusive = true` to **replace** the default logic entirely for matched properties/resources; leave it `false` (the default) to **augment** it.

### `IPropertyTemplateGenerationCustomization`

Implement this to change the template metadata for specific properties.

| Member | Description |
|--------|-------------|
| `bool Exclusive` | `true` → skip `DefaultPropertyTemplateGeneration` for matched properties. |
| `int Order` | Execution order among non-exclusive customizations. |
| `bool AppliesTo(PropertyInfo, Property)` | Return `true` only for the properties you want to handle. |
| `ValueTask<bool> IncludeAsync(PropertyInfo, Property, IFormTemplateFactory)` | Return `false` to exclude the property from the template entirely. |
| `ValueTask ApplyAsync(PropertyInfo, Property, IFormTemplateFactory)` | Modify the `Property` object. |

**Example — mark all `InternalCode` properties as hidden:**

```csharp
public class HideInternalCodeCustomization : IPropertyTemplateGenerationCustomization
{
    public bool Exclusive => false;
    public int Order => 10;

    public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty)
        => propertyInfo.Name == "InternalCode";

    public ValueTask<bool> IncludeAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory _)
        => ValueTask.FromResult(true); // still include it, just change the type

    public ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory _)
    {
        halFormsProperty.Type = PropertyType.Hidden;
        return ValueTask.CompletedTask;
    }
}
```

**Example — inject a dynamic option list for a `CountryCode` property:**

```csharp
public class CountryCodeOptionsCustomization : IPropertyTemplateGenerationCustomization
{
    private readonly ICountryService _countries;

    public CountryCodeOptionsCustomization(ICountryService countries) => _countries = countries;

    public bool Exclusive => false;
    public int Order => 0;

    public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty)
        => propertyInfo.Name == "CountryCode";

    public ValueTask<bool> IncludeAsync(PropertyInfo _, Property __, IFormTemplateFactory ___)
        => ValueTask.FromResult(true);

    public ValueTask ApplyAsync(PropertyInfo _, Property halFormsProperty, IFormTemplateFactory __)
    {
        var items = _countries.GetAll()
            .Select(c => new OptionsItem<object?>(c.Name, c.Code))
            .ToList();

        halFormsProperty.Type = null; // Type and Options are mutually exclusive
        halFormsProperty.Options = new Options<object?>(items) { MaxItems = 1, MinItems = 1 };
        return ValueTask.CompletedTask;
    }
}
```

### `IPropertyValueGenerationCustomization`

Implement this to control what value is pre-filled into each property at runtime (i.e. when filling the form with an existing DTO).

| Member | Description |
|--------|-------------|
| `bool Exclusive` | `true` → skip `DefaultPropertyValueGeneration` for matched properties. |
| `int Order` | Execution order. |
| `bool AppliesTo(PropertyInfo, Property)` | Return `true` only for the properties you want to handle. |
| `ValueTask ApplyAsync(PropertyInfo, Property, object? propertyValue, object? dtoValue, IFormValueFactory)` | Modify `halFormsProperty.Value` or `halFormsProperty.Options.SelectedValues`. |

**Example — mask a sensitive value:**

```csharp
public class MaskPasswordValueCustomization : IPropertyValueGenerationCustomization
{
    public bool Exclusive => true; // replace default value-filling for this property
    public int Order => 0;

    public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty)
        => halFormsProperty.Type == PropertyType.Password;

    public ValueTask ApplyAsync(PropertyInfo _, Property halFormsProperty, object? __, object? ___, IFormValueFactory ____)
    {
        halFormsProperty.Value = null; // never echo passwords back to the client
        return ValueTask.CompletedTask;
    }
}
```

### `IFormsResourceGenerationCustomization`

Implement this to modify the `FormsResource` as a whole — for example to add an additional template alongside the auto-generated `"default"` template.

| Member | Description |
|--------|-------------|
| `bool Exclusive` | `true` → skip the default resource generation. |
| `int Order` | Execution order. |
| `bool AppliesTo<TDto>(FormsResource, TDto, HttpMethod, string, string, string, string?, object?)` | Return `true` only for specific DTO types, HTTP methods, or actions. |
| `ValueTask ApplyAsync<TDto>(FormsResource, TDto, HttpMethod, string, string, string, string?, object?, IFormFactory)` | Modify the `FormsResource` (e.g. add templates, change the self link). |

**Example — add a `"delete"` template to every `PUT` form:**

```csharp
public class AddDeleteTemplateCustomization : IFormsResourceGenerationCustomization
{
    public bool Exclusive => false;
    public int Order => 0;

    public bool AppliesTo<TDto>(
        FormsResource formsResource, TDto value, 
        HttpMethod method, string title, string contentType,
        string action, string? controller, object? routeValues)
        => method == HttpMethod.Put;

    public async ValueTask ApplyAsync<TDto>(
        FormsResource formsResource, TDto value,
        HttpMethod method, string title, string contentType,
        string action, string? controller, object? routeValues,
        IFormFactory formFactory)
    {
        var deleteTemplate = await formFactory.CreateFormAsync(
            value,
            target: formsResource.Links?["self"]?.FirstOrDefault()?.Href ?? "",
            method: HttpMethod.Delete,
            title: "Delete");

        formsResource.Templates ??= new Dictionary<string, FormTemplate?>();
    }
}
```

---

## Execution order summary

```text
IFormFactory.CreateResourceForEndpointAsync<T>(value, ...)
│
├─ For each property of T:
│   ├─ DefaultPropertyTemplateGeneration.IncludeAsync   → include/exclude
│   ├─ [custom IPropertyTemplateGenerationCustomization].IncludeAsync (if Order < 0)
│   │
│   ├─ DefaultPropertyTemplateGeneration.ApplyAsync     → type, validators, options
│   └─ [custom IPropertyTemplateGenerationCustomization].ApplyAsync (Order ascending)
│       (skipped for matched properties if any registered customization is Exclusive)
│
├─ For each property of T (value filling):
│   ├─ DefaultPropertyValueGeneration.ApplyAsync        → pre-fill from DTO
│   └─ [custom IPropertyValueGenerationCustomization].ApplyAsync (Order ascending)
│
└─ FormsResource assembled
    ├─ [custom IFormsResourceGenerationCustomization].AppliesTo / ApplyAsync
    └─ Final FormsResource returned
```
