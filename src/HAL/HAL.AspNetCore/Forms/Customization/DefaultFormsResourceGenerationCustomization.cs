using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// The default customization for generating HAL-Forms resources.
    /// It adds a default form template to the resource.
    /// </summary>
    public class DefaultFormsResourceGenerationCustomization : IFormsResourceGenerationCustomization
    {
        private readonly ILinkFactory _linkFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFormsResourceGenerationCustomization"/> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        public DefaultFormsResourceGenerationCustomization(ILinkFactory linkFactory)
        {
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        /// <inheritdoc/>
        public bool Exclusive => false;

        /// <inheritdoc/>
        public int Order => 0;

        /// <inheritdoc/>
        public bool AppliesTo<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues)
            => formsResource.Templates.Count == 0;

        /// <inheritdoc/>
        public async ValueTask ApplyAsync<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues, IFormFactory formFactory)
        {
            var target = _linkFactory.GetSelfHref(action, controller, routeValues);
            var template = await formFactory.CreateFormAsync(value, target, method, title, contentType);

            // There shouldn't be another template, because we are checking for no templates in AppliesTo, but just in case
            formsResource.Templates.TryAdd(Constants.DefaultFormTemplateName, template);
        }
    }
}
