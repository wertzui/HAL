namespace HAL.Common.Forms
{
    /// <summary>
    /// In it’s simplest form, the inline attribute holds a set of anonymous JSON dictionary objects
    /// in the form {'"prompt": "...", "value" : ""}
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class OptionsItem<T>
    {
        /// <summary>
        /// The human-readable prompt for the parameter. This is a valid JSON string. This is an
        /// OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
        /// set to the value in the value attribute.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// The value of the element.
        /// </summary>
        public T Value { get; set; }
    }
}