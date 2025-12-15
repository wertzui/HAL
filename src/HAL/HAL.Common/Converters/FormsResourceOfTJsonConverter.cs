using HAL.Common.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Common.Converters;

/// <summary>
/// Caches reflection related information to be used by the
/// <see cref="FormsResourceOfTJsonConverter{T}"/> class.
/// </summary>
public static class FormsResourceJsonConverterCache
{
    private static readonly ConcurrentDictionary<Type, IDictionary<ConstructorInfo, Dictionary<string, ParameterInfo>>> _constructorCache = new();
    private static readonly ConcurrentDictionary<Type, IDictionary<string, PropertyInfo>> _propertyCacheWithCorrectCasing = new();
    private static readonly ConcurrentDictionary<Type, IDictionary<string, PropertyInfo>> _propertyCacheWithLowerCasing = new();
    private static readonly ConcurrentDictionary<Type, bool> _stateCanBeNullCache = new();

    [field: ThreadStatic]
    private static NullabilityInfoContext NullabilityContext => field ??= new NullabilityInfoContext();

    /// <summary>
    /// Gets all constructors and their parameters by their lower cased names.
    /// </summary>
    /// <param name="stateType">The type to get constructors for.</param>
    public static IDictionary<ConstructorInfo, Dictionary<string, ParameterInfo>> GetConstructors(Type stateType)
    {
        return _constructorCache.GetOrAdd(
            stateType,
            t => t
                .GetConstructors()
                .ToDictionary(c => c, c => c.GetParameters().ToDictionary(p => p.Name!.ToLowerInvariant())));
    }

    /// <summary>
    /// Gets all properties by their correct cased and their lower cased names. If two
    /// properties share the same lower cased name, only one of them will be returned and it is
    /// not determined which one.
    /// </summary>
    /// <param name="stateType">The type to get properties for.</param>
    public static (IDictionary<string, PropertyInfo> PropertiesWithCorrectCasing, IDictionary<string, PropertyInfo> PropertiesWithLowerCasing) GetProperties(Type stateType)
    {
        var propertiesWithCorrectCasing = _propertyCacheWithCorrectCasing.GetOrAdd(
            stateType,
            t => t.GetProperties().ToDictionary(p => p.Name));
        var propertiesWithLowerCasing = _propertyCacheWithLowerCasing.GetOrAdd(
            stateType,
            _ => propertiesWithCorrectCasing.Select(p => new { Key = p.Key.ToLowerInvariant(), p.Value }).DistinctBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));

        return (propertiesWithCorrectCasing, propertiesWithLowerCasing);
    }

    /// <summary>
    /// Returns if the <see cref="Resource{TState}.State"/> property can be null for the given <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>Whether the state can be null or not.</returns>
    public static bool StateCanBeNull<TState>()
    {
        var resourceType = typeof(FormsResource<TState>);

        return _stateCanBeNullCache.GetOrAdd(resourceType, t =>
        {
            var stateProperty = t.GetProperty(nameof(FormsResource<TState>.State)) ?? throw new ArgumentException($"Unable to find the State property on Resource<{typeof(TState).Name}>.");
            var nullabilityInfo = NullabilityContext.Create(stateProperty);
            var stateMustNotBeNull = nullabilityInfo.WriteState is NullabilityState.NotNull;
            return stateMustNotBeNull;
        });
    }

    /// <summary>
    /// Gets a property by its name. First try to get it with correct casing and if none is
    /// found, it tries to get it by ignoring the case.
    /// </summary>
    /// <param name="stateType">The type to get the property for.</param>
    /// <param name="propertyName">The name of the property in correct casing.</param>
    /// <param name="property">The property if found</param>
    /// <returns>Whether the property has been found or not.</returns>
    public static bool TryGetProperty(Type stateType, string propertyName, [MaybeNullWhen(false)] out PropertyInfo property)
    {
        var (propertiesWithCorrectCasing, propertiesWithLowerCasing) = GetProperties(stateType);

        return propertiesWithCorrectCasing.TryGetValue(propertyName, out property) ||
            propertiesWithLowerCasing.TryGetValue(propertyName.ToLowerInvariant(), out property);
    }
}

