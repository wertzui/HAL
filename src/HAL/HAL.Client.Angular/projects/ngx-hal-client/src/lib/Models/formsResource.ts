import { Resource, ResourceDto } from './resource';

/**
 * These are the types of values that can be directly used in a property without
 * the requirement of nested templates for objects or collections.
 */
export type SimpleValue = string | number | boolean | Date | null | undefined;

/**
 * THis helper type converts a PropertyDto<X, Y, Z> to a Property<X, Y, Z> and preserfes the generic parameters.
 */
export type ExtractGenericPropertyType<TPropertyDto> = TPropertyDto extends PropertyDto<infer X, infer Y, infer Z> ? Property<X, Y, Z> : never

/**
 * A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.
 */
export interface FormsResourceDto extends ResourceDto {

  /**
   * The _templates collection describes the available state transition details including the
   * HTTP method, message content-type, and arguments for the transition. This is a REQUIRED
   * element. If the HAL-FORMS document does not contain this element or the contents are
   * unrecognized or unparseable, the HAL-FORMS document SHOULD be ignored. The _templates
   * element contains a dictionary collection of template objects.A valid HAL-FORMS document
   * has at least one entry in the _templates dictionary collection. Each template contains
   * the following possible properties:
   */
  _templates?: TemplateDtos;
}

/**
 * A template of a HAL-Form.
 */
export interface TemplateDto<TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>> = ReadonlyArray<PropertyDto<SimpleValue, string, string>>> {
  /**
   * The value of contentType is the media type the client SHOULD use when sending a request
   * body to the server. This is an OPTIONAL element. The value of this property SHOULD be
   * set to "application/json" or "application/x-www-form-urlencoded". It MAY be set to other
   * valid media-type values. If the contentType property is missing, is set to empty, or
   * contains an unrecognized value, the client SHOULD act is if the contentType is set to
   * "application/json". See Encoding Request Bodies for details.
   */
  contentType?: string;
  /**
   * The HTTP method the client SHOULD use when the service request. Any valid HTTP method is
   * allowed. This is a REQUIRED element. If the value is empty or is not understood by the
   * client, the value MUST be treated as an HTTP GET.
   */
  method?: string;
  /**
   * An array of one or more anonymous property elements (see The property Element) that each
   * describe a parameter for the associated state transition. This is an OPTIONAL element.
   * If the array is missing or empty, the properties collection MUST be treated as an empty
   * set of parameters — meaning that the transition is meant to be executed without passing
   * any parameters.
   */
  properties: TProperties;
  /**
   * Contains the identifier of the target URL for the client to use when submitting the
   * completed HAL-FORMS template. For example, if the client should submit the completed
   * template to the following URL: http://api.example.org/jobs/ then the target proprety
   * would be target="http://api.example.org/jobs/". This is an OPTIONAL property. If this
   * property is not understood by the recipient, left blank, or contains an invalid URL
   * string, it SHOULD be ignored. The target property holds the same information as the
   * _htarget query string property. If both the target prorperty and the _htarget query
   * string value appear in the same message, the _htarget query string SHOULD be used and
   * the target property SHOULD be ignored.
   */
  target?: string;
  /**
   * A human-readable string that can be used to identify this template. This is a valid JSON
   * string. This is an OPTIONAL element. If it does not exist or is unparsable, consumers
   * MAY use the key value of the template as the value for title.
   */
  title?: string;
}

/**
 * Contains multiple templates.
 */
export interface TemplateDtos extends Record<string, TemplateDto> {
}

/**
 * A JSON object that describes the details of the state transition element (name, required,
 * readOnly, etc.). It appears as an anonymous array of properties as a child of the _templates
 * element (See The _templates Element). There is a set of property attributes that are
 * considered core attributes.There are is also group of property attributes that are
 * considered additional attributes. Any library supporting the HAL-FORMS specification SHOULD
 * support all of the core attributes and MAY support some or all of the additional attributes.
 * @template TValue The type of the value of the property.
 * @template OptionsPromptField The name of the prompt field of the options if this property has options. This is the same as the @see OptionsDto.promptField.
 * @template OptionsValueField The name of the value field of the options if this property has options. This is the same as the @see OptionsDto.valueField.
 */
