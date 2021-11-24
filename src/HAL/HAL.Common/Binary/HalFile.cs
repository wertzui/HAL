using HAL.Common.Converters;
using System;
using System.Text.Json.Serialization;

namespace HAL.Common.Binary
{
    /// <summary>
    /// Represents a file that is either referenced by a "normal" URI, or a "data" Uri.
    /// </summary>
    [JsonConverter(typeof(HalFileJsonConverter))]
    public class HalFile
    {
        private const string _base64Splitter = ";base64,";
        private byte[] _content;
        private bool _hasUri;
        private string _mimeType;
        private int _splitStart;
        private Uri _uri;

        /// <summary>
        /// Creates a new instance of the <see cref="HalFile"/> class.
        /// </summary>
        /// <param name="uri">
        /// The URI that points to the file. May either be a "normal" or a "data" Uri.
        /// </param>
        public HalFile(string uri)
            : this(new Uri(uri))
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException($"'{nameof(uri)}' cannot be null or whitespace.", nameof(uri));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="HalFile"/> class.
        /// </summary>
        /// <param name="uri">
        /// The URI that points to the file. May either be a "normal" or a "data" URI.
        /// </param>
        public HalFile(Uri uri)
        {
            _hasUri = true;
            _uri = uri ?? throw new ArgumentNullException(nameof(uri));
            _splitStart = uri.PathAndQuery.IndexOf(_base64Splitter);
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
        public byte[] Content => _hasUri ? (_hasDataUri ? Convert.FromBase64String(_uri.PathAndQuery[_splitEnd..]) : null) : _content;

        /// <summary>
        /// Gets the mime type of the file if this instance has been constructed from a byte[] or a
        /// "data" Uri. Null otherwise.
        /// </summary>
        public string MimeType => _hasUri ? (_hasDataUri ? _uri.PathAndQuery[.._splitStart] : null) : _mimeType;

        /// <summary>
        /// Gets the URI of the file. May either be a "normal" or a "data" URI.
        /// </summary>
        public Uri Uri => _hasUri ? _uri : new Uri($"data:{_mimeType}{_base64Splitter}{Convert.ToBase64String(_content)}");

        private bool _hasDataUri => _hasUri && _uri.Scheme == "data";
        private int _splitEnd => _splitStart + _base64Splitter.Length;
    }
}