/// <summary>
/// A converter that can read and write <see cref="Resource{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the state of the resource.</typeparam>
/// <seealso cref="JsonConverter{T}"/>
public class FormsResourceOfTJsonConverter<T> : JsonConverter<FormsResource<T>>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return
            typeToConvert == typeof(FormsResource);
    }

    /// <inheritdoc/>
    public override FormsResource<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        FormsResource<T> resource;
        var stateType = typeof(T);
        IDictionary<string, ICollection<Link>>? links = default;
        IDictionary<string, ICollection<Resource>>? embedded = default;
        IDictionary<string, FormTemplate>? templates = default;
        var stateProperties = new Dictionary<string, object?>();
        var stateConstructorArguments = new Dictionary<string, object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                var state = CreateState(stateProperties, stateConstructorArguments);

                templates ??= new Dictionary<string, FormTemplate>();

                resource = new FormsResource<T>(templates) { State = state };

                if (embedded is not null)
                    resource.Embedded = embedded;

                if (links is not null)
                    resource.Links = links;

                return resource;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Mal-formated JSON input. Expected a property name, but got {reader.TokenType}.");
            }

            var propertyName = reader.GetString() ?? throw new JsonException("Mal-formated JSON input. Received an empty property name.");
            if (propertyName == Constants.EmbeddedPropertyName)
            {
                embedded = JsonSerializer.Deserialize<IDictionary<string, ICollection<Resource>>>(ref reader, options);
            }
            else if (propertyName == Constants.LinksPropertyName)
            {
                links = JsonSerializer.Deserialize<IDictionary<string, ICollection<Link>>>(ref reader, options);
            }
            else if (propertyName == Constants.FormTemplatesPropertyName)
            {
                templates = JsonSerializer.Deserialize<IDictionary<string, FormTemplate>>(ref reader, options);
            }
            else
            {
                // Try to map by constructor parameter
                var lowerPropertyName = propertyName.ToLowerInvariant();
                if (TryGetConstructorParameter(stateType, lowerPropertyName, out var constructorParameter))
                {
                    stateConstructorArguments[constructorParameter.Name!.ToLowerInvariant()] = JsonSerializer.Deserialize(ref reader, constructorParameter.ParameterType, options);
                    continue;
                }

                // Try to map by property
                if (FormsResourceJsonConverterCache.TryGetProperty(stateType, propertyName, out var property))
                {
                    stateProperties[property.Name] = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    continue;
                }

                // Otherwise ignore it
                reader.Skip();
            }
        }

        throw new JsonException("Mal-formated JSON input. Missing end-object-token '}'.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, FormsResource<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the state
        WriteAllProperties(writer, value.State, value.State?.GetType().GetProperties(), options);

        // Write all other properties
        WriteAllProperties(writer, value, value.GetType().GetProperties().Where(p => p.Name != nameof(value.State)), options);

        writer.WriteEndObject();
    }

    private static T CreateState(
        Dictionary<string, object?> stateproperties,
        Dictionary<string, object?> stateConstructorArguments)
    {
        T state = default!;
        var stateType = typeof(T);
        var stateConstructors = FormsResourceJsonConverterCache.GetConstructors(stateType);

        // Create an instance
        if (stateConstructorArguments.Count == 0 && stateConstructors.Count > 0)
        {
            state = Activator.CreateInstance<T>();
        }
        else
        {
            // Try to bind constructors, starting with the constructor with the most parameters
            foreach (var pair in stateConstructors.OrderByDescending(c => c.Value.Count))
            {
                var constructor = pair.Key;
                var parameters = pair.Value;

                var parameterValues = parameters
                    .Select(p => stateConstructorArguments.TryGetValue(p.Key, out var parameterValue) ? parameterValue : null)
                    .ToArray();

                try
                {
                    state = (T)constructor.Invoke(parameterValues);
                    break;
                }
                catch
                {
                    // If we cannot create an instance, we simply try the next constructor
                }
            }
        }

        if (state is null)
        {
            if (!FormsResourceJsonConverterCache.StateCanBeNull<T>())
                throw new JsonException($"Unable to deserialize the state of the forms resource, because the state could not be created and cannot be null. Unable to find a matching constructor for the possible arguments {JsonSerializer.Serialize(stateConstructors)}");
            else
                return state;
        }

        // Set property values
        foreach (var pair in FormsResourceJsonConverterCache.GetProperties(stateType).PropertiesWithCorrectCasing)
        {
            var propertyName = pair.Key;
            var property = pair.Value;
            if (stateproperties.TryGetValue(propertyName, out var propertyValue))
            {
                property.SetValue(state, propertyValue);
                continue;
            }

            var lowerPropertyName = property.Name.ToLowerInvariant();
            if (stateConstructorArguments.TryGetValue(property.Name, out var parameterValue))
            {
                property.SetValue(state, parameterValue);
            }
        }

        return state;
    }

    private static string GetPropertyName(PropertyInfo property, JsonNamingPolicy? propertyNamingPolicy)
    {
        var attribute = property.GetCustomAttribute<JsonPropertyNameAttribute>(true);
        if (attribute is not null)
            return attribute.Name;

        if (propertyNamingPolicy is not null)
            return propertyNamingPolicy.ConvertName(property.Name);

        return property.Name;
    }

    private static bool TryGetConstructorParameter(Type stateType, string lowerPropertyName, [NotNullWhen(true)] out ParameterInfo? parameter)
    {
        var stateConstructors = FormsResourceJsonConverterCache.GetConstructors(stateType);

        parameter = stateConstructors
            .Select(c => c.Value.TryGetValue(lowerPropertyName, out var parameter) ? parameter : null)
            .FirstOrDefault(p => p is not null);

        return parameter is not null;
    }

    private static void WriteAllProperties<TState>(Utf8JsonWriter writer, TState state, IEnumerable<PropertyInfo>? properties, JsonSerializerOptions options)
    {
        if (state is null || properties is null)
            return;

        foreach (var property in properties)
        {
            WritePropertyNameAndValue(writer, state, options, property);
        }
    }

    private static void WritePropertyNameAndValue<TState>(Utf8JsonWriter writer, TState? state, JsonSerializerOptions options, PropertyInfo property)
    {
        var name = GetPropertyName(property, options.PropertyNamingPolicy);
        var value = property.GetValue(state);
        var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

        if (FormsResourceJsonConverter.ShouldWriteProperty(property, value, defaultValue, options.DefaultIgnoreCondition))
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }
    }
}