export interface PropertyDto<TValue extends SimpleValue, OptionsPromptField extends string = "prompt", OptionsValueField extends string = "value"> {
  /**
   * The cols attribute specifies the expected maximum number of characters per line to
   * display when rendering the input box. This attribute applies to the associated property
   * element when that property element’s type attribute is set to textarea. The cols
   * attribute is a non-negative integer greater than zero. If the cols attribute is missing
   * or contains an invalid value, it can be assumed to be set to the value of 40. If the
   * type attribute of the associated property element is not set to textarea, this attribute
   * SHOULD be ignored. This is an OPTIONAL attribute and it MAY be ignored. The cols
   * attribute SHOULD appear along with the rows attribute.
   */
  cols?: number;
  /**
   * The max attribute specifies the maximum numeric value for the value setting of a
   * property element. This attribute MAY appear along with the min attribute. This is an
   * OPTIONAL property and it MAY be ignored.
   */
  max?: number;
  /**
   * The maxLength attribute specifies the maximum number of characters allowed in the value
   * property. This attribute MAY appear along with the minLength attribute. This is an
   * OPTIONAL property and it MAY be ignored.
   */
  maxLength?: number;
  /**
   * The min attribute specifies the minimum numeric value for an value setting of a property
   * element. This attribute MAY appear along with the max attribute. This is an OPTIONAL
   * property and it MAY be ignored.
   */
  min?: number;
  /**
   * The minlength attribute specifies the minimum number of characters required in a value
   * property. this attribute MAY appear along with the maxLength attribute. This is an
   * OPTIONAL property and it MAY be ignored.
   */
  minLength?: number;
  /**
   * The property name. This is a valid JSON string. This is a REQUIRED element. If this
   * attribute is missing or set to empty, the client SHOULD ignore this property object completely.
   */
  name: string;
  /**
   * The options element contains a set of possible values accessible either byValue (e.g.
   * inline) or byReference (e.g. via link.href) and can be used to provide a constrained
   * list of possble values for a property field. See The options Element for details.
   * Support for the options object of a property element is OPTIONAL. If the client does not
   * understand or cannot parse the options element, the options element SHOULD be ignored
   * and the corresponding property SHOULD be treated as a simple text input element.
   */
  options?: OptionsDto<TValue, OptionsPromptField, OptionsValueField>;
  /**
   * The placeholder attribute specifies a short hint that describes the expected value of an
   * input field (e.g. a sample value or a short description of the expected format). This is
   * an OPTIONAL field and MAY be ignored.
   */
  placeholder?: TValue;
  /**
   * The human-readable prompt for the parameter. This is a valid JSON string. This is an
   * OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
   * set to the value in the name attribute.
   */
  prompt?: string;
  /**
   * The display behavior of the prompt. This is an OPTIONAL element.
   * If this element is missing, clients SHOULD act as if the prompt display value is
   * set to @see PropertyPromptDisplayType.Visible.
   */
  promptDisplay?: PropertyPromptDisplayType;
  /**
   * Indicates whether the parameter is read-only. This is a valid JSON boolean. This is an
   * OPTIONAL element. If this element is missing, empty, or set to an unrecognized value, it
   * SHOULD be treated as if the value of readOnly is set to ‘false’.
   */
  readOnly?: boolean;
  /**
   * A regular expression string to be applied to the value of the parameter. Rules for valid values are the same as the HTML5 pattern attribute [HTML5PAT].
   * This is an OPTIONAL element. If this attribute missing, is set to empty, or is unparseable , it SHOULD be ignored.
   */
  regex?: string;
  /**
   * Indicates whether the parameter is required. This is a valid JSON boolean. This is an
   * OPTIONAL element. If this attribute is missing, set to blank or contains an unrecognized
   * value, it SHOULD be treated as if the value of required is set to ‘false’.
   */
  required?: boolean;
  /**
   * The rows attribute specifies the expected maximum number of lines to display when
   * rendering the input box. This attribute applies to the associated property element when
   * that property element’s type attribute is set to "textarea". The cols attribute is a
   * non-negative integer greater than zero. If the cols attribute is missing or contains an
   * invalid value, it can be assumed to be set to the value of 5. If the type attribute of
   * the associated property element is not set to "textarea", this attribute SHOULD be
   * ignored. This is an OPTIONAL attribute and it MAY be ignored. The rows attribute SHOULD
   * appear along with the cols attribute.
   */
  rows?: number;
  /**
   * The step attribute specifies the interval between legal numbers in a value property. For
   * example, if step="3", legal numbers could be -3, 0, 3, 6, etc.
   */
  step?: number;
  /**
   * Indicates whether the value element contains a URI Template [RFC6570] string for the
   * client to resolve. This is a valid JSON boolean. This is an OPTIONAL element. If this
   * element is missing, set to empty, or contains unrecognized content, it SHOULD be treated
   * as if the value of templated is set to ‘false’.
   */
  templated?: boolean;
  /**
   * The _templates which are used for collection and object types. For an object there MUST
   * be only one template with the name "default". For a collection, there MUST be one
   * template with the name "default" which is used to create new list elements. For a
   * collection there CAN be templates which have an index as key. These reassemble the
   * current elements of the collection.
   */
  _templates?: TemplateDtos;
  /**
   * The type attribute controls the data type of the property value. It is an enumerated
   * attribute. The type can also used to determine the interface control to display for user
   * input. This is an OPTIONAL element. If the type value is not supported by the document
   * consumer, contains a value not understood by the consumer, and/or is missing, the the
   * document consumer SHOULD assume the type attribute is set to the default value: "text"
   * and render the display input as a simple text box. Possible settings for the type value
   * and the expected contents to be returned in it are: hidden, text, textarea, search, tel,
   * url, email, password, date, month, week, time, datetime-local, number, range, color. For
   * hints on how to render and process various type values as well as for guidance on how
   * each type value affects to the contents of the associated value property, see [HTML5TYPE].
   */
  type?: PropertyType;
  /**
   * The property value. This is a valid JSON string. This string MAY contain a URI Template
   * (see templated for details). This is an OPTIONAL element. If it does not exist, clients
   * SHOULD act as if the value property is set to an empty string.
   */
  value: TValue;
};

