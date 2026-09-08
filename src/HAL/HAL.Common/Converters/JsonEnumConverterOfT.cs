using System.Buffers.Text;
using System.Collections.Generic;
#if !NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
#endif
using System.Reflection;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics;
using HAL.Common;

namespace System.Text.Json.Serialization;

/// <summary>
/// <see cref="JsonConverter{TEnum}"/> to convert enums to and from strings, respecting <see cref="EnumMemberAttribute"/> decorations. Supports nullable enums.
/// </summary>
public class JsonEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private class EnumInfo
    {
#pragma warning disable SA1401 // Fields should be private
        public string Name;
        public TEnum EnumValue;
        public ulong RawValue;
#pragma warning restore SA1401 // Fields should be private

        public EnumInfo(string name, TEnum enumValue, ulong rawValue)
        {
            Name = name;
            EnumValue = enumValue;
            RawValue = rawValue;
        }
    }
    private class FlagsEnumInfo
    {
#pragma warning disable SA1401 // Fields should be private
        public string[] Name;
        public TEnum EnumValue;
        public ulong RawValue;
#pragma warning restore SA1401 // Fields should be private

        public FlagsEnumInfo(string[] name, TEnum enumValue, ulong rawValue)
        {
            Name = name;
            EnumValue = enumValue;
            RawValue = rawValue;
        }
    }

    private const BindingFlags _enumBindings = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
    private const int _maximumAutoGrowthCacheSize = 64;

#if NETSTANDARD2_0
		private static readonly string[] s_Split = new string[] { ", " };
