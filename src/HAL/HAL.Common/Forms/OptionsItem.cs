using System.ComponentModel;

namespace HAL.Common.Forms;

/// <summary>
/// In it's simplest form, the inline attribute holds a set of anonymous JSON dictionary objects
/// in the form {'"prompt": "...", "value" : ""}
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[Description(
    """
    In it's simplest form, the inline attribute holds a set of anonymous JSON dictionary objects
    in the form {'"prompt": "...", "value" : ""}
    """
)]
public class OptionsItem<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsItem{T}"/> class.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user (probably in a drop-down).</param>
    /// <param name="value">The value behind that prompt.</param>
    public OptionsItem(string prompt, T value)
    {
        Value = value;
        Prompt = prompt;
    }

    /// <summary>
    /// The human-readable prompt for the parameter. This is a valid JSON string. This is an
    /// OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
    /// set to the value in the value attribute.
    /// </summary>
    [Description(
        """
        The human-readable prompt for the parameter. This is a valid JSON string. This is an
        OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
        set to the value in the value attribute.
        """
    )]
    public string Prompt { get; }

    /// <summary>
    /// The value of the element.
    /// </summary>
    [Description("The value of the element.")]
    public T Value { get; }
}