/**
 * The options element contains an enumerated list of possible values for a property.
 * This can be used to provide a UI similar to HTML controls such as:
 * - SELECT &amp; OPTIONS
 * - INPUT.type="radio"
 * - INPUT.type= "checkbox"
 * - INPUT.type= "search"(w / type - ahead suggestions)
 *
 * The options control can also be used in machine-to-machine interactions where it is
 * important to provide the client information about the possible values for a property.
 *
 * The options element contains a set of possible values accessible either by value
 * (e.g.options.inline) or by reference (e.g.via options.link.href) and can be used to provide
 * a constrained list of possble values for a property field.If, when first loaded, the
 * HAL-FORMS template has a pre-set value in the corresponding property.options.selectedValues
 * array attribute, the UI MAY render the form with selected value(s) already chosen.
 *
 * Whatever value is ultimately selected gets placed into the property.options.selectedValues
 * array attribute.When sending the results of the completed HAL-FORMS to the server, content
 * property.options.selectedValues is serialized in a manner compliant with the media type
 * value in the contentType attribute (e.g.appilcation/json, application/x-www-form-urlencded,
 * etc.).
 * @param TValue The type of the value of the options.
 * @param PromptField The name of the prompt field. This is the same as the @see OptionsDto.promptField.
 * @param ValueField The name of the value field. This is the same as the @see OptionsDto.valueField.
 */
export interface OptionsDto<TValue extends SimpleValue, PromptField extends string = "prompt", ValueField extends string = "value"> {
  /**
   * The inline attribute is a JSON array that contains the list of possible values. The
   * inline attribute is OPTIONAL. If the inline attribute is missing or unparseable and the
   * link (see link) attribute is missing or unparseable, then the options element SHOULD be ignored.
   *
   * In it’s simplest form, the inline attribute holds a set of anonymous JSON dictionary
   * objects in the form {'"prompt": "...", "value" : ""} (see A Simple Inline Array of
   * Values). The inline contents can also be an array of unique name-value pairs (see An
   * Inline Array of Name/Value Pairs).
   */
  inline?: OptionsItemDto<TValue, PromptField, ValueField>[];
  /**
   * The link attribute is a JSON dictionary object that contains an href which points to an
   * external HTTP resource which contains the collection of possible values for a property'.
   * The +link attribute is OPTIONAL. If the link attribute is missing or unparseable and the
   * inline (see inline) attribute is missing or unparseable, then the options element SHOULD
   * be ignored.
   *
   * The value returned when dereferencing a link element SHOULD be either a simple array
   * (see An External Array of Values) or a custom collection (see An External Array of
   * Name/Value Pairs). The exact format of the returned collection will vary based on the
   * value of the HTTP Accept header sent with the request.
   *
   * When responding to an options.link request, the server MAY return additional fields
   * (e.g. more than prompt and value fields). These additional fields SHOULD be ignored by
   * the client application.
   */
  link?: OptionsLinkDto;
  /**
   * Indicates the maximum number of items to return in the selectedValues attribute. The
   * client application MAY use this as a UI hint and/or to perform a client-side validation.
   * The maxItems attribute is OPTIONAL. When it is missing or unparseable, the application
   * SHOULD treat the maxItems value as unbounded (e.g. there is no upper limit on the number
   * of items that can be selected and returned).
   */
  maxItems?: number;
  /**
   * Indicated the minimum number of items to return in the selectedValues attribute. The
   * client application MAY use this as a UI hint and/or to perform a client-side validation.
   * The minItems attribute is OPTIONAL. When it is missing or unparseable, the application
   * SHOULD treat the minItems value as 0 (e.g. there is no minimum number of items to be
   * selected and returned).
   */
  minItems?: number;
  /**
   * This attribute contains the name of the JSON dictionary element in the array returned
   * via the inline or link elements to use as the prompt when rendering the options UI. This
   * is an OPTIONAL attribute. If this attribute is missing or unparseable the application
   * SHOULD assume the promptField value is set to "prompt".
   *
   * See Reference Fields for an example.
   */
  promptField?: PromptField;
  /**
   * This is a JSON array that holds the set of values selected from the list of possible
   * values supplied by the inline and link attributes. This is an OPTIONAL element. If it is
   * missing or unparseable, the application SHOULD assume it is an empty JSON array.
   *
   * This attribute MAY be populated when the HAL-FORMS is first requested.In that case, the
   * application can use the value of the selectedValues array to pre-populate the user interface.
   */
  selectedValues?: TValue[];
  /**
   * This attribute contains the name of the JSON dictionary element in the array returned
   * via the inline or link elements to use as the value when rendering the options UI and
   * filling in the selectedValues attribute. This is an OPTIONAL attribute. If this
   * attribute is missing or unparseable the application SHOULD assume the valueField value
   * is set to "value".
   */
  valueField?: ValueField;
}

