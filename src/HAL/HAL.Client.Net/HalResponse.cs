using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace HAL.Client.Net
{
    /// <summary>
    /// A response from a HAL request.
    /// </summary>
    public class HalResponse
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        /// <summary>
        /// Creates a new instance of <see cref="HalResponse"/> using the given resource.
        /// </summary>
        /// <param name="resource">The resource to construct this response from.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        public HalResponse(Resource? resource, HttpStatusCode statusCode)
        {
            Succeeded = true;
            Resource = resource;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a new instance of <see cref="HalResponse"/> using the given problem details.
        /// </summary>
        /// <param name="problemDetails">Details about why the request failed.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        public HalResponse(ProblemDetails problemDetails, HttpStatusCode statusCode)
        {
            ProblemDetails = problemDetails;
            StatusCode = statusCode;
        }

        /// <summary>
        /// The problem details if the request was unsuccessful and did contain problem details.
        /// </summary>
        public ProblemDetails? ProblemDetails { get; }

        /// <summary>
        /// The resource containing the content of the response if the request was successful and
        /// did contain a resource.
        /// </summary>
        public virtual Resource? Resource { get; }

        /// <summary>
        /// The HTTP status code of the response
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Whether the request succeeded or not.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Creates a <see cref="HalResponse{TState}"/> from the given
        /// <paramref name="httpResponse"/>. The Content of the <paramref name="httpResponse"/> is
        /// read during this process, but the response itself is not disposed. The caller has to
        /// take care of disposing the response afterwards.
        /// </summary>
        /// <typeparam name="TState">
        /// The expected state of the response. This should normally be a <see cref="Resource{TState}"/>.
        /// </typeparam>
        /// <param name="httpResponse">
        /// The <see cref="HttpResponseMessage"/> to construct this response from.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="HalResponse{TState}"/> populated from the <paramref name="httpResponse"/>.</returns>
        public static async Task<HalResponse<TState>> FromHttpResponse<TState>(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonSerializerOptions);
                        if (problemDetails is null)
                            problemDetails = new ProblemDetails
                            {
                                Status = (int)httpResponse.StatusCode,
                                Title = "The response did not contain valid problem details. See the detail for the string representation.",
                                Detail = content
                            };
                        return new HalResponse<TState>(problemDetails, httpResponse.StatusCode);
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
                        return new HalResponse<TState>(problemDetails, httpResponse.StatusCode);
                    }
                }
                else
                {
                    try
                    {
                        var resource = JsonSerializer.Deserialize<Resource<TState>>(content, _jsonSerializerOptions);
                        return new HalResponse<TState>(resource, httpResponse.StatusCode);
                    }
                    catch (Exception e)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = (int)httpResponse.StatusCode,
                            Title = "The response did not contain a valid resource although it was successful. See the detail for the string representation.",
                            Detail = content
                        };
                        problemDetails.Extensions["Exception"] = e;
                        return new HalResponse<TState>(problemDetails, httpResponse.StatusCode);
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
                return new HalResponse<TState>(problemDetails, httpResponse.StatusCode);
            }
        }

        /// <summary>
        /// Creates a <see cref="HalResponse"/> from the given <paramref name="httpResponse"/>. The
        /// Content of the <paramref name="httpResponse"/> is read during this process, but the
        /// response itself is not disposed. The caller has to take care of disposing the response afterwards.
        /// </summary>
        /// <param name="httpResponse">
        /// The <see cref="HttpResponseMessage"/> to construct this response from.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="HalResponse"/> populated from the <paramref name="httpResponse"/>.</returns>
        public static async Task<HalResponse> FromHttpResponse(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonSerializerOptions);
                        if (problemDetails is null)
                            problemDetails = new ProblemDetails
                            {
                                Status = (int)httpResponse.StatusCode,
                                Title = "The response did not contain valid problem details. See the detail for the string representation.",
                                Detail = content
                            };
                        return new HalResponse(problemDetails, httpResponse.StatusCode);
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
                        return new HalResponse(problemDetails, httpResponse.StatusCode);
                    }
                }
                else
                {
                    try
                    {
                        var resource = JsonSerializer.Deserialize<Resource>(content, _jsonSerializerOptions);
                        return new HalResponse(resource, httpResponse.StatusCode);
                    }
                    catch (Exception e)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = (int)httpResponse.StatusCode,
                            Title = "The response did not contain a valid resource although it was successful. See the detail for the string representation.",
                            Detail = content
                        };
                        problemDetails.Extensions["Exception"] = e;
                        return new HalResponse(problemDetails, httpResponse.StatusCode);
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
                return new HalResponse(problemDetails, httpResponse.StatusCode);
            }
        }
    }

    /// <summary>
    /// A response from a HAL request.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class HalResponse<TState> : HalResponse
    {
        /// <summary>
        /// Creates a new instance of <see cref="HalResponse{TState}"/> using the given resource.
        /// </summary>
        /// <param name="resource">The resource to construct this response from.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        public HalResponse(Resource<TState>? resource, HttpStatusCode statusCode)
            : base(resource, statusCode)
        {
            Resource = resource;
        }

        /// <summary>
        /// Creates a new instance of <see cref="HalResponse{TState}"/> using the given problem details.
        /// </summary>
        /// <param name="problemDetails">Details about why the request failed.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        public HalResponse(ProblemDetails problemDetails, HttpStatusCode statusCode)
            : base(problemDetails, statusCode)
        {
        }

        /// <inheritdoc/>
        public override Resource<TState>? Resource { get; }
    }
}