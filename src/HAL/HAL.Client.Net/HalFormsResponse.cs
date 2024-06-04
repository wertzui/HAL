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
/// </summary>
public class HalFormsResponse
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions;

    static HalFormsResponse()
    {
        _jsonSerializerOptions = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        _jsonSerializerOptions.Converters.Add(new ExceptionJsonConverterFactory());
    }

    /// <summary>
    /// Creates a new instance of <see cref="HalFormsResponse"/> using the given forms resource.
    /// </summary>
    /// <param name="formsResource">The forms resource to construct this response from.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HalFormsResponse(FormsResource formsResource, HttpStatusCode statusCode)
    {
        FormsResource = formsResource ?? throw new ArgumentNullException(nameof(formsResource), "You must provide a forms resource. If the request failed, provide problem details instead.");
        StatusCode = statusCode;

        if (!Succeeded)
            throw new InvalidOperationException($"When providing a forms resource, the status code must indicate a success. The given status code was {statusCode}.");
    }

    /// <summary>
    /// Creates a new instance of <see cref="HalFormsResponse"/> using the given problem details.
    /// </summary>
    /// <param name="problemDetails">Details about why the request failed.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HalFormsResponse(ProblemDetails problemDetails, HttpStatusCode statusCode)
    {
        ProblemDetails = problemDetails ?? throw new ArgumentNullException(nameof(problemDetails), "You must provide problem details. If the request was successful, provide a forms resource instead.");
        StatusCode = statusCode;

        if (Succeeded)
            throw new InvalidOperationException($"When providing problem details, the status code must not indicate a success. The given status code was {statusCode}.");
    }

    /// <summary>
    /// The problem details if the request was unsuccessful and did contain problem details.
    /// </summary>
    public ProblemDetails? ProblemDetails { get; }

    /// <summary>
    /// The forms resource containing the content of the response if the request was successful and
    /// did contain a forms resource.
    /// </summary>
    public virtual FormsResource? FormsResource { get; }

    /// <summary>
    /// The HTTP status code of the response
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Whether the request succeeded or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(FormsResource))]
    [MemberNotNullWhen(false, nameof(ProblemDetails))]
    public bool Succeeded => (int)StatusCode >= 200 && (int)StatusCode < 300;

    /// <summary>
    /// Throws an <see cref="HttpRequestException"/> if the request was not successful.
    /// </summary>
    /// <exception cref="HttpRequestException"></exception>
    [MemberNotNull(nameof(FormsResource))]
    public void EnsureSuccessStatusCode()
    {
        if (!Succeeded)
            throw new HttpRequestException($"{StatusCode} - ProblemDetails: {JsonSerializer.Serialize(ProblemDetails)}", null, StatusCode);
    }

    /// <summary>
    /// Creates a <see cref="HalFormsResponse{TState}"/> from the given
    /// <paramref name="httpResponse"/>. The Content of the <paramref name="httpResponse"/> is
    /// read during this process, but the response itself is not disposed. The caller has to
    /// take care of disposing the response afterwards.
    /// </summary>
    /// <typeparam name="TState">
    /// The expected state of the response. This should normally be a <see cref="FormsResource{TState}"/>.
    /// </typeparam>
    /// <param name="httpResponse">
    /// The <see cref="HttpResponseMessage"/> to construct this response from.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="HalFormsResponse{TState}"/> populated from the <paramref name="httpResponse"/>.</returns>
    public static async Task<HalFormsResponse<TState>> FromHttpResponse<TState>(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
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
                            Title = "The response did not contain valid problem details. See the detail for the string representation.",
                            Detail = content
                        };

                    return new HalFormsResponse<TState>(problemDetails, httpResponse.StatusCode);
                }
                catch (Exception e)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)httpResponse.StatusCode,
                        Title = "The response did not contain valid problem details. See the detail for the string representation.",
                        Detail = content
                    };
                    problemDetails.Extensions["Exception"] = e;
                    return new HalFormsResponse<TState>(problemDetails, httpResponse.StatusCode);
                }
            }
            else
            {
                try
                {
                    var formsResource = JsonSerializer.Deserialize<FormsResource<TState>>(content, _jsonSerializerOptions);

                    if (formsResource is null)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = (int)httpResponse.StatusCode,
                            Title = "The response did not contain a formsResource although it was successful. See the detail for the string representation.",
                            Detail = content
                        };
                        return new HalFormsResponse<TState>(problemDetails, httpResponse.StatusCode);
                    }

                    return new HalFormsResponse<TState>(formsResource, httpResponse.StatusCode);
                }
                catch (Exception e)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)httpResponse.StatusCode,
                        Title = "The response did not contain a valid formsResource although it was successful. See the detail for the string representation.",
                        Detail = content
                    };
                    problemDetails.Extensions["Exception"] = e;
                    return new HalFormsResponse<TState>(problemDetails, httpResponse.StatusCode);
                }
            }
        }
        catch (Exception e)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)httpResponse.StatusCode,
                Title = "Unable to read the content of the response"
            };
            problemDetails.Extensions["Exception"] = e;
            return new HalFormsResponse<TState>(problemDetails, httpResponse.StatusCode);
        }
    }

    /// <summary>
    /// Creates a <see cref="HalFormsResponse"/> from the given <paramref name="httpResponse"/>. The
    /// Content of the <paramref name="httpResponse"/> is read during this process, but the
    /// response itself is not disposed. The caller has to take care of disposing the response afterwards.
    /// </summary>
    /// <param name="httpResponse">
    /// The <see cref="HttpResponseMessage"/> to construct this response from.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="HalFormsResponse"/> populated from the <paramref name="httpResponse"/>.</returns>
    public static async Task<HalFormsResponse> FromHttpResponse(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
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
                            Title = "The response did not contain valid problem details. See the detail for the string representation.",
                            Detail = content
                        };
                    return new HalFormsResponse(problemDetails, httpResponse.StatusCode);
                }
                catch (Exception e)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)httpResponse.StatusCode,
                        Title = "The response did not contain valid problem details. See the detail for the string representation.",
                        Detail = content
                    };
                    problemDetails.Extensions["Exception"] = e;
                    return new HalFormsResponse(problemDetails, httpResponse.StatusCode);
                }
            }
            else
            {
                try
                {
                    var formsResource = JsonSerializer.Deserialize<FormsResource>(content, _jsonSerializerOptions);

                    if (formsResource is null)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = (int)httpResponse.StatusCode,
                            Title = "The response did not contain a formsResource although it was successful. See the detail for the string representation.",
                            Detail = content
                        };
                        return new HalFormsResponse(problemDetails, httpResponse.StatusCode);
                    }

                    return new HalFormsResponse(formsResource, httpResponse.StatusCode);
                }
                catch (Exception e)
                {
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)httpResponse.StatusCode,
                        Title = "The response did not contain a valid formsResource although it was successful. See the detail for the string representation.",
                        Detail = content
                    };
                    problemDetails.Extensions["Exception"] = e;
                    return new HalFormsResponse(problemDetails, httpResponse.StatusCode);
                }
            }
        }
        catch (Exception e)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)httpResponse.StatusCode,
                Title = "Unable to read the content of the response"
            };
            problemDetails.Extensions["Exception"] = e;
            return new HalFormsResponse(problemDetails, httpResponse.StatusCode);
        }
    }
}

/// <summary>
/// A response from a HAL request.
/// </summary>
/// <typeparam name="TState"></typeparam>
public class HalFormsResponse<TState> : HalFormsResponse
{
    /// <summary>
    /// Creates a new instance of <see cref="HalFormsResponse{TState}"/> using the given forms resource.
    /// </summary>
    /// <param name="formsResource">The forms resource to construct this response from.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HalFormsResponse(FormsResource<TState> formsResource, HttpStatusCode statusCode)
        : base(formsResource, statusCode)
    {
        FormsResource = formsResource;
    }

    /// <summary>
    /// Creates a new instance of <see cref="HalFormsResponse{TState}"/> using the given problem details.
    /// </summary>
    /// <param name="problemDetails">Details about why the request failed.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    public HalFormsResponse(ProblemDetails problemDetails, HttpStatusCode statusCode)
        : base(problemDetails, statusCode)
    {
    }

    /// <inheritdoc/>
    public override FormsResource<TState>? FormsResource { get; }
}