/**
 * In it’s simplest form, the inline attribute holds a set of anonymous JSON dictionary objects
 * in the form {'"prompt": "...", "value" : ""}
 * @param TValue The type of the value of the options.
 * @param PromptField The name of the prompt field. This is the same as the @see OptionsDto.promptField.
 * @param ValueField The name of the value field. This is the same as the @see OptionsDto.valueField.
 */
export type OptionsItemDto<TValue extends SimpleValue, PromptField extends string = "prompt", ValueField extends string = "value"> = 
   
    /**
     * The human-readable prompt for the parameter. This is a valid JSON string. This is an
     * OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
     * set to the value in the value attribute.
     */
    Record<Exclude<PromptField, ValueField>, string> &
    /**
     * The value of the element.
    */
    Record<Exclude<ValueField, PromptField>, TValue>;

  /**
 * The link attribute is a JSON dictionary object that contains an href which points to an
 * external HTTP resource which contains the collection of possible values for a property'. The
 * +link attribute is OPTIONAL. If the link attribute is missing or unparseable and the inline
 * (see inline) attribute is missing or unparseable, then the options element SHOULD be ignored.
 *
 * The value returned when dereferencing a link element SHOULD be either a simple array (see An
 * External Array of Values) or a custom collection (see An External Array of Name/Value
 * Pairs). The exact format of the returned collection will vary based on the value of the HTTP
 * Accept header sent with the request.
 *
 * When responding to an options.link request, the server MAY return additional fields (e.g.
 * more than prompt and value fields). These additional fields SHOULD be ignored by the client application.
   */
export interface OptionsLinkDto {
  /**
     * The URL associated with the key. This is a REQUIRED property. If this is missing, set to
     * empty or cannot be parsed, the associated link element SHOULD be ignored.
   */
  href: string;
  /**
     * A boolean value that SHOULD be set to true when link.href contains a URI Template
     * [RFC6570]. This is an OPTIONAL attribute. If it is missing or unparseable, the value of
     * templated SHOULD be treated as set to false.
   */
  templated?: boolean;
  /**
     * A string used as a hint to indicate the media type expected when dereferencing the
     * target resource. This is an OPTIONAL attribute. The type value SHOULD be set to
     * application/json or text/csv. It MAY be set to some other value. If the type attribute
     * is missing or unparseable, it SHOULD be assumed to be set to application/json. Client
     * applications SHOULD use the value of the type attribute to populate the HTTP Accept header.
   */
  type?: string;
}

/**
 * Defines how the prompt of a property should be displayed
 */
export enum PropertyPromptDisplayType {
  /**
   * The prompt should be visible.
   */
  Visible = 'visible',
  /**
   * The prompt should be hidden.
   */
  Hidden = 'hidden',
  /**
   * The prompt should be collapsed.
   */
  Collapsed = 'collapsed'
}

/**
 * The type attribute controls the data type of the property value. It is an enumerated
 * attribute. The type can also used to determine the interface control to display for user
 * input. This is an OPTIONAL element. If the type value is not supported by the document
 * consumer, contains a value not understood by the consumer, and/or is missing, the the
 * document consumer SHOULD assume the type attribute is set to the default value: "text"
 * and render the display input as a simple text box. Possible settings for the type value
 * and the expected contents to be returned in it are: hidden, text, textarea, search, tel,
 * url, email, password, date, month, week, time, datetime-local, number, range, color. For
 * hints on how to render and process various type values as well as for guidance on how
 * each type value affects to the contents of the associated value property, see [HTML5TYPE].
 */
export enum PropertyType {
  Hidden = 'hidden',
  Text = 'text',
  Textarea = 'textarea',
  Search = 'search',
  Tel = 'tel',
  Url = 'url',
  Email = 'email',
  Password = 'password',
  Date = 'date',
  Month = 'month',
  Week = 'week',
  Time = 'time',
  DatetimeLocal = 'datetime-local',
  Number = 'number',
  Range = 'range',
  Color = 'color',
  Bool = 'bool',
  DatetimeOffset = 'datetime-offset',
  Duration = 'duration',
  Image = 'image',
  File = 'file',
  Collection = 'collection',
  Object = 'object',
  Percent = "percent",
  Currency = "currency"
}

/**
 * Contains multiple templates.
 */
export interface Templates extends Partial<Record<string, Template>> {
}

/**
 * Contains multiple number templates.
 * This is used in collections where the index in the collection is the key of the template.
 */
export interface NumberTemplates extends Partial<Record<string, NumberTemplate>> {
}

/**
 * The options element contains an enumerated list of possible values for a property.
 * This can be used to provide a UI similar to HTML controls such as:
 * - SELECT &amp; OPTIONS
 * - INPUT.type="radio"
 * - INPUT.type= "checkbox"
 * - INPUT.type= "search"(w / type - ahead suggestions)
 *
 * The options control can also be used in machine-to-machine interactions where it is
 * important to provide the client information about the possible values for a property.
 *
 * The options element contains a set of possible values accessible either by value
 * (e.g.options.inline) or by reference (e.g.via options.link.href) and can be used to provide
 * a constrained list of possble values for a property field.If, when first loaded, the
 * HAL-FORMS template has a pre-set value in the corresponding property.options.selectedValues
 * array attribute, the UI MAY render the form with selected value(s) already chosen.
 *
 * Whatever value is ultimately selected gets placed into the property.options.selectedValues
 * array attribute.When sending the results of the completed HAL-FORMS to the server, content
 * property.options.selectedValues is serialized in a manner compliant with the media type
 * value in the contentType attribute (e.g.appilcation/json, application/x-www-form-urlencded,
 * etc.).
 * @param TValue The type of the value of the options.
 * @param PromptField The name of the prompt field. This is the same as the @see OptionsDto.promptField.
 * @param ValueField The name of the value field. This is the same as the @see OptionsDto.valueField.
 */
