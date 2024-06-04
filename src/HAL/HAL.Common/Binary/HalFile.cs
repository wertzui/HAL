using HAL.Common.Converters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace HAL.Common.Binary;

/// <summary>
/// Represents a file that is either referenced by a "normal" URI, or a "data" Uri.
/// </summary>
[JsonConverter(typeof(HalFileJsonConverter))]
public class HalFile
{
    private const string _base64Splitter = ";base64,";
    private const string _dataSchema = "data:";
    private const int _dataSchemaEnd = 5; // _dataSchema.Length
    private readonly byte[]? _content;
    private readonly string? _mimeType;
    private readonly int _splitStart;
    private readonly string? _uri;

    /// <summary>
    /// Creates a new instance of the <see cref="HalFile"/> class.
    /// </summary>
    /// <param name="uri">
    /// The URI that points to the file. May either be a "normal" or a "data" Uri.
    /// </param>
    public HalFile(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));

        _uri = uri ?? throw new ArgumentNullException(nameof(uri));
        _splitStart = uri.IndexOf(_base64Splitter);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="HalFile"/> class.
    /// </summary>
    /// <param name="uri">
    /// The URI that points to the file. May either be a "normal" or a "data" URI.
    /// </param>
    public HalFile(Uri uri)
        : this(uri.AbsoluteUri)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="HalFile"/> class.
    /// </summary>
    /// <param name="mimeType">The type of the file.</param>
    /// <param name="content">The content of the file.</param>
    public HalFile(string mimeType, byte[] content)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException($"'{nameof(mimeType)}' cannot be null or whitespace.", nameof(mimeType));

        _mimeType = mimeType;
        _content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Gets the content of the file if this instance has been constructed from a byte[] or a
    /// "data" Uri. Null otherwise.
    /// </summary>
    public byte[]? Content => _content ?? (HasDataUri ? Convert.FromBase64String(_uri![SplitEnd..]) : null);

    /// <summary>
    /// Whether this instance has a URI with a data schema or not.
    /// If it has a data schema, it's Content property will be populated too.
    /// </summary>
    [MemberNotNullWhen(true, [nameof(Content), nameof(MimeType)])]
    public bool HasDataUri => _content is not null || (_uri?.StartsWith(_dataSchema) ?? false);

    /// <summary>
    /// Gets the mime type of the file if this instance has been constructed from a byte[] or a
    /// "data" Uri. Null otherwise.
    /// </summary>
    public string? MimeType => _mimeType ?? (HasDataUri ? _uri?[_dataSchemaEnd.._splitStart] : null);

    /// <summary>
    /// Gets the URI of the file. May either be a "normal" or a "data" URI.
    /// </summary>
    public string Uri => _uri ?? string.Concat(_dataSchema, _mimeType, _base64Splitter, Convert.ToBase64String(_content!));

    private int SplitEnd => _splitStart + _base64Splitter.Length;
}