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
public enum PropertyType
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    // The following are according to the HAL-Forms spec
    Hidden,
    Text,
    Textarea,
    Search,
    Tel,
    Url,
    Email,
    Password,
    Date,
    Month,
    Week,
    Time,
    [EnumMember(Value = "datetime-local")]
    DatetimeLocal,
    Number,
    Range,
    Color,
    // From here on, these are not in the HAL-Forms spec, but are custom additions
    Bool,
    [EnumMember(Value = "datetime-offset")]
    DatetimeOffset,
    Duration,
    Image,
    File,
    Collection,
    Object,
    Percent,
    Currency
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}