export class Options<TValue extends SimpleValue = SimpleValue, PromptField extends string= "prompt", ValueField extends string = "value"> {
  /**
   * The inline attribute is a JSON array that contains the list of possible values. The
   * inline attribute is OPTIONAL. If the inline attribute is missing or unparseable and the
   * link (see link) attribute is missing or unparseable, then the options element SHOULD be ignored.
   *
   * In it’s simplest form, the inline attribute holds a set of anonymous JSON dictionary
   * objects in the form {'"prompt": "...", "value" : ""} (see A Simple Inline Array of
   * Values). The inline contents can also be an array of unique name-value pairs (see An
   * Inline Array of Name/Value Pairs).
   */
  inline?: OptionsItemDto<TValue, PromptField, ValueField>[];
  /**
   * The link attribute is a JSON dictionary object that contains an href which points to an
   * external HTTP resource which contains the collection of possible values for a property'.
   * The +link attribute is OPTIONAL. If the link attribute is missing or unparseable and the
   * inline (see inline) attribute is missing or unparseable, then the options element SHOULD
   * be ignored.
   *
   * The value returned when dereferencing a link element SHOULD be either a simple array
   * (see An External Array of Values) or a custom collection (see An External Array of
   * Name/Value Pairs). The exact format of the returned collection will vary based on the
   * value of the HTTP Accept header sent with the request.
   *
   * When responding to an options.link request, the server MAY return additional fields
   * (e.g. more than prompt and value fields). These additional fields SHOULD be ignored by
   * the client application.
   */
  link?: OptionsLinkDto;
  /**
   * Indicates the maximum number of items to return in the selectedValues attribute. The
   * client application MAY use this as a UI hint and/or to perform a client-side validation.
   * The maxItems attribute is OPTIONAL. When it is missing or unparseable, the application
   * SHOULD treat the maxItems value as unbounded (e.g. there is no upper limit on the number
   * of items that can be selected and returned).
   */
  maxItems?: number;
  /**
   * Indicated the minimum number of items to return in the selectedValues attribute. The
   * client application MAY use this as a UI hint and/or to perform a client-side validation.
   * The minItems attribute is OPTIONAL. When it is missing or unparseable, the application
   * SHOULD treat the minItems value as 0 (e.g. there is no minimum number of items to be
   * selected and returned).
   */
  minItems?: number;
  /**
   * This attribute contains the name of the JSON dictionary element in the array returned
   * via the inline or link elements to use as the prompt when rendering the options UI. This
   * is an OPTIONAL attribute. If this attribute is missing or unparseable the application
   * SHOULD assume the promptField value is set to "prompt".
   *
   * See Reference Fields for an example.
   */
  promptField?: PromptField;
  /**
   * This is a JSON array that holds the set of values selected from the list of possible
   * values supplied by the inline and link attributes. This is an OPTIONAL element. If it is
   * missing or unparseable, the application SHOULD assume it is an empty JSON array.
   *
   * This attribute MAY be populated when the HAL-FORMS is first requested.In that case, the
   * application can use the value of the selectedValues array to pre-populate the user interface.
   */
  selectedValues?: TValue[];
  /**
   * This attribute contains the name of the JSON dictionary element in the array returned
   * via the inline or link elements to use as the value when rendering the options UI and
   * filling in the selectedValues attribute. This is an OPTIONAL attribute. If this
   * attribute is missing or unparseable the application SHOULD assume the valueField value
   * is set to "value".
   */
  valueField?: ValueField;

  constructor(dto?: OptionsDto<TValue, PromptField, ValueField>) {
    Object.assign(this, dto);

    if (!this.inline)
      this.inline = [];
  }
}

/**
 * A JSON object that describes the details of the state transition element (name, required,
 * readOnly, etc.). It appears as an anonymous array of properties as a child of the _templates
 * element (See The _templates Element). There is a set of property attributes that are
 * considered core attributes.There are is also group of property attributes that are
 * considered additional attributes. Any library supporting the HAL-FORMS specification SHOULD
 * support all of the core attributes and MAY support some or all of the additional attributes.
 * @template TValue The type of the value of the property.
 * @template OptionsPromptField The name of the prompt field of the options if this property has options. This is the same as the @see OptionsDto.promptField.
 * @template OptionsValueField The name of the value field of the options if this property has options. This is the same as the @see OptionsDto.valueField.
 */
