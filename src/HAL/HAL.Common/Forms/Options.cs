using System.Collections.Generic;

namespace HAL.Common.Forms
{
    /// <summary> The options element contains an enumerated list of possible values for a property.
    /// This can be used to provide a UI similar to HTML controls such as:
    /// - SELECT & OPTIONS
    /// - INPUT.type="radio"
    /// - INPUT.type= "checkbox"
    /// - INPUT.type= "search"(w / type - ahead suggestions)
    ///
    /// The options control can also be used in machine-to-machine interactions where it is
    /// important to provide the client information about the possible values for a property.
    ///
    /// The options element contains a set of possible values accessible either by value
    /// (e.g.options.inline) or by reference (e.g.via options.link.href) and can be used to provide
    /// a constrained list of possble values for a property field.If, when first loaded, the
    /// HAL-FORMS template has a pre-set value in the corresponding property.options.selectedValues
    /// array attribute, the UI MAY render the form with selected value(s) already chosen.
    ///
    /// Whatever value is ultimately selected gets placed into the property.options.selectedValues
    /// array attribute.When sending the results of the completed HAL-FORMS to the server, content
    /// property.options.selectedValues is serialized in a manner compliant with the media type
    /// value in the contentType attribute (e.g.appilcation/json, application/x-www-form-urlencded,
    /// etc.). </summary> <typeparam name="T">The type of the value(s) for this element.</typeparam>
    public class Options<T>
    {
        /// <summary>
        /// The inline attribute is a JSON array that contains the list of possible values. The
        /// inline attribute is OPTIONAL. If the inline attribute is missing or unparseable and the
        /// link (see link) attribute is missing or unparseable, then the options element SHOULD be ignored.
        ///
        /// In it’s simplest form, the inline attribute holds a set of anonymous JSON dictionary
        /// objects in the form {'"prompt": "...", "value" : ""} (see A Simple Inline Array of
        /// Values). The inline contents can also be an array of unique name-value pairs (see An
        /// Inline Array of Name/Value Pairs).
        /// </summary>
        public ICollection<OptionsItem<T>> Inline { get; set; }

        /// <summary>
        /// The link attribute is a JSON dictionary object that contains an href which points to an
        /// external HTTP resource which contains the collection of possible values for a property'.
        /// The +link attribute is OPTIONAL. If the link attribute is missing or unparseable and the
        /// inline (see inline) attribute is missing or unparseable, then the options element SHOULD
        /// be ignored.
        ///
        /// The value returned when dereferencing a link element SHOULD be either a simple array
        /// (see An External Array of Values) or a custom collection (see An External Array of
        /// Name/Value Pairs). The exact format of the returned collection will vary based on the
        /// value of the HTTP Accept header sent with the request.
        ///
        /// When responding to an options.link request, the server MAY return additional fields
        /// (e.g. more than prompt and value fields). These additional fields SHOULD be ignored by
        /// the client application.
        /// </summary>
        public OptionsLink Link { get; set; }

        /// <summary>
        /// Indicates the maximum number of items to return in the selectedValues attribute. The
        /// client application MAY use this as a UI hint and/or to perform a client-side validation.
        /// The maxItems attribute is OPTIONAL. When it is missing or unparseable, the application
        /// SHOULD treat the maxItems value as unbounded (e.g. there is no upper limit on the number
        /// of items that can be selected and returned).
        /// </summary>
        public long? MaxItems { get; set; }

        /// <summary>
        /// Indicated the minimum number of items to return in the selectedValues attribute. The
        /// client application MAY use this as a UI hint and/or to perform a client-side validation.
        /// The minItems attribute is OPTIONAL. When it is missing or unparseable, the application
        /// SHOULD treat the minItems value as 0 (e.g. there is no minimum number of items to be
        /// selected and returned).
        /// </summary>
        public long? MinItems { get; set; }

        /// <summary>
        /// This attribute contains the name of the JSON dictionary element in the array returned
        /// via the inline or link elements to use as the prompt when rendering the options UI. This
        /// is an OPTIONAL attribute. If this attribute is missing or unparseable the application
        /// SHOULD assume the promptField value is set to "prompt".
        ///
        /// See Reference Fields for an example.
        /// </summary>
        public string PromptField { get; set; }

        /// <summary>
        /// This is a JSON array that holds the set of values selected from the list of possible
        /// values supplied by the inline and link attributes. This is an OPTIONAL element. If it is
        /// missing or unparseable, the application SHOULD assume it is an empty JSON array.
        ///
        /// This attribute MAY be populated when the HAL-FORMS is first requested.In that case, the
        /// application can use the value of the selectedValues array to pre-populate the user interface.
        /// </summary>
        public ICollection<T> SelectedValues { get; set; }

        /// <summary>
        /// This attribute contains the name of the JSON dictionary element in the array returned
        /// via the inline or link elements to use as the value when rendering the options UI and
        /// filling in the selectedValues attribute. This is an OPTIONAL attribute. If this
        /// attribute is missing or unparseable the application SHOULD assume the valueField value
        /// is set to "value".
        /// </summary>
        public string ValueField { get; set; }
    }
}