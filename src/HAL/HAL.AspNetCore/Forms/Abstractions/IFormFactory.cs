using HAL.Common;
using HAL.Common.Forms;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Abstractions;

/// <summary>
/// A factory to create form resources. This is probably the factory, you want to use when
/// returning HAL-Form responses.
/// </summary>
public interface IFormFactory
{
    /// <summary>
    /// Creates the <see cref="FormTemplate"/> using one type for values and another type for validations.
    /// </summary>
    /// <typeparam name="TTemplate">The type that defines the editable properties and validations.</typeparam>
    /// <typeparam name="TValue">The type whose values are used to fill the form.</typeparam>
    /// <param name="value">The value to fill the form template with.</param>
    /// <param name="target">The URL to which the form is submitted to.</param>
    /// <param name="method">The HTTP method to use when submitting the form.</param>
    /// <param name="title">The title of the form.</param>
    /// <param name="contentType">The content type that is used when submitting the form.</param>
    /// <returns>A <see cref="FormTemplate"/> combining values and validations from different types.</returns>
    ValueTask<FormTemplate> CreateFormAsync<TValue, TTemplate>(TValue value, string target, HttpMethod method, string? title = null, string contentType = Constants.MediaTypes.Json);

    /// <summary>
    /// Creates the <see cref="FormTemplate"/> using one type for values and another type for validations.
    /// </summary>
    /// <typeparam name="TTemplate">The type that defines the editable properties and validations.</typeparam>
    /// <typeparam name="TValue">The type whose values are used to fill the form.</typeparam>
    /// <param name="value">The value to fill the form template with.</param>
    /// <param name="target">The URL to which the form is submitted to.</param>
    /// <param name="method">The HTTP method to use when submitting the form.</param>
    /// <param name="title">The title of the form.</param>
    /// <param name="contentType">The content type that is used when submitting the form.</param>
    /// <returns>A <see cref="FormTemplate"/> combining values and validations from different types.</returns>
    ValueTask<FormTemplate> CreateFormAsync<TValue, TTemplate>(TValue value, string target, string method, string? title = null, string contentType = Constants.MediaTypes.Json);

    /// <summary>
    /// Creates an empty <see cref="FormsResource"/>.
    /// </summary>
    /// <param name="defaultTemplate">The "default" template.</param>
    /// <returns>An empty <see cref="FormsResource"/>.</returns>
    FormsResource CreateResource(FormTemplate defaultTemplate);

    /// <summary>
    /// Creates a resource which holds a "default" form template using one type for values and another type for
    /// validations.
    /// </summary>
    /// <typeparam name="TTemplate">The type that defines the editable properties and validations.</typeparam>
    /// <typeparam name="TValue">The type whose values are used to fill the form.</typeparam>
    /// <param name="value">The value to fill the form template with.</param>
    /// <param name="method">The HTTP method to use when submitting the form.</param>
    /// <param name="title">The title of the form.</param>
    /// <param name="contentType">The content type that is used when submitting the form.</param>
    /// <param name="action">The action to which the form will be submitted to.</param>
    /// <param name="controller">The controller to which the form will be submitted to.</param>
    /// <param name="routeValues">The route values to which the form will be submitted to.</param>
    /// <returns></returns>
    ValueTask<FormsResource> CreateResourceForEndpointAsync<TValue, TTemplate>(TValue value, HttpMethod method, string title, string contentType = Constants.MediaTypes.Json, string action = "Get", string? controller = null, object? routeValues = null);
}