export class Property<TValue extends SimpleValue = SimpleValue, OptionsPromptField extends string= "prompt", OptionsValueField extends string = "value"> {
  /**
   * The cols attribute specifies the expected maximum number of characters per line to
   * display when rendering the input box. This attribute applies to the associated property
   * element when that property element’s type attribute is set to textarea. The cols
   * attribute is a non-negative integer greater than zero. If the cols attribute is missing
   * or contains an invalid value, it can be assumed to be set to the value of 40. If the
   * type attribute of the associated property element is not set to textarea, this attribute
   * SHOULD be ignored. This is an OPTIONAL attribute and it MAY be ignored. The cols
   * attribute SHOULD appear along with the rows attribute.
   */
  cols?: number;
  /**
   * The max attribute specifies the maximum numeric value for the value setting of a
   * property element. This attribute MAY appear along with the min attribute. This is an
   * OPTIONAL property and it MAY be ignored.
   */
  max?: number;
  /**
   * The maxLength attribute specifies the maximum number of characters allowed in the value
   * property. This attribute MAY appear along with the minLength attribute. This is an
   * OPTIONAL property and it MAY be ignored.
   */
  maxLength?: number;
  /**
   * The min attribute specifies the minimum numeric value for an value setting of a property
   * element. This attribute MAY appear along with the max attribute. This is an OPTIONAL
   * property and it MAY be ignored.
   */
  min?: number;
  /**
   * The minlength attribute specifies the minimum number of characters required in a value
   * property. this attribute MAY appear along with the maxLength attribute. This is an
   * OPTIONAL property and it MAY be ignored.
   */
  minLength?: number;
  /**
   * The property name. This is a valid JSON string. This is a REQUIRED element. If this
   * attribute is missing or set to empty, the client SHOULD ignore this property object completely.
   */
  name!: string;
  /**
   * The options element contains a set of possible values accessible either byValue (e.g.
   * inline) or byReference (e.g. via link.href) and can be used to provide a constrained
   * list of possble values for a property field. See The options Element for details.
   * Support for the options object of a property element is OPTIONAL. If the client does not
   * understand or cannot parse the options element, the options element SHOULD be ignored
   * and the corresponding property SHOULD be treated as a simple text input element.
   */
  options?: Options<TValue, OptionsPromptField, OptionsValueField>;
  /**
   * The placeholder attribute specifies a short hint that describes the expected value of an
   * input field (e.g. a sample value or a short description of the expected format). This is
   * an OPTIONAL field and MAY be ignored.
   */
  placeholder?: TValue;
  /**
   * The human-readable prompt for the parameter. This is a valid JSON string. This is an
   * OPTIONAL element. If this element is missing, clients MAY act as if the prompt value is
   * set to the value in the name attribute.
   */
  prompt?: string;
  /**
   * The display behavior of the prompt. This is an OPTIONAL element.
   * If this element is missing, clients SHOULD act as if the prompt display value is
   * set to @see PropertyPromptDisplayType.Visible.
   */
  promptDisplay?: PropertyPromptDisplayType;
  /**
   * Indicates whether the parameter is read-only. This is a valid JSON boolean. This is an
   * OPTIONAL element. If this element is missing, empty, or set to an unrecognized value, it
   * SHOULD be treated as if the value of readOnly is set to ‘false’.
   */
  readOnly?: boolean;
  /**
   * A regular expression string to be applied to the value of the parameter. Rules for valid values are the same as the HTML5 pattern attribute [HTML5PAT].
   * This is an OPTIONAL element. If this attribute missing, is set to empty, or is unparseable , it SHOULD be ignored.
   */
  regex?: string;
  /**
   * Indicates whether the parameter is required. This is a valid JSON boolean. This is an
   * OPTIONAL element. If this attribute is missing, set to blank or contains an unrecognized
   * value, it SHOULD be treated as if the value of required is set to ‘false’.
   */
  required?: boolean;
  /**
   * The rows attribute specifies the expected maximum number of lines to display when
   * rendering the input box. This attribute applies to the associated property element when
   * that property element’s type attribute is set to "textarea". The cols attribute is a
   * non-negative integer greater than zero. If the cols attribute is missing or contains an
   * invalid value, it can be assumed to be set to the value of 5. If the type attribute of
   * the associated property element is not set to "textarea", this attribute SHOULD be
   * ignored. This is an OPTIONAL attribute and it MAY be ignored. The rows attribute SHOULD
   * appear along with the cols attribute.
   */
  rows?: number;
  /**
   * The step attribute specifies the interval between legal numbers in a value property. For
   * example, if step="3", legal numbers could be -3, 0, 3, 6, etc.
   */
  step?: number;
  /**
   * Indicates whether the value element contains a URI Template [RFC6570] string for the
   * client to resolve. This is a valid JSON boolean. This is an OPTIONAL element. If this
   * element is missing, set to empty, or contains unrecognized content, it SHOULD be treated
   * as if the value of templated is set to ‘false’.
   */
  templated?: boolean;
  /**
   * The _templates which are used for collection and object types. For an object there MUST
   * be only one template with the name "default". For a collection, there MUST be one
   * template with the name "default" which is used to create new list elements. For a
   * collection there CAN be templates which have an index as key. These reassemble the
   * current elements of the collection.
   */
  _templates: Templates;
  /**
   * The type attribute controls the data type of the property value. It is an enumerated
   * attribute. The type can also used to determine the interface control to display for user
   * input. This is an OPTIONAL element. If the type value is not supported by the document
   * consumer, contains a value not understood by the consumer, and/or is missing, the the
   * document consumer SHOULD assume the type attribute is set to the default value: "text"
   * and render the display input as a simple text box. Possible settings for the type value
   * and the expected contents to be returned in it are: hidden, text, textarea, search, tel,
   * url, email, password, date, month, week, time, datetime-local, number, range, color. For
   * hints on how to render and process various type values as well as for guidance on how
   * each type value affects to the contents of the associated value property, see [HTML5TYPE].
   */
  type?: PropertyType;
  /**
   * The property value. This is a valid JSON string. This string MAY contain a URI Template
   * (see templated for details). This is an OPTIONAL element. If it does not exist, clients
   * SHOULD act as if the value property is set to an empty string.
   */
  value!: TValue;

