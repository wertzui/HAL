using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.Forms.Customization;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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
    /// <param name="customizations">The customizations which add <see cref="FormTemplate"/>s to the forms resource.</param>
    /// <param name="cache">The cache to hold the empty templates.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FormFactory(
        IFormTemplateFactory templateFactory,
        IFormValueFactory valueFactory,
        ILinkFactory linkFactory,
        IEnumerable<IFormsResourceGenerationCustomization> customizations,
        IMemoryCache cache)
    {
        TemplateFactory = templateFactory ?? throw new ArgumentNullException(nameof(templateFactory));
        ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        LinkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        Customizations = customizations ?? throw new ArgumentNullException(nameof(customizations));
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// The link factory.
    /// </summary>
    protected ILinkFactory LinkFactory { get; }

    /// <summary>
    /// The customizations which are applied in <see cref="CreateResourceForEndpointAsync{T}(T, HttpMethod, string, string, string, string?, object?)"/>.
    /// </summary>
    protected IEnumerable<IFormsResourceGenerationCustomization> Customizations { get; }

    /// <summary>
    /// The form template factory.
    /// </summary>
    protected IFormTemplateFactory TemplateFactory { get; }

    /// <summary>
    /// The form value factory.
    /// </summary>
    protected IFormValueFactory ValueFactory { get; }

    /// <inheritdoc/>
    public ValueTask<FormTemplate> CreateFormAsync<T>(T value, string target, HttpMethod method, string? title = null, string contentType = Constants.MediaTypes.Json)
        => CreateFormAsync(value, target, method.Method, title, contentType);

    /// <inheritdoc/>
    public async ValueTask<FormTemplate> CreateFormAsync<T>(T value, string target, string method, string? title = null, string contentType = Constants.MediaTypes.Json)
    {
        var type = typeof(T);
        var name = "FormTemplate_" + type.Name;

        // We do not cache method and title so we can reuse the same template for Create (POST)
        // and Edit (PUT) forms.
        if (!Cache.TryGetValue(name, out FormTemplate? template) || template is null)
        {
            template = await TemplateFactory.CreateTemplateForAsync<T>("template_does_not_need_a_method", contentType: contentType);
            Cache.Set(name, template);
        }

        var filled = await ValueFactory.FillWithAsync(template, value);
        filled.Method = method;
        filled.Title = title;
        filled.Target = target;

        return filled;
    }

    /// <inheritdoc/>
    public FormsResource CreateResource(FormTemplate defaultTemplate) => new(new Dictionary<string, FormTemplate> { { Constants.DefaultFormTemplateName, defaultTemplate } });

    /// <inheritdoc/>
    public async ValueTask<FormsResource> CreateResourceForEndpointAsync<T>(T value, HttpMethod method, string title, string contentType = Constants.MediaTypes.Json, string action = "Get", string? controller = null, object? routeValues = null)
    {
        var resource = new FormsResource(new Dictionary<string, FormTemplate>())
            .AddSelfLink(LinkFactory, action, controller, routeValues);

        foreach (var customization in Customizations)
        {
            if (customization.AppliesTo(resource, value, method, title, contentType, action, controller, routeValues))
            {
                await customization.ApplyAsync(resource, value, method, title, contentType, action, controller, routeValues, this);
            }
        }

        return resource;
    }
}