using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore
{
    /// <inheritdoc/>
    public class ResourceFactory : IResourceFactory
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
        private readonly ILinkFactory _linkFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFactory" /> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        /// <exception cref="ArgumentNullException">
        /// linkFactory
        /// or
        /// apiExplorer
        /// </exception>
        public ResourceFactory(ILinkFactory linkFactory, IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
            _apiExplorer = apiExplorer ?? throw new ArgumentNullException(nameof(apiExplorer));
        }

        /// <inheritdoc/>
        public Resource Create() => new Resource();

        /// <inheritdoc/>
        public Resource<T> Create<T>(T state) => new Resource<T> { State = state };

        /// <inheritdoc/>
        public Resource<T> CreateForGetEndpoint<T>(T state) =>
            Create(state)
            .AddSelfLink(_linkFactory);

        /// <inheritdoc/>
        public Resource CreateForHomeEndpoint(string curieName, string curieUrlTemplate) =>
            Create()
            .AddLinks(_linkFactory.CreateAllLinks(curieName))
            .AddSelfLink(_linkFactory)
            .AddLink("curies", new Link { Name = curieName, Href = curieUrlTemplate, Templated = true });

        /// <inheritdoc/>
        public Resource CreateForHomeEndpointWithSwaggerUi(string curieName) =>
            Create()
            .AddLinks(_linkFactory.CreateAllLinks(curieName))
            .AddSelfLink(_linkFactory)
            .AddSwaggerUiCurieLink(_linkFactory, curieName);

        /// <inheritdoc/>
        public Resource CreateForListEndpoint<T, TId>(IEnumerable<T> resources, Func<T, TId> idAccessor, string getMethod = "Get") =>
            Create()
            .AddEmbedded(
                resources,
                idAccessor,
                r => Create(r)
                    .AddLink(Constants.SelfLinkName, _linkFactory.Create(Constants.SelfLinkName, StripAsyncSuffix(getMethod), null, new { id = idAccessor(r) })));

        private static string StripAsyncSuffix(string actionMethod) => actionMethod.EndsWith("Async") ? actionMethod[0..^5] : actionMethod;
    }
}