using HAL.Common.Forms;
using System.Net.Http;

namespace HAL.AspNetCore.Forms.Abstractions
{
    /// <summary>
    /// A factory to create form resources. This is probably the factory, you want to use when
    /// returning HAL-Form responses.
    /// </summary>
    public interface IFormFactory
    {
        /// <summary>
        /// Creates the <see cref="FormTemplate"/> for the given type and fills it with the given value.
        /// </summary>
        /// <typeparam name="T">The type to create the form template for.</typeparam>
        /// <param name="value">The value to fill the form template with.</param>
        /// <param name="target">The URL to which the form is submitted to.</param>
        /// <param name="method">The HTTP method to use when submitting the form.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="contentType">The content type that is used when submitting the form.</param>
        /// <returns>A <see cref="FormTemplate"/> for the type, filled with the given values.</returns>
        FormTemplate CreateForm<T>(T value, string target, string method, string? title = null, string contentType = "application/json");

        /// <summary>
        /// Creates an empty <see cref="FormsResource"/>.
        /// </summary>
        /// <param name="defaultTemplate">The "default" template.</param>
        /// <returns>An empty <see cref="FormsResource"/>.</returns>
        FormsResource CreateResource(FormTemplate defaultTemplate);

        /// <summary>
        /// Creates a resource which holds a "default" form template for the given type with the
        /// given value.
        /// </summary>
        /// <typeparam name="T">The type to create the form template for.</typeparam>
        /// <param name="value">The value to fill the form template with.</param>
        /// <param name="method">The HTTP method to use when submitting the form.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="contentType">The content type that is used when submitting the form.</param>
        /// <param name="action">The action to which the form will be submitted to.</param>
        /// <param name="controller">The controller to which the form will be submitted to.</param>
        /// <param name="routeValues">The route values to which the form will be submitted to.</param>
        /// <returns></returns>
        FormsResource CreateResourceForEndpoint<T>(T value, HttpMethod method, string title, string contentType = "application/json", string action = "Get", string? controller = null, object? routeValues = null);
    }
}