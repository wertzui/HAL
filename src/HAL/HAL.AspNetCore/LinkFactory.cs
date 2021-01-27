using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public TResource AddSwaggerUiCurieLinkTo<TResource>(TResource resource, string name)
            where TResource : Resource
        {
            return resource.AddCurieLink(name, _urlHelper.ActionLink() + "swagger/index.html#/{rel}");
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
                    .Select(descriptor => Create(name: $"{descriptor.ControllerName}.{descriptor.ActionName}", action: descriptor.ActionName, controller: descriptor.ControllerName))
                    .ToList();

        /// <inheritdoc/>
        public IDictionary<string, ICollection<Link>> CreateAllLinks(string prefix = null) =>
            _apiExplorer.ApiDescriptionGroups.Items[0].Items
                    .Select(action => action.ActionDescriptor as ControllerActionDescriptor)
                    .Where(descriptor =>
                        descriptor is not null && // only ControllerActionDescriptors
                        descriptor != _urlHelper.ActionContext.ActionDescriptor) // without the self link
                    .Select(d => (d.ControllerName, CreateTemplated(d)))
                    .GroupBy(p => p.ControllerName)
                    .ToDictionary(g => string.IsNullOrWhiteSpace(prefix) ? g.Key : $"{prefix}:{g.Key}", g => (ICollection<Link>)g.Select(p => p.Item2).ToList());

        /// <inheritdoc/>
        public Link CreateTemplated(string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null)
        {
            var link = Create(action, controller, values, protocol, host, fragment);
            if (values == null)
                return link;

            var uri = new Uri(link.Href);
            var path = uri.GetLeftPart(UriPartial.Path);
            var parameters = GetParameters(values);
            var hasNonTemplatedParameters = parameters.nonTemplated.Any();
            var hasTemplatedParameters = parameters.templated.Any();

            if (!hasNonTemplatedParameters && !hasTemplatedParameters)
                return link;

            var uriSb = new StringBuilder(path);

            // non templated parameters with value, separated by &
            if (hasNonTemplatedParameters)
            {
                uriSb.Append("?");

                var isFirst = true;
                foreach (var nonTemplatedParameter in parameters.nonTemplated)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        uriSb.Append("&");

                    uriSb.Append(nonTemplatedParameter.Key);
                    uriSb.Append("=");
                    uriSb.Append(Uri.EscapeDataString(nonTemplatedParameter.Value?.ToString()));
                }
            }

            // templated parameters as comma separated list
            if (hasTemplatedParameters)
            {
                uriSb.Append("{");
                if (hasNonTemplatedParameters)
                    uriSb.Append("&");
                else
                    uriSb.Append("?");

                var isFirst = true;
                foreach (var templatedParameter in parameters.templated)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        uriSb.Append(",");

                    uriSb.Append(templatedParameter);
                }

                uriSb.Append("}");
            }

            link.Href = uriSb.ToString();
            return link;
        }

        private (IReadOnlyDictionary<string, object> nonTemplated, IReadOnlyCollection<string> templated) GetParameters(object values)
        {
            var routeValues = new RouteValueDictionary(values);
            var nonTemplated = new Dictionary<string, object>();
            var templated = new List<string>();

            foreach (var routeValue in routeValues)
            {
                // check for reference and value types.
                if (routeValue.Value == default || routeValue.Value == Activator.CreateInstance(routeValue.Value.GetType()))
                    templated.Add(routeValue.Key);
                else
                    nonTemplated.Add(routeValue.Key, routeValue.Value);
            }

            return (nonTemplated, templated);
        }

        /// <inheritdoc/>
        public Link CreateTemplated(ControllerActionDescriptor descriptor)
        {
            var hrefBuilder = new StringBuilder();
            hrefBuilder.Append(_urlHelper.Link(null, null)); // now we have a url like https://localhost:5001/

            AppendPath(descriptor, hrefBuilder, out var pathIsTemplated);

            AppendQuery(descriptor, hrefBuilder, out var queryIsTemplated);

            return new Link
            {
                Name = descriptor.ActionName,
                Href = hrefBuilder.ToString(),
                Templated = pathIsTemplated || queryIsTemplated
            };
        }

        private static void AppendQuery(ControllerActionDescriptor descriptor, StringBuilder sb, out bool isTemplated)
        {
            var queryStarted = false;
            foreach (var parameter in descriptor.Parameters)
            {
                if (parameter.BindingInfo.BindingSource == BindingSource.Query)
                {
                    if (!queryStarted)
                    {
                        queryStarted = true;
                        sb.Append("{?");
                    }
                    else
                    {
                        sb.Append(',');
                    }

                    sb.Append(parameter.Name);
                }
            }

            if (queryStarted)
                sb.Append('}');

            isTemplated = queryStarted;
        }

        private static void AppendPath(ControllerActionDescriptor descriptor, StringBuilder sb, out bool isTemplated)
        {
            isTemplated = false;

            var isFirst = true;
            foreach (var metaData in descriptor.EndpointMetadata)
            {
                if (metaData is IRouteTemplateProvider provider && provider.Template is not null)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append('/');

                    // strip the type part ":long" from "{id:long}"
                    var isInType = false;
                    foreach (var c in provider.Template)
                    {
                        if (!isInType)
                        {
                            if (c == ':')
                                isInType = true;
                            else
                                sb.Append(c);
                        }
                        else
                        {
                            if (c == '}')
                            {
                                isTemplated = true;
                                isInType = false;
                                sb.Append(c);
                            }
                        }
                    }
                }
            }
        }
    }
}