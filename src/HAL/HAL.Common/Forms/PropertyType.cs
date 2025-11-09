using System.ComponentModel;
using System.Runtime.Serialization;

namespace HAL.Common.Forms;

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
[Description(
    """
    The type attribute controls the data type of the property value. It is an enumerated
    attribute. The type can also used to determine the interface control to display for user
    input. This is an OPTIONAL element. If the type value is not supported by the document
    consumer, contains a value not understood by the consumer, and/or is missing, the the
    document consumer SHOULD assume the type attribute is set to the default value: "text"
    and render the display input as a simple text box. Possible settings for the type value
    and the expected contents to be returned in it are: hidden, text, textarea, search, tel,
    url, email, password, date, month, week, time, datetime-local, number, range, color. For
    hints on how to render and process various type values as well as for guidance on how
    each type value affects to the contents of the associated value property, see [HTML5TYPE].
    """
)]
public enum PropertyType
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    // The following are according to the HAL-Forms spec
    [Description("Hidden input field")]
    Hidden,
    [Description("Text input field")]
    Text,
    [Description("Textarea input field")]
    Textarea,
    [Description("Search input field")]
    Search,
    [Description("Telephone input field")]
    Tel,
    [Description("URL input field")]
    Url,
    [Description("Email input field")]
    Email,
    [Description("Password input field")]
    Password,
    [Description("Date input field")]
    Date,
    [Description("Month input field")]
    Month,
    [Description("Week input field")]
    Week,
    [Description("Time input field")]
    Time,
    [EnumMember(Value = "datetime-local")]
    [Description("Datetime-local input field")]
    DatetimeLocal,
    [Description("Number input field")]
    Number,
    [Description("Range input field")]
    Range,
    [Description("Color input field")]
    Color,
    // From here on, these are not in the HAL-Forms spec, but are custom additions
    [Description("Boolean input field")]
    Bool,
    [EnumMember(Value = "datetime-offset")]
    [Description("Datetime-offset input field")]
    DatetimeOffset,
    [Description("Duration input field")]
    Duration,
    [Description("Image input field")]
    Image,
    [Description("File input field")]
    File,
    [Description("Collection input field")]
    Collection,
    [Description("Object input field")]
    Object,
    [Description("Percent input field")]
    Percent,
    [Description("Currency input field")]
    Currency
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}