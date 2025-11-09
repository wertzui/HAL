using HAL.Common.Forms;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HAL.Common.Converters;

/// <summary>
/// A factory to instantiate a <see cref="FormsResourceOfTJsonConverter{T}"/> with the correct type.
/// </summary>
/// <seealso cref="JsonConverterFactory" />
public class FormsResourceOfTJsonConverterFactory : JsonConverterFactory, IJsonTypeInfoResolver
{
    private static readonly Type _converterType = typeof(FormsResourceOfTJsonConverter<>);
    private static readonly Type _formsResourceType = typeof(FormsResource<>);

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var generic = typeToConvert.GetGenericTypeDefinition();

        return generic == _formsResourceType;
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var stateType = typeToConvert.GetGenericArguments()[0];

        var converter = (JsonConverter?)Activator.CreateInstance(
            _converterType.MakeGenericType(stateType));

        return converter ?? throw new ArgumentException($"{nameof(typeToConvert)} is not of type Resource<T>.", nameof(typeToConvert));
    }

    /// <inheritdoc/>
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        if (!type.IsGenericType)
            return null;

        var genericType = type.GetGenericTypeDefinition();
        if (genericType != typeof(FormsResource<>))
            return null;

        var stateType = type.GetGenericArguments()[0];

        var typeInfo = JsonTypeInfo.CreateJsonTypeInfo(type, options);

        var resourceProperties = typeof(FormsResource).GetProperties();
        var stateProperties = stateType.GetProperties();

        foreach (var property in stateProperties)
        {
            var propertyName = ConverterUtils.GetPropertyName(property, options.PropertyNamingPolicy);
            typeInfo.AddJsonPropertyInfo(property, propertyName);
        }

        foreach (var property in resourceProperties)
        {
            var propertyName = ConverterUtils.GetPropertyName(property, options.PropertyNamingPolicy);
            typeInfo.AddJsonPropertyInfo(property, propertyName);
        }

        typeInfo.SetConverter(CreateConverter(type, options));

        return typeInfo;
    }
}