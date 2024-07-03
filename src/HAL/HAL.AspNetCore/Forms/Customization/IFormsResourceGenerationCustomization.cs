using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Forms;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// Allows customizing the generation of HAL-Forms resources.
    /// </summary>
    public interface IFormsResourceGenerationCustomization
    {
        /// <summary>
        /// Whether the logic defined in this customization should be executed exclusively or in addition to the default logic.
        /// When set to <c>false</c> (default) the logic will be executed after the default logic.
        /// </summary>
        public bool Exclusive { get; }

        /// <summary>
        /// The order in which multiple non exclusive customizations should be applied.
        /// Default is 0.
        /// A lower value means that the logic defined in this customization will be executed before the logic of customizations with a higher value.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Determines whether this customization applies to the given HAL-Forms resource.
        /// </summary>
        /// <param name="formsResource">The forms resource for which the customization is being applied.</param>
        /// <param name="value">The value to fill the form template with.</param>
        /// <param name="method">The HTTP method to use when submitting the form.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="contentType">The content type that is used when submitting the form.</param>
        /// <param name="action">The action to which the form will be submitted to.</param>
        /// <param name="controller">The controller to which the form will be submitted to.</param>
        /// <param name="routeValues">The route values to which the form will be submitted to.</param>
        /// <returns></returns>
        public bool AppliesTo<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues);

        /// <summary>
        /// Applies the logic defined in this customization to the given HAL-Forms resource during generation.
        /// </summary>
        /// <param name="formsResource">The forms resource for which the customization is being applied.</param>
        /// <param name="value">The value to fill the form template with.</param>
        /// <param name="method">The HTTP method to use when submitting the form.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="contentType">The content type that is used when submitting the form.</param>
        /// <param name="action">The action to which the form will be submitted to.</param>
        /// <param name="controller">The controller to which the form will be submitted to.</param>
        /// <param name="routeValues">The route values to which the form will be submitted to.</param>
        /// <param name="formFactory">The factory which is currently calling this customization.</param>
        public ValueTask ApplyAsync<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues, IFormFactory formFactory);
    }
}
