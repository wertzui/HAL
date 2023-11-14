﻿using HAL.Common.Forms;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters;

/// <summary>
/// A factory to instantiate a <see cref="ResourceJsonConverter{T}"/> with the correct type.
/// </summary>
/// <seealso cref="JsonConverterFactory" />
public class ResourceOfTJsonConverterFactory : JsonConverterFactory
{
    private static readonly Type _converterType = typeof(ResourceJsonConverter<>);
    private static readonly Type _resourceType = typeof(Resource<>);
    private static readonly Type _formsResourceType = typeof(FormsResource<>);

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var generic = typeToConvert.GetGenericTypeDefinition();

        return generic == _resourceType || generic == _formsResourceType;
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var stateType = typeToConvert.GetGenericArguments()[0];

        var converter = (JsonConverter?)Activator.CreateInstance(
            _converterType.MakeGenericType(stateType));

        return converter ?? throw new ArgumentException($"{nameof(typeToConvert)} is not of type Resource<T>.", nameof(typeToConvert));
    }
}