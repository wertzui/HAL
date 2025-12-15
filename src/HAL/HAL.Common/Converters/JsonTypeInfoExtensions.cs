using System.Linq;
using System.Reflection;

namespace System.Text.Json.Serialization.Metadata;

/// <summary>
/// Provides extension methods for <see cref="JsonTypeInfo"/> to simplify adding and creating <see cref="JsonPropertyInfo"/> instances.
/// These methods handle property metadata, nullability, and custom naming for JSON serialization.
/// </summary>
public static class JsonTypeInfoExtensions
{
    private static readonly FieldInfo _converterBackingField = typeof(JsonTypeInfo).GetField($"<{nameof(JsonTypeInfo.Converter)}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Cannot find backing field for JsonTypeInfo.Converter.");

    [field: ThreadStatic]
    private static NullabilityInfoContext NullabilityContext => field ??= new NullabilityInfoContext();

    /// <summary>
    /// Adds a JSON property to the type info with explicit property type, name, and separate get/set nullability.
    /// Use this when you need different nullability for reading vs writing, or when working with dynamic properties.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, Type propertyType, string propertyName, bool isGetNullable, bool isSetNullable)
    {
        var propertyInfo = typeInfo.CreateJsonPropertyInfo(propertyType, propertyName, isGetNullable, isSetNullable);
        typeInfo.Properties.Add(propertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info with explicit property type, name, and unified nullability.
    /// Use this when the property has the same nullability for both reading and writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, Type propertyType, string propertyName, bool isNullable)
        => typeInfo.AddJsonPropertyInfo(propertyType, propertyName, isNullable, isNullable);

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/>, inferring nullability from the property's metadata.
    /// Use this when mapping existing CLR properties to JSON and you want to respect nullable reference type annotations.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo(propertyInfo);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a custom JSON name.
    /// Use this when you want to map a CLR property to a different JSON property name while respecting nullability annotations.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo(propertyInfo, name);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with explicit separate get/set nullability.
    /// Use this when you need to override the inferred nullability with different values for reading vs writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isGetNullable, bool isSetNullable)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo(propertyInfo, isGetNullable, isSetNullable);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a custom JSON name and explicit separate get/set nullability.
    /// Use this when you need both a custom JSON name and to override nullability with different values for reading vs writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isGetNullable, bool isSetNullable)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo(propertyInfo, name, isGetNullable, isSetNullable);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with explicit unified nullability.
    /// Use this when you need to override the inferred nullability with the same value for both reading and writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isNullable)
        => typeInfo.AddJsonPropertyInfo(propertyInfo, isNullable, isNullable);

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a custom JSON name and explicit unified nullability.
    /// Use this when you need both a custom JSON name and to override nullability with the same value for reading and writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    public static void AddJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isNullable)
        => typeInfo.AddJsonPropertyInfo(propertyInfo, name, isNullable, isNullable);

    /// <summary>
    /// Adds a JSON property to the type info with a generic type parameter, property name, and separate get/set nullability.
    /// Use this when you know the property type at compile time and need different nullability for reading vs writing.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, string propertyName, bool isGetNullable, bool isSetNullable)
        => typeInfo.AddJsonPropertyInfo(typeof(T), propertyName, isGetNullable, isSetNullable);

    /// <summary>
    /// Adds a JSON property to the type info with a generic type parameter, property name, and unified nullability.
    /// Use this when you know the property type at compile time and the property has the same nullability for both reading and writing.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, string propertyName, bool isNullable)
        => typeInfo.AddJsonPropertyInfo<T>(propertyName, isNullable, isNullable);

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a generic type constraint.
    /// Use this when you want compile-time type safety while mapping a CLR property and respecting nullability annotations.
    /// The generic type parameter is primarily for type validation and doesn't affect the serialization behavior.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo<T>(propertyInfo);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a generic type constraint and custom JSON name.
    /// Use this when you want compile-time type safety, a custom JSON name, and respect for nullability annotations.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo<T>(propertyInfo, name);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a generic type constraint and explicit separate get/set nullability.
    /// Use this when you want compile-time type safety and need to override nullability with different values for reading vs writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isGetNullable, bool isSetNullable)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo<T>(propertyInfo, isGetNullable, isSetNullable);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a generic type constraint, custom JSON name, and explicit separate get/set nullability.
    /// Use this when you want compile-time type safety, a custom JSON name, and need to override nullability with different values for reading vs writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isGetNullable, bool isSetNullable)
    {
        var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo<T>(propertyInfo, name, isGetNullable, isSetNullable);
        typeInfo.Properties.Add(jsonPropertyInfo);
    }

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a generic type constraint and explicit unified nullability.
    /// Use this when you want compile-time type safety and need to override nullability with the same value for both reading and writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isNullable)
        => typeInfo.AddJsonPropertyInfo<T>(propertyInfo, isNullable, isNullable);

    /// <summary>
    /// Adds a JSON property to the type info based on a <see cref="PropertyInfo"/> with a generic type constraint, custom JSON name, and explicit unified nullability.
    /// Use this when you want compile-time type safety, a custom JSON name, and need to override nullability with the same value for reading and writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to add the property to.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    public static void AddJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isNullable)
        => typeInfo.AddJsonPropertyInfo<T>(propertyInfo, name, isNullable, isNullable);

    /// <summary>
    /// Creates a JSON property info with explicit property type, name, and separate get/set nullability.
    /// Use this when you need to create a property without immediately adding it, or need different nullability for reading vs writing.
    /// Note: This creates a property with null getter and setter that do nothing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, Type propertyType, string propertyName, bool isGetNullable, bool isSetNullable)
    {
        var propertyInfo = typeInfo.CreateJsonPropertyInfo(propertyType, propertyName);
        propertyInfo.Get = (obj) => null;
        propertyInfo.Set = (obj, value) => { };
        propertyInfo.IsGetNullable = isGetNullable;
        propertyInfo.IsSetNullable = isSetNullable;

        return propertyInfo;
    }

    /// <summary>
    /// Creates a JSON property info with explicit property type, name, and unified nullability.
    /// Use this when you need to create a property without immediately adding it and the property has the same nullability for both reading and writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, Type propertyType, string propertyName, bool isNullable)
        => typeInfo.CreateJsonPropertyInfo(propertyType, propertyName, isNullable, isNullable);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/>, inferring nullability from the property's metadata.
    /// Use this when you need to create a property without immediately adding it and want to respect nullable reference type annotations.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo)
    {
        var nullabilityInfo = NullabilityContext.Create(propertyInfo);
        return typeInfo.CreateJsonPropertyInfo(
            propertyInfo.PropertyType,
            propertyInfo.Name,
            nullabilityInfo.ReadState == NullabilityState.Nullable,
            nullabilityInfo.WriteState == NullabilityState.Nullable);
    }

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a custom JSON name.
    /// Use this when you need to create a property without immediately adding it, want a custom JSON name, and want to respect nullability annotations.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name)
    {
        var nullabilityInfo = NullabilityContext.Create(propertyInfo);
        return typeInfo.CreateJsonPropertyInfo(
            propertyInfo.PropertyType,
            name ?? propertyInfo.Name,
            nullabilityInfo.ReadState == NullabilityState.Nullable,
            nullabilityInfo.WriteState == NullabilityState.Nullable);
    }

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with explicit separate get/set nullability.
    /// Use this when you need to create a property without immediately adding it and need to override nullability with different values for reading vs writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isGetNullable, bool isSetNullable)
    {
        return typeInfo.CreateJsonPropertyInfo(
            propertyInfo.PropertyType,
            propertyInfo.Name,
            isGetNullable,
            isSetNullable);
    }

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a custom JSON name and explicit separate get/set nullability.
    /// Use this when you need to create a property without immediately adding it and need both a custom JSON name and to override nullability with different values for reading vs writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isGetNullable, bool isSetNullable)
    {
        return typeInfo.CreateJsonPropertyInfo(
            propertyInfo.PropertyType,
            name ?? propertyInfo.Name,
            isGetNullable,
            isSetNullable);
    }

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with explicit unified nullability.
    /// Use this when you need to create a property without immediately adding it and need to override nullability with the same value for both reading and writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isNullable)
        => typeInfo.CreateJsonPropertyInfo(propertyInfo, isNullable, isNullable);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a custom JSON name and explicit unified nullability.
    /// Use this when you need to create a property without immediately adding it and need both a custom JSON name and to override nullability with the same value for reading and writing.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isNullable)
        => typeInfo.CreateJsonPropertyInfo(propertyInfo, name, isNullable, isNullable);

    /// <summary>
    /// Creates a JSON property info with a generic type parameter, property name, and separate get/set nullability.
    /// Use this when you know the property type at compile time, need to create a property without immediately adding it, and need different nullability for reading vs writing.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, string propertyName, bool isGetNullable, bool isSetNullable)
        => typeInfo.CreateJsonPropertyInfo(typeof(T), propertyName, isGetNullable, isSetNullable);

    /// <summary>
    /// Creates a JSON property info with a generic type parameter, property name, and unified nullability.
    /// Use this when you know the property type at compile time, need to create a property without immediately adding it, and the property has the same nullability for both reading and writing.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, string propertyName, bool isNullable)
        => typeInfo.CreateJsonPropertyInfo<T>(propertyName, isNullable, isNullable);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a generic type constraint.
    /// Use this when you want compile-time type safety, need to create a property without immediately adding it, and want to respect nullability annotations.
    /// The generic type parameter is primarily for type validation and doesn't affect the serialization behavior.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo)
        => typeInfo.CreateJsonPropertyInfo(propertyInfo);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a generic type constraint and custom JSON name.
    /// Use this when you want compile-time type safety, need to create a property without immediately adding it, want a custom JSON name, and want to respect nullability annotations.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name)
        => typeInfo.CreateJsonPropertyInfo(propertyInfo, name);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a generic type constraint and explicit separate get/set nullability.
    /// Use this when you want compile-time type safety, need to create a property without immediately adding it, and need to override nullability with different values for reading vs writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isGetNullable, bool isSetNullable)
        => typeInfo.CreateJsonPropertyInfo(propertyInfo, isGetNullable, isSetNullable);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a generic type constraint, custom JSON name, and explicit separate get/set nullability.
    /// Use this when you want compile-time type safety, need to create a property without immediately adding it, need a custom JSON name, and need to override nullability with different values for reading vs writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isGetNullable">Whether the property can be null when reading (deserialization).</param>
    /// <param name="isSetNullable">Whether the property can be null when writing (serialization).</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isGetNullable, bool isSetNullable)
        => typeInfo.CreateJsonPropertyInfo(propertyInfo, name, isGetNullable, isSetNullable);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a generic type constraint and explicit unified nullability.
    /// Use this when you want compile-time type safety, need to create a property without immediately adding it, and need to override nullability with the same value for both reading and writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, bool isNullable)
        => typeInfo.CreateJsonPropertyInfo<T>(propertyInfo, isNullable, isNullable);

    /// <summary>
    /// Creates a JSON property info based on a <see cref="PropertyInfo"/> with a generic type constraint, custom JSON name, and explicit unified nullability.
    /// Use this when you want compile-time type safety, need to create a property without immediately adding it, need a custom JSON name, and need to override nullability with the same value for reading and writing.
    /// </summary>
    /// <typeparam name="T">The expected type of the property (for type validation).</typeparam>
    /// <param name="typeInfo">The JSON type info to create the property for.</param>
    /// <param name="propertyInfo">The property info to create the JSON property from.</param>
    /// <param name="name">The custom JSON property name to use instead of the CLR property name.</param>
    /// <param name="isNullable">Whether the property can be null for both reading and writing.</param>
    /// <returns>A new <see cref="JsonPropertyInfo"/> instance.</returns>
    public static JsonPropertyInfo CreateJsonPropertyInfo<T>(this JsonTypeInfo typeInfo, PropertyInfo propertyInfo, string name, bool isNullable)
        => typeInfo.CreateJsonPropertyInfo<T>(propertyInfo, name, isNullable, isNullable);

    /// <summary>
    /// Sets a custom converter on a <see cref="JsonTypeInfo{T}"/>.
    /// Use this when you need to replace the default converter with a custom one for advanced serialization scenarios.
    /// This method uses reflection to set both the public Converter property and the internal EffectiveConverter backing field.
    /// </summary>
    /// <typeparam name="T">The type being serialized/deserialized.</typeparam>
    /// <param name="typeInfo">The JSON type info to set the converter on.</param>
    /// <param name="converter">The custom converter to use.</param>
    public static void SetConverter<T>(this JsonTypeInfo<T> typeInfo, JsonConverter<T> converter)
    {
        var type = typeof(JsonTypeInfo<T>);
        var _effectiveConverterBackingField = type.GetField("<EffectiveConverter>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Cannot find backing field for JsonTypeInfo<{type.Name}>.EffectiveConverter.");
        _effectiveConverterBackingField.SetValue(typeInfo, converter);
        _converterBackingField.SetValue(typeInfo, converter);
    }

    /// <summary>
    /// Sets a custom converter on a <see cref="JsonTypeInfo"/>.
    /// Use this when you need to replace the default converter with a custom one for advanced serialization scenarios.
    /// This method uses reflection to set both the public Converter property and the internal EffectiveConverter backing field.
    /// </summary>
    /// <param name="typeInfo">The JSON type info to set the converter on.</param>
    /// <param name="converter">The custom converter to use. This must not be a converter factory.</param>
    public static void SetConverter(this JsonTypeInfo typeInfo, JsonConverter converter)
    {
        var type = typeInfo.GetType();
        var _effectiveConverterBackingField = type.GetField("<EffectiveConverter>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (_effectiveConverterBackingField is not null)
        {
            var converterType = converter.GetType();
            var fieldType = _effectiveConverterBackingField.FieldType;
            if (!converter.GetType().IsAssignableTo(_effectiveConverterBackingField.FieldType))
                throw new InvalidOperationException($"Cannot set the JSON converter, because the generic types don't match. The JsonTypeInfo is {type.GetPrettyName()} which expects a converter of type {fieldType.GetPrettyName()}, but the passed in converter is of type {converterType.GetPrettyName()}");
            _effectiveConverterBackingField?.SetValue(typeInfo, converter);
        }
        _converterBackingField.SetValue(typeInfo, converter);
    }

    private static string GetPrettyName(this Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
        var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetPrettyName));
        return $"{genericTypeName}<{genericArgs}>";
    }
}
