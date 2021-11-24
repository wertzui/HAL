using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HAL.AspNetCore.Forms
{
    /// <inheritdoc/>
    public class FormFactory : IFormFactory
    {
        private readonly IMemoryCache _cache;
        private readonly ILinkFactory _linkFactory;
        private readonly IFormTemplateFactory _templateFactory;
        private readonly IFormValueFactory _valueFactory;

        /// <summary>
        /// Creates a new instance of the <see cref="FormFactory"/> class.
        /// </summary>
        /// <param name="templateFactory">
        /// The factory that is used to generate templates without values.
        /// </param>
        /// <param name="valueFactory">The factory that is used to fill the templates with values.</param>
        /// <param name="linkFactory">The link factory.</param>
        /// <param name="cache">The cache to hold the empty templates.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public FormFactory(
            IFormTemplateFactory templateFactory,
            IFormValueFactory valueFactory,
            ILinkFactory linkFactory,
            IMemoryCache cache)
        {
            _templateFactory = templateFactory ?? throw new System.ArgumentNullException(nameof(templateFactory));
            _valueFactory = valueFactory ?? throw new System.ArgumentNullException(nameof(valueFactory));
            _linkFactory = linkFactory ?? throw new System.ArgumentNullException(nameof(linkFactory));
            _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc/>
        public FormTemplate CreateForm<T>(T value, string target, string method, string title = null, string contentType = "application/json")
        {
            var type = typeof(T);
            var name = type.Name;

            // We do not cache method and title so we can reuse the same template for Create (POST)
            // and Edit (PUT) forms.
            var template = _cache.GetOrCreate(type, entry => _templateFactory.CreateTemplateFor<T>(null, null, contentType));

            var filled = _valueFactory.FillWith(template, value);
            filled.Method = method;
            filled.Title = title;
            filled.Target = target;

            return filled;
        }

        /// <inheritdoc/>
        public FormsResource CreateResource() => new FormsResource();

        /// <inheritdoc/>
        public FormsResource CreateResourceForEndpoint<T>(T value, HttpMethod method, string title, string contentType = "application/json", string action = "Get", string controller = null, object routeValues = null)
        {
            var resource = CreateResource();
            _linkFactory.AddSelfLinkTo(resource, action, controller, routeValues);

            var template = CreateForm(value, resource.Links[Constants.SelfLinkName].First().Href, method.Method, title, contentType);

            resource.Templates = new Dictionary<string, FormTemplate> { { "default", template } };

            return resource;
        }
    }
}