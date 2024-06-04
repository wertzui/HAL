using HAL.Common;
using HAL.Common.Converters;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HAL.Client.Net;

/// <summary>
/// A response from a HAL request.
/// Contains either a Resource or ProblemDetails, depending on the success of the request.
/// </summary>
public class HalResponse<TResource>
    where TResource : Resource
{
    /// <summary>
    /// Creates a new instance of <see cref="HalResponse"/> using the given resource.
    /// </summary>
    /// <param name="resource">The resource to construct this response from.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HalResponse(TResource resource, HttpStatusCode statusCode)
    {
        Resource = resource ?? throw new ArgumentNullException(nameof(resource), "You must provide a resource. If the request failed, provide problem details instead.");
        StatusCode = statusCode;

        if (!Succeeded)
            throw new InvalidOperationException($"When providing a resource, the status code must indicate a success. The given status code was {statusCode}.");
    }

    /// <summary>
    /// Creates a new instance of <see cref="HalResponse"/> using the given problem details.
    /// </summary>
    /// <param name="problemDetails">Details about why the request failed.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HalResponse(ProblemDetails problemDetails, HttpStatusCode statusCode)
    {
        ProblemDetails = problemDetails ?? throw new ArgumentNullException(nameof(problemDetails), "You must provide problem details. If the request was successful, provide a resource instead.");
        StatusCode = statusCode;

        if (Succeeded)
            throw new InvalidOperationException($"When providing problem details, the status code must not indicate a success. The given status code was {statusCode}.");
    }

    /// <summary>
    /// The problem details if the request was unsuccessful and did contain problem details.
    /// </summary>
    public ProblemDetails? ProblemDetails { get; }

    /// <summary>
    /// The resource containing the content of the response if the request was successful and
    /// did contain a resource.
    /// </summary>
    public virtual TResource? Resource { get; }

    /// <summary>
    /// The HTTP status code of the response
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Whether the request succeeded or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Resource))]
    [MemberNotNullWhen(false, nameof(ProblemDetails))]
    public bool Succeeded => (int)StatusCode >= 200 && (int)StatusCode < 300;

    /// <summary>
    /// Throws an <see cref="HttpRequestException"/> if the request was not successful.
    /// </summary>
    /// <exception cref="HttpRequestException"></exception>
    [MemberNotNull(nameof(Resource))]
    public void EnsureSuccessStatusCode()
    {
        if (!Succeeded)
            throw new HttpRequestException($"{StatusCode} - ProblemDetails: {JsonSerializer.Serialize(ProblemDetails)}", null, StatusCode);
    }
}

/// <summary>
/// Base class for parsing a response from a HAL or HAL-Forms request.
/// </summary>
public static class HalResponse
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions;

    static HalResponse()
    {
        _jsonSerializerOptions = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        _jsonSerializerOptions.Converters.Add(new ExceptionJsonConverterFactory());
    }

    /// <summary>
    /// Creates a <see cref="HalResponse{TState}"/> from the given
    /// <paramref name="httpResponse"/>. The Content of the <paramref name="httpResponse"/> is
    /// read during this process, but the response itself is not disposed. The caller has to
    /// take care of disposing the response afterwards.
    /// </summary>
    /// <typeparam name="TResource">
    /// The expected type of the resource. This should normally be a <see cref="Resource"/>, <see cref="Resource{TState}"/>, <see cref="FormsResource"/>, or a <see cref="FormsResource{TState}"/>.
    /// </typeparam>
    /// <param name="httpResponse">
    /// The <see cref="HttpResponseMessage"/> to construct this response from.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="HalResponse{TState}"/> populated from the <paramref name="httpResponse"/>.</returns>
    public static async Task<HalResponse<TResource>> FromHttpResponse<TResource>(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
        where TResource : Resource
    {
        try
        {
            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                try
                {
                    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonSerializerOptions) ??
                        new ProblemDetails
                        {
                            Status = (int)httpResponse.StatusCode,
                            Title = "The response did not contain valid problem details. See the Detail for the string representation and the Status for the original HTTP status code returned by the server.",
                            Detail = content
                        };

                    return new HalResponse<TResource>(problemDetails, httpResponse.StatusCode);
                }
                catch (Exception e)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)httpResponse.StatusCode,
                        Title = "The response did not contain valid problem details. See the Detail for the string representation and the Status for the original HTTP status code returned by the server.",
                        Detail = content
                    };
                    problemDetails.Extensions["Exception"] = e;
                    return new HalResponse<TResource>(problemDetails, httpResponse.StatusCode);
                }
            }
            else
            {
                try
                {
                    var resource = JsonSerializer.Deserialize<TResource>(content, _jsonSerializerOptions);

                    if (resource is null)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = (int)httpResponse.StatusCode,
                            Title = "The response did not contain a resource although it was successful. See the detail for the string representation and the Status for the original HTTP status code returned by the server.",
                            Detail = content
                        };
                        return new HalResponse<TResource>(problemDetails, HttpStatusCode.UnprocessableEntity);
                    }

                    return new HalResponse<TResource>(resource, httpResponse.StatusCode);
                }
                catch (Exception e)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)httpResponse.StatusCode,
                        Title = "The response did not contain a valid resource although it was successful. See the detail for the string representation, the Exception in the Extensions for the exception that occurred and the Status for the original HTTP status code returned by the server.",
                        Detail = content
                    };
                    problemDetails.Extensions["Exception"] = e;
                    return new HalResponse<TResource>(problemDetails, HttpStatusCode.UnprocessableEntity);
                }
            }
        }
        catch (Exception e)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)httpResponse.StatusCode,
                Title = "Unable to read the content of the response. See the Exception in the Extensions for the exception that occurred and the Status for the original HTTP status code returned by the server."
            };
            problemDetails.Extensions["Exception"] = e;
            return new HalResponse<TResource>(problemDetails, HttpStatusCode.UnprocessableEntity);
        }
    }
}