  /**
   * Creates a new instance of the Property class.
   * @param dto - The DTO to use for initialization.
   */
  public constructor(dto: PropertyDto<TValue, OptionsPromptField, OptionsValueField>) {
    Object.assign(this, dto);

    this._templates = (!(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]))) as Templates;

    if (this.options)
      this.options = new Options(dto?.options);
  }
};

interface Array<T> {
  // T[] | [T] enforces a tuple type.
  // {[K in keyof this]: U} keeps a mapped tuple type.
  map<U>(callbackfn: (value: T, index: number, tuple: T[] | [T]) => U, thisArg?: any): {[K in keyof this]: U}
}

export abstract class TemplateBase<TTitle extends string | number = string, TPropertyDtos extends ReadonlyArray<PropertyDto<SimpleValue, string, string>> = ReadonlyArray<PropertyDto<SimpleValue, string, string>>> {
    /**
   * The value of contentType is the media type the client SHOULD use when sending a request
   * body to the server. This is an OPTIONAL element. The value of this property SHOULD be
   * set to "application/json" or "application/x-www-form-urlencoded". It MAY be set to other
   * valid media-type values. If the contentType property is missing, is set to empty, or
   * contains an unrecognized value, the client SHOULD act is if the contentType is set to
   * "application/json". See Encoding Request Bodies for details.
   */
    contentType?: string;
    /**
     * The HTTP method the client SHOULD use when the service request. Any valid HTTP method is
     * allowed. This is a REQUIRED element. If the value is empty or is not understood by the
     * client, the value MUST be treated as an HTTP GET.
     */
    method?: string;
    /**
     * An array of one or more anonymous property elements (see The property Element) that each
     * describe a parameter for the associated state transition. This is an OPTIONAL element.
     * If the array is missing or empty, the properties collection MUST be treated as an empty
     * set of parameters — meaning that the transition is meant to be executed without passing
     * any parameters.
     */
    properties: { [K in keyof TPropertyDtos]: ExtractGenericPropertyType<TPropertyDtos[K]> };
    /**
     * Contains the identifier of the target URL for the client to use when submitting the
     * completed HAL-FORMS template. For example, if the client should submit the completed
     * template to the following URL: http://api.example.org/jobs/ then the target proprety
     * would be target="http://api.example.org/jobs/". This is an OPTIONAL property. If this
     * property is not understood by the recipient, left blank, or contains an invalid URL
     * string, it SHOULD be ignored. The target property holds the same information as the
     * _htarget query string property. If both the target prorperty and the _htarget query
     * string value appear in the same message, the _htarget query string SHOULD be used and
     * the target property SHOULD be ignored.
     */
    target?: string;
    /**
     * A human-readable string that can be used to identify this template. This is a valid JSON
     * string. This is an OPTIONAL element. If it does not exist or is unparsable, consumers
     * MAY use the key value of the template as the value for title.
     */
    title?: TTitle;
  
    /**
     * Creates a new instance of the Template class.
     * @param dto - The DTO to use for initialization.
     */
    constructor(dto: TemplateDto<TPropertyDtos>) {
      Object.assign(this, dto);
  
      this.properties = this.mapProperties(dto.properties);
    }
  
    private mapProperties<TArray extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(array: TArray): { [K in keyof TArray]: ExtractGenericPropertyType<TArray[K]> } {
      return array.map(dto => new Property(dto)) as { [K in keyof TArray]: ExtractGenericPropertyType<TArray[K]> };
    }
  
    /**
     * Returns an object containing the names and values of all properties in the form.
     * If there are no properties, an empty object is returned.
     */
    public get values(): { [name: string]: SimpleValue | undefined } {
      return !this.properties ? {} : Object.fromEntries(this.properties.map(property => [property.name, property.value]));
    }

    /**
     * Returns an object where the keys are the property names and the values are the properties.
     * If there are no properties, an empty object is returned.
     */
    public get propertiesRecord(): Record<string, ExtractGenericPropertyType<TPropertyDtos[number]>> {
      return !this.properties ? {} : Object.fromEntries(this.properties.map(property => [property.name, property as any]));
    }

