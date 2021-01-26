using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;
using System.Linq;

namespace HAL.AspNetCore
{
    /// <inheritdoc/>
    public class LinkFactory : ILinkFactory
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkFactory" /> class.
        /// </summary>
        /// <param name="urlHelperFactory">The URL helper factory.</param>
        /// <param name="actionContextAccessor">The action context accessor.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        /// <exception cref="System.ArgumentNullException">
        /// urlHelperFactory
        /// or
        /// actionContextAccessor
        /// or
        /// apiExplorer
        /// </exception>
        public LinkFactory(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            if (urlHelperFactory is null)
            {
                throw new System.ArgumentNullException(nameof(urlHelperFactory));
            }

            if (actionContextAccessor is null)
            {
                throw new System.ArgumentNullException(nameof(actionContextAccessor));
            }

            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _apiExplorer = apiExplorer ?? throw new System.ArgumentNullException(nameof(apiExplorer));
        }

        /// <inheritdoc/>
        public TResource AddSelfLinkTo<TResource>(TResource resource)
            where TResource : Resource
        {
            return resource.AddSelfLink(_urlHelper.ActionLink());
        }

        /// <inheritdoc/>
        public Link Create(string href) => new Link { Href = href };

        /// <inheritdoc/>
        public Link Create(string name, string href) => new Link { Name = name, Href = href };

        /// <inheritdoc/>
        public Link Create(string name, string title, string href) => new Link { Name = name, Title = title, Href = href };

        /// <inheritdoc/>
        public Link Create(string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
            => Create(_urlHelper.ActionLink(action, controller, values, protocol, host, fragment));

        /// <inheritdoc/>
        public Link Create(string name, string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
            => Create(name, _urlHelper.ActionLink(action, controller, values, protocol, host, fragment));

        /// <inheritdoc/>
        public Link Create(string name, string title, string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
            => Create(name, title, _urlHelper.ActionLink(action, controller, values, protocol, host, fragment));

        /// <inheritdoc/>
        public ICollection<Link> CreateAllLinksWithoutParameters() =>
            _apiExplorer.ApiDescriptionGroups.Items[0].Items
                    .Select(action => action.ActionDescriptor as ControllerActionDescriptor)
                    .Where(descriptor =>
                        descriptor is not null && // only ControllerActionDescriptors
                        descriptor.Parameters.Count == 0 && // only without any parameters (we do not know how to fill these)
                        descriptor != _urlHelper.ActionContext.ActionDescriptor) // without the self link
                    .Select(descriptor => Create(name: descriptor.DisplayName, action: descriptor.ActionName, controller: descriptor.ControllerName))
                    .ToList();
    }
}