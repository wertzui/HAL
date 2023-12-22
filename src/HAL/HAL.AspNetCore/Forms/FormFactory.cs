using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HAL.AspNetCore.Forms;

/// <inheritdoc/>
public class FormFactory : IFormFactory
{
    /// <summary>
    /// A memory cache.
    /// </summary>
    protected IMemoryCache Cache { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="FormFactory"/> class.
    /// </summary>
    /// <param name="templateFactory">
    /// The factory that is used to generate templates without values.
    /// </param>
    /// <param name="valueFactory">The factory that is used to fill the templates with values.</param>
    /// <param name="linkFactory">The link factory.</param>
    /// <param name="cache">The cache to hold the empty templates.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FormFactory(
        IFormTemplateFactory templateFactory,
        IFormValueFactory valueFactory,
        ILinkFactory linkFactory,
        IMemoryCache cache)
    {
        TemplateFactory = templateFactory ?? throw new ArgumentNullException(nameof(templateFactory));
        ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        LinkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// The link factory.
    /// </summary>
    protected ILinkFactory LinkFactory { get; }

    /// <summary>
    /// The form template factory.
    /// </summary>
    protected IFormTemplateFactory TemplateFactory { get; }

    /// <summary>
    /// The form value factory.
    /// </summary>
    protected IFormValueFactory ValueFactory { get; }

    /// <inheritdoc/>
    public FormTemplate CreateForm<T>(T value, string target, string method, string? title = null, string contentType = Constants.MediaTypes.Json)
    {
        var type = typeof(T);
        var name = type.Name;

        // We do not cache method and title so we can reuse the same template for Create (POST)
        // and Edit (PUT) forms.
        var template = Cache.GetOrCreate(type, entry => TemplateFactory.CreateTemplateFor<T>("template_does_not_need_a_method", contentType: contentType)) ?? throw new InvalidOperationException($"A form template for the type {type.Name} exists in the cache but is null.");
        var filled = ValueFactory.FillWith(template, value);
        filled.Method = method;
        filled.Title = title;
        filled.Target = target;

        return filled;
    }

    /// <inheritdoc/>
    public FormsResource CreateResource(FormTemplate defaultTemplate) => new(new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, defaultTemplate } });

    /// <inheritdoc/>
    public FormsResource CreateResourceForEndpoint<T>(T value, HttpMethod method, string title, string contentType = Constants.MediaTypes.Json, string action = "Get", string? controller = null, object? routeValues = null)
    {
        var target = LinkFactory.GetSelfHref(action, controller, routeValues);
        var template = CreateForm(value, target, method.Method, title, contentType);

        var resource = CreateResource(template)
            .AddSelfLink(target);

        return resource;
    }
}