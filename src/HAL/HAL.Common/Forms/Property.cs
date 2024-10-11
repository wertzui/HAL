using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAL.Common.Forms;

/// <summary>
/// A JSON object that describes the details of the state transition element (name, required,
/// readOnly, etc.). It appears as an anonymous array of properties as a child of the _templates
/// element (See The _templates Element). There is a set of property attributes that are
/// considered core attributes.There are is also group of property attributes that are
/// considered additional attributes. Any library supporting the HAL-FORMS specification SHOULD
/// support all of the core attributes and MAY support some or all of the additional attributes.
/// </summary>
/// <typeparam name="T">The type of the value of this property.</typeparam>
public record Property<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Property{T}" /> class.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public Property(string name)
    {
        Name = name;
    }

    /// <summary>
    /// The cols attribute specifies the expected maximum number of characters per line to
    /// display when rendering the input box. This attribute applies to the associated property
    /// element when that property element’s type attribute is set to textarea. The cols
    /// attribute is a non-negative integer greater than zero. If the cols attribute is missing
    /// or contains an invalid value, it can be assumed to be set to the value of 40. If the
    /// type attribute of the associated property element is not set to textarea, this attribute
    /// SHOULD be ignored. This is an OPTIONAL attribute and it MAY be ignored. The cols
    /// attribute SHOULD appear along with the rows attribute.
    /// </summary>
    public int? Cols { get; set; }

    /// <summary>
    /// The max attribute specifies the maximum numeric value for the value setting of a
    /// property element. This attribute MAY appear along with the min attribute. This is an
    /// OPTIONAL property and it MAY be ignored.
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    /// The maxLength attribute specifies the maximum number of characters allowed in the value
    /// property. This attribute MAY appear along with the minLength attribute. This is an
    /// OPTIONAL property and it MAY be ignored.
    /// </summary>
    public long? MaxLength { get; set; }

    /// <summary>
    /// The min attribute specifies the minimum numeric value for an value setting of a property
    /// element. This attribute MAY appear along with the max attribute. This is an OPTIONAL
    /// property and it MAY be ignored.
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    /// The minlength attribute specifies the minimum number of characters required in a value
    /// property. this attribute MAY appear along with the maxLength attribute. This is an
    /// OPTIONAL property and it MAY be ignored.
    /// </summary>
    public long? MinLength { get; set; }

    /// <summary>
    /// The property name. This is a valid JSON string. This is a REQUIRED element. If this
    /// attribute is missing or set to empty, the client SHOULD ignore this property object completely.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The options element contains a set of possible values accessible either byValue (e.g.
    /// inline) or byReference (e.g. via link.href) and can be used to provide a constrained
    /// list of possble values for a property field. See The options Element for details.
    /// Support for the options object of a property element is OPTIONAL. If the client does not
    /// understand or cannot parse the options element, the options element SHOULD be ignored
    /// and the corresponding property SHOULD be treated as a simple text input element.
    /// </summary>
    public Options<T>? Options { get; set; }

    /// <summary>
    /// The placeholder attribute specifies a short hint that describes the expected value of an
    /// input field (e.g. a sample value or a short description of the expected format). This is
    /// an OPTIONAL field and MAY be ignored.
    /// </summary>
    public T? Placeholder { get; set; }

    /// <summary>
    /// The human-readable prompt for the parameter. This is a valid JSON string. This is an
    /// OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
    /// set to the value in the name attribute.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// The display behavior of the prompt. This is an OPTIONAL element.
    /// If this element is missing, clients SHOULD act as if the prompt display value is
    /// set to <see cref="PropertyPromptDisplayType.Visible"/>.
    /// </summary>
    public PropertyPromptDisplayType? PromptDisplay { get; set; }

    /// <summary>
    /// Indicates whether the parameter is read-only. This is a valid JSON boolean. This is an
    /// OPTIONAL element. If this element is missing, empty, or set to an unrecognized value, it
    /// SHOULD be treated as if the value of readOnly is set to ‘false’.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// A regular expression string to be applied to the value of the parameter. Rules for valid values are the same as the HTML5 pattern attribute [HTML5PAT].
    /// This is an OPTIONAL element. If this attribute missing, is set to empty, or is unparseable , it SHOULD be ignored.
    /// </summary>
    public string? Regex { get; set; }

    /// <summary>
    /// Indicates whether the parameter is required. This is a valid JSON boolean. This is an
    /// OPTIONAL element. If this attribute is missing, set to blank or contains an unrecognized
    /// value, it SHOULD be treated as if the value of required is set to ‘false’.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// The rows attribute specifies the expected maximum number of lines to display when
    /// rendering the input box. This attribute applies to the associated property element when
    /// that property element’s type attribute is set to "textarea". The cols attribute is a
    /// non-negative integer greater than zero. If the cols attribute is missing or contains an
    /// invalid value, it can be assumed to be set to the value of 5. If the type attribute of
    /// the associated property element is not set to "textarea", this attribute SHOULD be
    /// ignored. This is an OPTIONAL attribute and it MAY be ignored. The rows attribute SHOULD
    /// appear along with the cols attribute.
    /// </summary>
    public long Rows { get; set; }

    /// <summary>
    /// The step attribute specifies the interval between legal numbers in a value property. For
    /// example, if step="3", legal numbers could be -3, 0, 3, 6, etc.
    /// </summary>
    public double? Step { get; set; }

    /// <summary>
    /// Indicates whether the value element contains a URI Template [RFC6570] string for the
    /// client to resolve. This is a valid JSON boolean. This is an OPTIONAL element. If this
    /// element is missing, set to empty, or contains unrecognized content, it SHOULD be treated
    /// as if the value of templated is set to ‘false’.
    /// </summary>
    public bool Templated { get; set; }

    /// <summary>
    /// The _templates which are used for collection and object types. For an object there MUST
    /// be only one template with the name "default". For a collection, there MUST be one
    /// template with the name "default" which is used to create new list elements. For a
    /// collection there CAN be templates which have an index as key. These reassemble the
    /// current elements of the collection.
    /// </summary>
    [JsonPropertyName(Constants.FormTemplatesPropertyName)]
    public IDictionary<string, FormTemplate>? Templates { get; set; }

    /// <summary>
    /// The type attribute controls the data type of the property value. It is an enumerated
    /// attribute. The type can also used to determine the interface control to display for user
    /// input. This is an OPTIONAL element. If the type value is not supported by the document
    /// consumer, contains a value not understood by the consumer, and/or is missing, the the
    /// document consumer SHOULD assume the type attribute is set to the default value: "text"
    /// and render the display input as a simple text box. Possible settings for the type value
    /// and the expected contents to be returned in it are: hidden, text, textarea, search, tel,
    /// url, email, password, date, month, week, time, datetime-local, number, range, color. For
    /// hints on how to render and process various type values as well as for guidance on how
    /// each type value affects to the contents of the associated value property, see [HTML5TYPE].
    /// </summary>
    public PropertyType? Type { get; set; }

    /// <summary>
    /// The property value. This is a valid JSON string. This string MAY contain a URI Template
    /// (see templated for details). This is an OPTIONAL element. If it does not exist, clients
    /// SHOULD act as if the value property is set to an empty string.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Additional properties which may be used by clients or not.
    /// This implementation is using it to add additional information for images.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// A Property whose state is of type object?.
/// </summary>
public record Property : Property<object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> class.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public Property(string name) : base(name)
    {
    }
}