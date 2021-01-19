using HAL.AspNetCore.Abstractions;
using HAL.Common;
using HAL.Common.Abstractions;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore
{
    /// <inheritdoc/>
    public class ResourceFactory : IResourceFactory
    {
        private readonly ILinkFactory _linkFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFactory"/> class.
        /// </summary>
        /// <param name="linkFactory">The link factory.</param>
        public ResourceFactory(ILinkFactory linkFactory)
        {
            _linkFactory = linkFactory;
        }

        /// <inheritdoc/>
        public IResource Create() => new Resource();

        /// <inheritdoc/>
        public IResource<T> Create<T>(T state) => new Resource<T> { State = state };

        /// <inheritdoc/>
        public IResource<T> CreateForGetEndpoint<T>(T state) =>
            Create(state)
            .AddSelfLink(_linkFactory);

        /// <inheritdoc/>
        public IResource CreateForListEndpoint<T, TId>(IEnumerable<T> resources, Func<T, TId> idAccessor) =>
            Create()
            .AddEmbedded(
                resources,
                idAccessor,
                r => Create(r)
                    .AddLink(Constants.SelfLinkName, _linkFactory.Create(Constants.SelfLinkName, "Get", null, new { id = idAccessor(r) })));
    }
}