#endif

    private readonly bool _allowIntegerValues;
    private readonly ulong? _deserializationFailureFallbackValueRaw;
    private readonly TEnum? _deserializationFailureFallbackValue;
    private readonly Type _enumType;
    private readonly TypeCode _enumTypeCode;
    private readonly bool _isFlags;
    private readonly JsonFlagsEnumSerializationHandling _jsonFlagsEnumSerializationHandling;
    private readonly object _transformedToRawCopyLockObject = new();
    private readonly object _rawToTransformedCopyLockObject = new();
    private Dictionary<TEnum, EnumInfo> _rawToTransformed;
    private Dictionary<TEnum, FlagsEnumInfo> _rawFlagsToTransformed;
    private Dictionary<string, EnumInfo> _transformedToRaw;
    private static readonly PropertyInfo? _sJsonExceptionAppendPathInformation
        = typeof(JsonException).GetProperty("AppendPathInformation", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Creates a new <see cref="JsonEnumConverter{TEnum}"/> instance.
    /// </summary>
    /// <param name="options">The options to configure the converter.</param>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="JsonException"></exception>
    public JsonEnumConverter(JsonEnumConverterOptions? options)
    {
        _enumType = typeof(TEnum);

        var computedOptions
            = _enumType.GetCustomAttribute<JsonEnumConverterOptionsAttribute>(false)?.Options
            ?? options;

        _allowIntegerValues = computedOptions?.AllowIntegerValues ?? true;
        _enumTypeCode = Type.GetTypeCode(_enumType);
        _isFlags = _enumType.IsDefined(typeof(FlagsAttribute), true);
        _jsonFlagsEnumSerializationHandling = computedOptions?.JsonFlagsEnumSerializationHandling ?? JsonFlagsEnumSerializationHandling.Array;

        var deserializationFailureFallbackValue = computedOptions?.ConvertedDeserializationFailureFallbackValue;

        var builtInNames = _enumType.GetEnumNames();
        var builtInValues = _enumType.GetEnumValues();

        var numberOfBuiltInNames = builtInNames.Length;

        _rawToTransformed = new Dictionary<TEnum, EnumInfo>(numberOfBuiltInNames);
        _rawFlagsToTransformed = new Dictionary<TEnum, FlagsEnumInfo>(numberOfBuiltInNames);
        _transformedToRaw = new Dictionary<string, EnumInfo>(numberOfBuiltInNames);

        for (var i = 0; i < numberOfBuiltInNames; i++)
        {
            var enumValue = (Enum?)builtInValues.GetValue(i);
            if (enumValue == null)
                continue;
            var rawValue = JsonEnumConverter.GetEnumValue(_enumTypeCode, enumValue);

            var name = builtInNames[i];
            var field = _enumType.GetField(name, _enumBindings)!;

            var transformedName = field.GetCustomAttribute<EnumMemberAttribute>(true)?.Value ??
                                     field.GetCustomAttribute<JsonPropertyNameAttribute>(true)?.Name ??
                                     computedOptions?.NamingPolicy?.ConvertName(name) ??
                                     name;

            if (enumValue is not TEnum typedValue)
                throw new NotSupportedException($"Enum type mismatch: {enumValue.GetType()} cannot be converted to {_enumType}.");

            if (deserializationFailureFallbackValue.HasValue && rawValue == deserializationFailureFallbackValue)
            {
                _deserializationFailureFallbackValueRaw = deserializationFailureFallbackValue;
                _deserializationFailureFallbackValue = typedValue;
            }

            _rawToTransformed[typedValue] = new EnumInfo(transformedName, typedValue, rawValue);
            _transformedToRaw[transformedName] = new EnumInfo(name, typedValue, rawValue);
        }

        if (deserializationFailureFallbackValue.HasValue && !_deserializationFailureFallbackValue.HasValue)
            throw new JsonException($"JsonStringEnumMemberConverter could not find a definition on Enum type {_enumType} matching deserializationFailureFallbackValue '{deserializationFailureFallbackValue}'.");
    }

    /// <inheritdoc/>
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.TokenType;

        if (token == JsonTokenType.String)
        {
            var enumString = reader.GetString()!;

            var transformedToRaw = _transformedToRaw;

            // Case sensitive search attempted first.
            if (transformedToRaw.TryGetValue(enumString, out var enumInfo))
                return enumInfo.EnumValue;

            if (_isFlags)
            {
                return ConvertFlagsStringValueToEnumValue(enumString, transformedToRaw);
            }

            // Case insensitive search attempted second.
            foreach (var enumItem in transformedToRaw)
            {
                if (string.Equals(enumItem.Key, enumString, StringComparison.OrdinalIgnoreCase))
                {
                    return enumItem.Value.EnumValue;
                }
            }

            return _deserializationFailureFallbackValue ?? throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType, enumString);
        }
        else if (token == JsonTokenType.StartArray && _isFlags)
        {
            return ReadFlagsEnumArray(ref reader, _transformedToRaw);
        }

        return ReadNumericEnumValue(ref reader, token);
    }

    /// <inheritdoc/>
    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.TokenType;

        if (token != JsonTokenType.PropertyName)
            throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType);

        var enumString = reader.GetString()!;

        var transformedToRaw = _transformedToRaw;

        // Case sensitive search attempted first.
        if (transformedToRaw.TryGetValue(enumString, out var enumInfo))
            return enumInfo.EnumValue;

        // For Enums used a dictionary keys, numeric form is still a string eg { "0": "value" }.
        if (ulong.TryParse(enumString, out var numericValue))
        {
            return !_allowIntegerValues
                ? _deserializationFailureFallbackValue ?? throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType)
                : (TEnum)Enum.ToObject(_enumType, numericValue);
        }

        if (_isFlags)
        {
            return ConvertFlagsStringValueToEnumValue(enumString, transformedToRaw);
        }

        // Case insensitive search attempted second.
        foreach (var enumItem in transformedToRaw)
        {
            if (string.Equals(enumItem.Key, enumString, StringComparison.OrdinalIgnoreCase))
            {
                return enumItem.Value.EnumValue;
            }
        }

        return _deserializationFailureFallbackValue ?? throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType, enumString);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var rawToTransformed = _rawToTransformed;
        if (rawToTransformed.TryGetValue(value, out var enumInfo))
        {
            writer.WriteStringValue(enumInfo.Name);
            return;
        }

        var rawValue = JsonEnumConverter.GetEnumValue(_enumTypeCode, value);

        if (_isFlags)
        {
            if (_jsonFlagsEnumSerializationHandling == JsonFlagsEnumSerializationHandling.Array
                && TryGetArrayForFlagsEnumValue(value, rawValue, rawToTransformed, out var flagsValueArray))
            {
                writer.WriteStartArray();
                foreach (var flagValue in flagsValueArray)
                    writer.WriteStringValue(flagValue);
                writer.WriteEndArray();
                return;
            }
            else if (_jsonFlagsEnumSerializationHandling == JsonFlagsEnumSerializationHandling.String
                && TryGetStringForFlagsEnumValue(value, rawValue, rawToTransformed, out var flagsValueString))
            {
                writer.WriteStringValue(flagsValueString);
                return;
            }
        }

        if (!_allowIntegerValues)
            throw new JsonException($"Enum type {_enumType} does not have a mapping for integer value '{rawValue.ToString(CultureInfo.CurrentCulture)}'.");

        Span<byte> data = stackalloc byte[20];
        WriteNumericValueToSpan(rawValue, ref data);
        writer.WriteRawValue(data, skipInputValidation: true);
    }

    /// <inheritdoc/>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var rawToTransformed = _rawToTransformed;
        if (rawToTransformed.TryGetValue(value, out var enumInfo))
        {
            writer.WritePropertyName(enumInfo.Name);
            return;
        }

        var rawValue = JsonEnumConverter.GetEnumValue(_enumTypeCode, value);

        if (_isFlags
            && TryGetStringForFlagsEnumValue(value, rawValue, rawToTransformed, out var flagsValueString))
        {
            writer.WritePropertyName(flagsValueString);
            return;
        }

        if (!_allowIntegerValues)
            throw new JsonException($"Enum type {_enumType} does not have a mapping for integer value '{rawValue.ToString(CultureInfo.CurrentCulture)}'.");

        Span<byte> data = stackalloc byte[20];
        WriteNumericValueToSpan(rawValue, ref data);
        writer.WritePropertyName(data);
    }

    private TEnum ConvertFlagsStringValueToEnumValue(string value, Dictionary<string, EnumInfo> transformedToRaw)
    {
        ulong calculatedValue = 0;

#if NETSTANDARD2_0
			string[] flagValues = value.Split(s_Split, StringSplitOptions.None);
#else
        var flagValues = value.Split(", ");
#endif
        foreach (var flagValue in flagValues)
        {
            // Case sensitive search attempted first.
            if (transformedToRaw.TryGetValue(flagValue, out var enumInfo))
            {
                calculatedValue |= enumInfo.RawValue;
            }
            else
            {
                // Case insensitive search attempted second.
                var matched = false;
                foreach (var enumItem in transformedToRaw)
                {
                    if (string.Equals(enumItem.Key, flagValue, StringComparison.OrdinalIgnoreCase))
                    {
                        calculatedValue |= enumItem.Value.RawValue;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    if (_deserializationFailureFallbackValueRaw.HasValue)
                        calculatedValue |= _deserializationFailureFallbackValueRaw.Value;
                    else
                        throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType, flagValue);
                }
            }
        }

        var enumValue = (TEnum)Enum.ToObject(_enumType, calculatedValue);
        if (transformedToRaw.Count < _maximumAutoGrowthCacheSize)
        {
            lock (_transformedToRawCopyLockObject)
            {
                if (!_transformedToRaw.ContainsKey(value) && _transformedToRaw.Count < _maximumAutoGrowthCacheSize)
                {
                    Dictionary<string, EnumInfo> transformedToRawCopy = new(_transformedToRaw)
                    {
                        [value] = new EnumInfo(value, enumValue, calculatedValue)
                    };
                    _transformedToRaw = transformedToRawCopy;
                }
            }
        }

        return enumValue;
    }

    private TEnum ReadFlagsEnumArray(ref Utf8JsonReader reader, Dictionary<string, EnumInfo> transformedToRaw)
    {
        ulong calculatedValue = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (!_allowIntegerValues)
                {
                    reader.Skip();
                    continue;
                }

                var rawValue = reader.GetUInt64();
                calculatedValue |= rawValue;
                continue;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                if (_deserializationFailureFallbackValueRaw.HasValue)
                {
                    reader.Skip();
                    calculatedValue |= _deserializationFailureFallbackValueRaw.Value;
                    continue;
                }
                throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType);
            }

            var flagValue = reader.GetString();

            if (flagValue is null)
            {
                if (_deserializationFailureFallbackValueRaw.HasValue)
                {
                    calculatedValue |= _deserializationFailureFallbackValueRaw.Value;
                    continue;
                }
                throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType);
            }

            // Case sensitive search attempted first.
            if (transformedToRaw.TryGetValue(flagValue, out var enumInfo))
            {
                calculatedValue |= enumInfo.RawValue;
            }
            else
            {
                // Case insensitive search attempted second.
                var matched = false;
                foreach (var enumItem in transformedToRaw)
                {
                    if (string.Equals(enumItem.Key, flagValue, StringComparison.OrdinalIgnoreCase))
                    {
                        calculatedValue |= enumItem.Value.RawValue;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    if (_deserializationFailureFallbackValueRaw.HasValue)
                        calculatedValue |= _deserializationFailureFallbackValueRaw.Value;
                    else
                        throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType, flagValue);
                }
            }
        }

        var enumValue = (TEnum)Enum.ToObject(_enumType, calculatedValue);

        return enumValue;
    }

    private TEnum ReadNumericEnumValue(ref Utf8JsonReader reader, JsonTokenType tokenType)
    {
        if (tokenType != JsonTokenType.Number || !_allowIntegerValues)
        {
            if (_deserializationFailureFallbackValue.HasValue)
            {
                reader.Skip();
                return _deserializationFailureFallbackValue.Value;
            }
            throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType);
        }

        switch (_enumTypeCode)
        {
            case TypeCode.Int32:
                if (reader.TryGetInt32(out var int32))
                {
                    return (TEnum)Enum.ToObject(_enumType, int32);
                }
                break;
            case TypeCode.Int64:
                if (reader.TryGetInt64(out var int64))
                {
                    return (TEnum)Enum.ToObject(_enumType, int64);
                }
                break;
            case TypeCode.Int16:
                if (reader.TryGetInt16(out var int16))
                {
                    return (TEnum)Enum.ToObject(_enumType, int16);
                }
                break;
            case TypeCode.Byte:
                if (reader.TryGetByte(out var ubyte8))
                {
                    return (TEnum)Enum.ToObject(_enumType, ubyte8);
                }
                break;
            case TypeCode.UInt32:
                if (reader.TryGetUInt32(out var uint32))
                {
                    return (TEnum)Enum.ToObject(_enumType, uint32);
                }
                break;
            case TypeCode.UInt64:
                if (reader.TryGetUInt64(out var uint64))
                {
                    return (TEnum)Enum.ToObject(_enumType, uint64);
                }
                break;
            case TypeCode.UInt16:
                if (reader.TryGetUInt16(out var uint16))
                {
                    return (TEnum)Enum.ToObject(_enumType, uint16);
                }
                break;
            case TypeCode.SByte:
                if (reader.TryGetSByte(out var byte8))
                {
                    return (TEnum)Enum.ToObject(_enumType, byte8);
                }
                break;
        }

        throw GenerateJsonException_DeserializeUnableToConvertValue(_enumType);
    }

    private bool TryGetStringForFlagsEnumValue(
        TEnum value,
        ulong rawValue,
        Dictionary<TEnum, EnumInfo> rawToTransformed,
#if !NETSTANDARD2_0
        [NotNullWhen(true)]
#endif
			out string? flagsValueString)
    {
        ulong calculatedValue = 0;

        StringBuilder Builder = new();
        foreach (var enumItem in rawToTransformed)
        {
            var enumInfo = enumItem.Value;
            if (!value.HasFlag(enumInfo.EnumValue)
                || enumInfo.RawValue == 0) // Definitions with 'None' should hit the cache case.
            {
                continue;
            }

            // Track the value to make sure all bits are represented.
            calculatedValue |= enumInfo.RawValue;

            if (Builder.Length > 0)
                Builder.Append(", ");
            Builder.Append(enumInfo.Name);
        }

        if (calculatedValue == rawValue)
        {
            flagsValueString = Builder.ToString();
            if (rawToTransformed.Count < _maximumAutoGrowthCacheSize)
            {
                lock (_rawToTransformedCopyLockObject)
                {
                    if (!_rawToTransformed.ContainsKey(value) && _rawToTransformed.Count < _maximumAutoGrowthCacheSize)
                    {
                        Dictionary<TEnum, EnumInfo> rawToTransformedCopy = new(_rawToTransformed)
                        {
                            [value] = new EnumInfo(flagsValueString, value, rawValue)
                        };
                        _rawToTransformed = rawToTransformedCopy;
                    }
                }
            }
            return true;
        }

        flagsValueString = null;
        return false;
    }

    private bool TryGetArrayForFlagsEnumValue(
        TEnum value,
        ulong rawValue,
        Dictionary<TEnum, EnumInfo> rawToTransformed,
        out string[] flagsValueArray)
    {
        if (_rawFlagsToTransformed.TryGetValue(value, out var flagsEnumInfo))
        {
            flagsValueArray = flagsEnumInfo.Name;
            return true;
        }

        ulong calculatedValue = 0;

        var list = new List<string>();
        foreach (var enumItem in rawToTransformed)
        {
            var enumInfo = enumItem.Value;
            if (!value.HasFlag(enumInfo.EnumValue)
                || enumInfo.RawValue == 0) // Definitions with 'None' should hit the cache case.
            {
                continue;
            }

            // Track the value to make sure all bits are represented.
            calculatedValue |= enumInfo.RawValue;

            list.Add(enumInfo.Name);
        }

        if (calculatedValue == rawValue)
        {
            flagsValueArray = list.ToArray();
            if (rawToTransformed.Count < _maximumAutoGrowthCacheSize)
            {
                lock (_rawToTransformedCopyLockObject)
                {
                    if (!_rawFlagsToTransformed.ContainsKey(value) && _rawFlagsToTransformed.Count < _maximumAutoGrowthCacheSize)
                    {
                        Dictionary<TEnum, FlagsEnumInfo> rawToTransformedCopy = new(_rawFlagsToTransformed)
                        {
                            [value] = new FlagsEnumInfo(flagsValueArray, value, rawValue)
                        };
                        _rawFlagsToTransformed = rawToTransformedCopy;
                    }
                }
            }
            return true;
        }

        flagsValueArray = [];
        return false;
    }

    private void WriteNumericValueToSpan(ulong rawValue, ref Span<byte> data)
    {
        int bytesWritten;

        switch (_enumTypeCode)
        {
            case TypeCode.Int32:
                Utf8Formatter.TryFormat((int)rawValue, data, out bytesWritten);
                break;
            case TypeCode.Int64:
                Utf8Formatter.TryFormat((long)rawValue, data, out bytesWritten);
                break;
            case TypeCode.Int16:
                Utf8Formatter.TryFormat((short)rawValue, data, out bytesWritten);
                break;
            case TypeCode.Byte:
                Utf8Formatter.TryFormat((byte)rawValue, data, out bytesWritten);
                break;
            case TypeCode.UInt32:
                Utf8Formatter.TryFormat((uint)rawValue, data, out bytesWritten);
                break;
            case TypeCode.UInt64:
                Utf8Formatter.TryFormat(rawValue, data, out bytesWritten);
                break;
            case TypeCode.UInt16:
                Utf8Formatter.TryFormat((ushort)rawValue, data, out bytesWritten);
                break;
            case TypeCode.SByte:
                Utf8Formatter.TryFormat((sbyte)rawValue, data, out bytesWritten);
                break;
            default:
                throw new JsonException(); // GetEnumValue should have already thrown.
        }

#if NETSTANDARD2_0
			data = data.Slice(0, bytesWritten);
#else
        data = data[..bytesWritten];
#endif
    }

    /// <summary>
    /// Generate a <see cref="JsonException"/> using the internal
    /// <c>JsonException.AppendPathInformation</c> property that will
    /// eventually include the JSON path, line number, and byte position in
    /// line.
    /// <para>
    /// The final message of the exception looks like: The JSON value could
    /// not be converted to {0}. Path: $.{JSONPath} | LineNumber:
    /// {LineNumber} | BytePositionInLine: {BytePositionInLine}.
    /// </para>
    /// </summary>
    /// <param name="propertyType">Property type.</param>
    /// <returns><see cref="JsonException"/>.</returns>
    private static JsonException GenerateJsonException_DeserializeUnableToConvertValue(Type propertyType)
    {
        Debug.Assert(_sJsonExceptionAppendPathInformation != null);

        JsonException jsonException = new($"The JSON value could not be converted to {propertyType}.");
        _sJsonExceptionAppendPathInformation?.SetValue(jsonException, true);
        return jsonException;
    }

    /// <summary>
    /// Generate a <see cref="JsonException"/> using the internal
    /// <c>JsonException.AppendPathInformation</c> property that will
    /// eventually include the JSON path, line number, and byte position in
    /// line.
    /// <para>
    /// The final message of the exception looks like: The JSON value '{1}'
    /// could not be converted to {0}. Path: $.{JSONPath} | LineNumber:
    /// {LineNumber} | BytePositionInLine: {BytePositionInLine}.
    /// </para>
    /// </summary>
    /// <param name="propertyType">Property type.</param>
    /// <param name="propertyValue">Value that could not be parsed into
    /// property type.</param>
    /// <param name="innerException">Optional inner <see cref="Exception"/>.</param>
    /// <returns><see cref="JsonException"/>.</returns>
    private static JsonException GenerateJsonException_DeserializeUnableToConvertValue(
        Type propertyType,
        string propertyValue,
        Exception? innerException = null)
    {
        Debug.Assert(_sJsonExceptionAppendPathInformation != null);

        JsonException jsonException = new(
            $"The JSON value '{propertyValue}' could not be converted to {propertyType}.",
            innerException);
        _sJsonExceptionAppendPathInformation?.SetValue(jsonException, true);
        return jsonException;
    }
}
