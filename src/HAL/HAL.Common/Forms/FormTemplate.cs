using System.Collections.Generic;
using System.Net.Http;

namespace HAL.Common.Forms;

/// <summary>
/// A template of a HAL-Form.
/// </summary>
public class FormTemplate
{
    /// <summary>
    /// The value of contentType is the media type the client SHOULD use when sending a request
    /// body to the server. This is an OPTIONAL element. The value of this property SHOULD be
    /// set to "application/json" or "application/x-www-form-urlencoded". It MAY be set to other
    /// valid media-type values. If the contentType property is missing, is set to empty, or
    /// contains an unrecognized value, the client SHOULD act is if the contentType is set to
    /// "application/json". See Encoding Request Bodies for details.
    /// </summary>
    public string ContentType { get; set; } = Constants.MediaTypes.Json;

    /// <summary>
    /// The HTTP method the client SHOULD use when the service request. Any valid HTTP method is
    /// allowed. This is a REQUIRED element. If the value is empty or is not understood by the
    /// client, the value MUST be treated as an HTTP GET.
    /// </summary>
    public string Method { get; set; } = HttpMethod.Get.Method;

    /// <summary>
    /// An array of one or more anonymous property elements (see The property Element) that each
    /// describe a parameter for the associated state transition. This is an OPTIONAL element.
    /// If the array is missing or empty, the properties collection MUST be treated as an empty
    /// set of parameters — meaning that the transition is meant to be executed without passing
    /// any parameters.
    /// </summary>
    public ICollection<Property>? Properties { get; set; }

    /// <summary>
    /// Contains the identifier of the target URL for the client to use when submitting the
    /// completed HAL-FORMS template. For example, if the client should submit the completed
    /// template to the following URL: http://api.example.org/jobs/ then the target proprety
    /// would be target="http://api.example.org/jobs/". This is an OPTIONAL property. If this
    /// property is not understood by the recipient, left blank, or contains an invalid URL
    /// string, it SHOULD be ignored. The target property holds the same information as the
    /// _htarget query string property. If both the target prorperty and the _htarget query
    /// string value appear in the same message, the _htarget query string SHOULD be used and
    /// the target property SHOULD be ignored.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// A human-readable string that can be used to identify this template. This is a valid JSON
    /// string. This is an OPTIONAL element. If it does not exist or is unparsable, consumers
    /// MAY use the key value of the template as the value for title.
    /// </summary>
    public string? Title { get; set; }
}