using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore
{
    /// <inheritdoc/>
    public class ResourceFactory : IResourceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFactory" /> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        /// <exception cref="ArgumentNullException">linkFactory
        /// or
        /// apiExplorer</exception>
        /// <exception cref="ArgumentNullException">linkFactory
        /// or
        /// apiExplorer</exception>
        public ResourceFactory(ILinkFactory linkFactory)
        {
            LinkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        /// <summary>
        /// Gets the link factory.
        /// </summary>
        protected ILinkFactory LinkFactory { get; }

        /// <inheritdoc/>
        public Resource Create() => new Resource();

        /// <inheritdoc/>
        public Resource<T> Create<T>(T state) => new Resource<T> { State = state };

        /// <inheritdoc/>
        public Resource<T> CreateForGetEndpoint<T>(T state, string action = "Get", string controller = null, object routeValues = null) =>
            Create(state)
            .AddSelfLink(LinkFactory, action, controller, routeValues);

        /// <inheritdoc/>
        public Resource CreateForHomeEndpoint(string curieName, string curieUrlTemplate, ApiVersion version = null) =>
            Create()
            .AddLinks(LinkFactory.CreateAllLinks(curieName, version))
            .AddSelfLink(LinkFactory, "Index", "Home")
            .AddLink("curies", new Link { Name = curieName, Href = curieUrlTemplate, Templated = true });

        /// <inheritdoc/>
        public Resource CreateForHomeEndpointWithSwaggerUi(string curieName, ApiVersion version = null) =>
            Create()
            .AddLinks(LinkFactory.CreateAllLinks(curieName, version))
            .AddSelfLink(LinkFactory, "Index", "Home")
            .AddSwaggerUiCurieLink(LinkFactory, curieName);

        /// <inheritdoc/>
        public Resource CreateForListEndpoint<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get")
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            if (keyAccessor is null)
                throw new ArgumentNullException(nameof(keyAccessor));

            if (idAccessor is null)
                throw new ArgumentNullException(nameof(idAccessor));

            if (string.IsNullOrWhiteSpace(listGetMethod))
                throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or whitespace.", nameof(listGetMethod));

            if (string.IsNullOrWhiteSpace(singleGetMethod))
                throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or whitespace.", nameof(singleGetMethod));

            var resource = Create();

            AddSelfAndEmbedded(resources, keyAccessor, idAccessor, controller, listGetMethod, singleGetMethod, resource);

            return resource;
        }

        /// <inheritdoc/>
        public Resource<Page> CreateForListEndpointWithPaging<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string firstHref = null, string prevHref = null, string nextHref = null, string lastHref = null, Page state = null, string controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get")
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            if (keyAccessor is null)
                throw new ArgumentNullException(nameof(keyAccessor));

            if (idAccessor is null)
                throw new ArgumentNullException(nameof(idAccessor));

            if (string.IsNullOrWhiteSpace(listGetMethod))
                throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or whitespace.", nameof(listGetMethod));

            if (string.IsNullOrWhiteSpace(singleGetMethod))
                throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or whitespace.", nameof(singleGetMethod));

            var resource = Create(state ?? new Page());

            AddSelfAndEmbedded(resources, keyAccessor, idAccessor, controller, listGetMethod, singleGetMethod, resource);

            if (!string.IsNullOrWhiteSpace(firstHref))
                resource.AddLink(new Link { Name = "first", Href = firstHref });

            if (!string.IsNullOrWhiteSpace(prevHref))
                resource.AddLink(new Link { Name = "prev", Href = prevHref });

            if (!string.IsNullOrWhiteSpace(nextHref))
                resource.AddLink(new Link { Name = "next", Href = nextHref });

            if (!string.IsNullOrWhiteSpace(lastHref))
                resource.AddLink(new Link { Name = "last", Href = lastHref });

            return resource;
        }

        private static string StripAsyncSuffix(string actionMethod) => actionMethod.EndsWith("Async") ? actionMethod[0..^5] : actionMethod;

        private void AddSelfAndEmbedded<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string controller, string listGetMethod, string singleGetMethod, Resource resource)
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            if (keyAccessor is null)
                throw new ArgumentNullException(nameof(keyAccessor));

            if (idAccessor is null)
                throw new ArgumentNullException(nameof(idAccessor));

            if (string.IsNullOrWhiteSpace(listGetMethod))
                throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or white space.", nameof(listGetMethod));

            if (string.IsNullOrWhiteSpace(singleGetMethod))
                throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or white space.", nameof(singleGetMethod));

            resource
                .AddSelfLink(LinkFactory, StripAsyncSuffix(listGetMethod), controller)
                .AddEmbedded(
                resources,
                keyAccessor,
                r =>
                {
                    var embeddedResource =
                        Create(r)
                        .AddLinks(LinkFactory.CreateTemplated(StripAsyncSuffix(singleGetMethod), controller), _ => Constants.SelfLinkName, l => l);

                    foreach (var link in embeddedResource.Links[Constants.SelfLinkName])
                    {
                        link.Href = link.Href.Replace("{id}", idAccessor(r)?.ToString());
                        link.Templated = false;
                    }

                    return embeddedResource;
                });
        }
    }
}