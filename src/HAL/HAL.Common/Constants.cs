namespace HAL.Common;

/// <summary>
/// Contains some constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The rel for curies links.
    /// </summary>
    public const string CuriesLinkRel = "curies";

    /// <summary>
    /// The name of the embedded property: "_embedded".
    /// </summary>
    public const string EmbeddedPropertyName = "_embedded";

    /// <summary>
    /// The name of the templates property: "_templates".
    /// </summary>
    public const string FormTemplatesPropertyName = "_templates";

    /// <summary>
    /// The name of the links property: "_links".
    /// </summary>
    public const string LinksPropertyName = "_links";

    /// <summary>
    /// The name of the "self" link which points to the resource where it is added as a link.
    /// </summary>
    public const string SelfLinkName = "self";

    /// <summary>
    /// The name of the default form template: "default".
    /// If nothing else is specified, this template will be used.
    /// </summary>
    public const string DefaultFormTemplateName = "default";

    /// <summary>
    /// Media Types used in conjunction with HAL.
    /// </summary>
    public static class MediaTypes
    {
        /// <summary>
        /// application/hal+json
        /// </summary>
        public const string Hal = "application/hal+json";

        /// <summary>
        /// application/hal-forms+json
        /// </summary>
        public const string HalForms = "application/hal-forms+json";

        /// <summary>
        /// application/prs.hal-forms+json
        /// </summary>
        public const string HalFormsPrs = "application/prs.hal-forms+json";

        /// <summary>
        /// application/json
        /// </summary>
        public const string Json = "application/json";
    }
}