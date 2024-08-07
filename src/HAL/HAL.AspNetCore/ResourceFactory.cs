﻿using Asp.Versioning;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Utils;
using HAL.Common;
using System;
using System.Collections.Generic;

namespace HAL.AspNetCore;

/// <inheritdoc/>
public class ResourceFactory : IResourceFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceFactory"/> class.
    /// </summary>
    /// <param name="linkFactory">The link factory.</param>
    /// <exception cref="ArgumentNullException">linkFactory or apiExplorer</exception>
    /// <exception cref="ArgumentNullException">linkFactory or apiExplorer</exception>
    public ResourceFactory(ILinkFactory linkFactory)
    {
        LinkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
    }

    /// <summary>
    /// Gets the link factory.
    /// </summary>
    protected ILinkFactory LinkFactory { get; }

    /// <inheritdoc/>
    public Resource Create() => new();

    /// <inheritdoc/>
    public Resource<T> Create<T>(T state) => new() { State = state };

    /// <inheritdoc/>
    public Resource<T> CreateForEndpoint<T>(T state, string? action = "Get", string? controller = null, object? routeValues = null) =>
        Create(state)
        .AddSelfLink(LinkFactory, action, controller, routeValues);

    /// <inheritdoc/>
    public Resource CreateForHomeEndpoint(string curieName, string curieUrlTemplate, ApiVersion? version = null) =>
        Create()
        .AddLinks(LinkFactory.CreateAllLinks(curieName, version))
        .AddSelfLink(LinkFactory, "Index", "Home")
        .AddLink("curies", new Link(curieUrlTemplate) { Name = curieName, Templated = true });

    /// <inheritdoc/>
    public Resource<TState> CreateForHomeEndpoint<TState>(TState state, string curieName, string curieUrlTemplate, ApiVersion? version = null) =>
        Create(state)
        .AddLinks(LinkFactory.CreateAllLinks(curieName, version))
        .AddSelfLink(LinkFactory, "Index", "Home")
        .AddLink("curies", new Link(curieUrlTemplate) { Name = curieName, Templated = true });

    /// <inheritdoc/>
    public Resource CreateForHomeEndpointWithSwaggerUi(string curieName, ApiVersion? version = null) =>
        Create()
        .AddLinks(LinkFactory.CreateAllLinks(curieName, version))
        .AddSelfLink(LinkFactory, "Index", "Home")
        .AddSwaggerUiCurieLink(LinkFactory, curieName);

    /// <inheritdoc/>
    public Resource<TState> CreateForHomeEndpointWithSwaggerUi<TState>(TState state, string curieName, ApiVersion? version = null) =>
        Create(state)
        .AddLinks(LinkFactory.CreateAllLinks(curieName, version))
        .AddSelfLink(LinkFactory, "Index", "Home")
        .AddSwaggerUiCurieLink(LinkFactory, curieName);

    /// <inheritdoc/>
    public Resource CreateForListEndpoint<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string? controller = null, ApiVersion? version = null, string listGetMethod = "GetList", string singleGetMethod = "Get")
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(keyAccessor);
        ArgumentNullException.ThrowIfNull(idAccessor);

        if (string.IsNullOrWhiteSpace(listGetMethod))
            throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or whitespace.", nameof(listGetMethod));

        if (string.IsNullOrWhiteSpace(singleGetMethod))
            throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or whitespace.", nameof(singleGetMethod));

        var resource = Create();

        AddSelfAndEmbedded(resources, keyAccessor, idAccessor, controller, version, listGetMethod, singleGetMethod, resource);

        return resource;
    }

    /// <inheritdoc/>
    public Resource<Page> CreateForListEndpointWithPaging<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, IPageLinks? links = null, Page? state = null, string? controller = null, ApiVersion? version = null, string listGetMethod = "GetList", string singleGetMethod = "Get")
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(keyAccessor);
        ArgumentNullException.ThrowIfNull(idAccessor);

        if (string.IsNullOrWhiteSpace(listGetMethod))
            throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or whitespace.", nameof(listGetMethod));

        if (string.IsNullOrWhiteSpace(singleGetMethod))
            throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or whitespace.", nameof(singleGetMethod));

        var resource = Create(state ?? new Page());

        AddSelfAndEmbedded(resources, keyAccessor, idAccessor, controller, version, listGetMethod, singleGetMethod, resource);

        links?.AddTo(resource);

        return resource;
    }

    private void AddSelfAndEmbedded<T, TKey, TId>(IEnumerable<T> resources, Func<T, TKey> keyAccessor, Func<T, TId> idAccessor, string? controller, ApiVersion? version, string listGetMethod, string singleGetMethod, Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resources);

        ArgumentNullException.ThrowIfNull(keyAccessor);

        ArgumentNullException.ThrowIfNull(idAccessor);

        if (string.IsNullOrWhiteSpace(listGetMethod))
            throw new ArgumentException($"'{nameof(listGetMethod)}' cannot be null or white space.", nameof(listGetMethod));

        if (string.IsNullOrWhiteSpace(singleGetMethod))
            throw new ArgumentException($"'{nameof(singleGetMethod)}' cannot be null or white space.", nameof(singleGetMethod));

        resource
            .AddSelfLink(LinkFactory, ActionHelper.StripAsyncSuffix(listGetMethod), ActionHelper.StripControllerSuffix(controller))
            .AddEmbedded(
            resources,
            keyAccessor,
            r =>
            {
                var embeddedResource =
                    Create(r)
                    .AddLinks(LinkFactory.CreateTemplated(ActionHelper.StripAsyncSuffix(singleGetMethod), ActionHelper.StripControllerSuffix(controller), version), _ => Constants.SelfLinkName, l => l);

                if (embeddedResource.Links is not null && embeddedResource.Links.TryGetValue(Constants.SelfLinkName, out var selfLinks))
                {
                    foreach (var link in selfLinks)
                    {
                        link.Href = link.Href.Replace("{id}", idAccessor(r)?.ToString());
                        link.Templated = link.Href.Contains('{');
                    }
                }

                return embeddedResource;
            });
    }
}