    /**
     * Checks whether this is a number template.
     * @returns True if this is a number template, false otherwise.
     */
    public isNumberTemplate(): this is NumberTemplate<TPropertyDtos> {
      return NumberTemplate.isNumberTemplate(this);
    }
}

/**
 * A template of a HAL-Form.
 */
export class Template<TPropertyDtos extends ReadonlyArray<PropertyDto<SimpleValue, string, string>> = ReadonlyArray<PropertyDto<SimpleValue, string, string>>> extends TemplateBase<string, TPropertyDtos>{
  /**
   * Creates a new instance of the Template class.
   * @param dto - The DTO to use for initialization.
   */
  constructor(dto: TemplateDto<TPropertyDtos>) {
    super(dto);
  }
}

/**
 * A template of a HAL-Form that is used for a collection of items.
 */
export class NumberTemplate<TPropertyDtos extends ReadonlyArray<PropertyDto<SimpleValue, string, string>> = ReadonlyArray<PropertyDto<SimpleValue, string, string>>> extends TemplateBase<number, TPropertyDtos>{
  /**
   * Creates a new instance of the NumberTemplate class.
   * @param dto - The DTO to use for initialization.
   * @throws An error if the title of the template is not a number.
   */
  constructor(dto: TemplateDto<TPropertyDtos>) {
    super(dto);

    this.title = Number.parseInt(dto!.title!);

    if (!Number.isInteger(this.title))
      throw new Error(`Expected ${dto?.title} to be an integer, but parsing resulted in ${this.title}.`);
  }

  /**
   * Checks whether the specified template is a number template.
   * @param template The template to check.
   * @returns True if the specified template is a number template, false otherwise.
   */
  public static isNumberTemplate<TPropertyDtos extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(template: TemplateBase<string | number, TPropertyDtos>): template is NumberTemplate<TPropertyDtos> {
    return Number.isInteger(template.title);
  }
}

/**
 * A HAL-Forms document is the same as a normal HAL resource, but also has a _templates property.
 */
export class FormsResource extends Resource {

  /**
   * The _templates collection describes the available state transition details including the
   * HTTP method, message content-type, and arguments for the transition. This is a REQUIRED
   * element. If the HAL-FORMS document does not contain this element or the contents are
   * unrecognized or unparseable, the HAL-FORMS document SHOULD be ignored. The _templates
   * element contains a dictionary collection of template objects.A valid HAL-FORMS document
   * has at least one entry in the _templates dictionary collection. Each template contains
   * the following possible properties:
   */
  public declare _templates: Templates;

  /**
   * Constructs a new FormsResource object.
   * @param dto The DTO object used to initialize the FormsResource.
   */
  public constructor(dto: FormsResourceDto) {
    super(dto);

    this._templates = (!(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]))) as Templates;
  }

  /**
   * Returns the template with the specified name.
   * @param name The name of the template to retrieve.
   * @returns The template with the specified name.
   * @throws An error if the form does not have a template with the specified name.
   */
  public getTemplate(name: string) {
    const templateNames = Object.getOwnPropertyNames(this._templates);
    if (!templateNames.includes(name))
      throw new Error(`The form ${this} does not have a _template with the name '${name}'. It only has ${templateNames}.`);

    const template = this._templates[name];

    return template;
  }

  /**
   * Returns the template with the specified title.
   * @param title - The title of the template to retrieve.
   * @returns The template with the specified title.
   * @throws An error if the form does not have a template with the specified title.
   */
  public getTemplateByTitle(title: string): Template {
    const template = Object.entries(this._templates)
      .map(([, t]) => t)
      .find(t => t?.title === title);

      if (template === undefined) {
        const templateTitles = Object.entries(this._templates)
          .map(([, t]) => t?.title);
        throw new Error(`The form ${this} does not have a _template with the title '${title}'. It only has ${templateTitles}.`);
      }

      return template;
  }
}

const d1: PropertyDto<string, "x", "y"> = {
  name: "name1",
  value: "value",
  options: {
    promptField: "x",
    valueField: "y",
    inline: [{x: "promptX", y: "valueY"}],
    selectedValues: []
  }
};
const d2: PropertyDto<number, "xa", "ya"> = {
  name: "name2",
  value: 1,
  options: {
    promptField: "xa",
    valueField: "ya",
    inline: [{xa: "promptX", ya: 42}],
    selectedValues: [1]
  }
};
const dtos= [d1, d2] as const;
const dt: TemplateDto<typeof dtos> = {
  properties: dtos,
}

/**
 * This helper type converts a Property<X, Y, Z> to an OptionsItemDto<X, Y, Z> and preserves the generic parameters.
 */
export type ExtractGenericOptionsItemType<TProperty> = TProperty extends Property<infer X, infer Y, infer Z> ? OptionsItemDto<X, Y, Z> : never
export type ExtractGenericOptionsSelectedValuesType<TProperty> = TProperty extends Property<infer X, infer Y, infer Z> ? Options<X, Y, Z>["selectedValues"] : never
export type ExtractValueType<TProperty> = TProperty extends Property<infer X, infer Y, infer Z> ? X : never