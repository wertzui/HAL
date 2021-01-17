using HAL.AspNetCore.Abstractions;
using HAL.Common;
using HAL.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace HAL.AspNetCore
{
    /// <inheritdoc/>
    public class LinkFactory : ILinkFactory
    {
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkFactory"/> class.
        /// </summary>
        /// <param name="urlHelperFactory">The URL helper factory.</param>
        /// <param name="actionContextAccessor">The action context accessor.</param>
        public LinkFactory(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        /// <inheritdoc/>
        public TResource AddSelfLinkTo<TResource>(TResource resource)
            where TResource : IResource
        {
            return resource.AddSelfLink(_urlHelper.ActionLink());
        }

        /// <inheritdoc/>
        public ILink Create(string href) => new Link { Href = href };

        /// <inheritdoc/>
        public ILink Create(string name, string href) => new Link { Name = name, Href = href };

        /// <inheritdoc/>
        public ILink Create(string name, string title, string href) => new Link { Name = name, Title = title, Href = href };

        /// <inheritdoc/>
        public ILink Create(string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
            => Create(_urlHelper.ActionLink(action, controller, values, protocol, host, fragment));

        /// <inheritdoc/>
        public ILink Create(string name, string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
            => Create(name, _urlHelper.ActionLink(action, controller, values, protocol, host, fragment));

        /// <inheritdoc/>
        public ILink Create(string name, string title, string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
            => Create(name, title, _urlHelper.ActionLink(action, controller, values, protocol, host, fragment));
    }
}