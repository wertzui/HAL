namespace HAL.Common.Forms
{
    /// <summary>
    /// The link attribute is a JSON dictionary object that contains an href which points to an
    /// external HTTP resource which contains the collection of possible values for a property'. The
    /// +link attribute is OPTIONAL. If the link attribute is missing or unparseable and the inline
    /// (see inline) attribute is missing or unparseable, then the options element SHOULD be ignored.
    ///
    /// The value returned when dereferencing a link element SHOULD be either a simple array (see An
    /// External Array of Values) or a custom collection (see An External Array of Name/Value
    /// Pairs). The exact format of the returned collection will vary based on the value of the HTTP
    /// Accept header sent with the request.
    ///
    /// When responding to an options.link request, the server MAY return additional fields (e.g.
    /// more than prompt and value fields). These additional fields SHOULD be ignored by the client application.
    /// </summary>
    public class OptionsLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsLink"/> class.
        /// </summary>
        /// <param name="href">The URL associated with the key.</param>
        public OptionsLink(string href)
        {
            Href = href;
        }

        /// <summary>
        /// The URL associated with the key. This is a REQUIRED property. If this is missing, set to
        /// empty or cannot be parsed, the associated link element SHOULD be ignored.
        /// </summary>
        public string Href { get; }

        /// <summary>
        /// A boolean value that SHOULD be set to true when link.href contains a URI Template
        /// [RFC6570]. This is an OPTIONAL attribute. If it is missing or unparseable, the value of
        /// templated SHOULD be treated as set to false.
        /// </summary>
        public bool Templated { get; set; }

        /// <summary>
        /// A string used as a hint to indicate the media type expected when dereferencing the
        /// target resource. This is an OPTIONAL attribute. The type value SHOULD be set to
        /// application/json or text/csv. It MAY be set to some other value. If the type attribute
        /// is missing or unparseable, it SHOULD be assumed to be set to application/json. Client
        /// applications SHOULD use the value of the type attribute to populate the HTTP Accept header.
        /// </summary>
        public string? Type { get; set; }
    }
}