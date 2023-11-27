using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HAL.AspNetCore.OData;

/// <inheritdoc/>
public class ODataFormFactory : FormFactory, IODataFormFactory
{
    private readonly IODataResourceFactory _resourceFactory;

    /// <summary>
    /// Creates a new instance of the <see cref="ODataFormFactory"/> class.
    /// </summary>
    /// <param name="templateFactory">The template factory.</param>
    /// <param name="valueFactory">The value factory.</param>
    /// <param name="linkFactory">The link factory.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="resourceFactory">The resource factory.</param>
    /// <exception cref="ArgumentNullException">resourceFactory</exception>
    public ODataFormFactory(
        IFormTemplateFactory templateFactory,
        IFormValueFactory valueFactory,
        ILinkFactory linkFactory,
        IMemoryCache cache,
        IODataResourceFactory resourceFactory)
        : base(templateFactory, valueFactory, linkFactory, cache)
    {
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
    }

    /// <inheritdoc/>
    public FormsResource<Page> CreateForODataListEndpointUsingSkipTopPaging<TDto, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, ODataRawQueryOptions oDataQueryOptions, long maxTop = 50, long? totalCount = null, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put")
    {
        var resource = _resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(resources, keyAccessor, idAccessor, oDataQueryOptions, maxTop, totalCount, controller, listGetMethod, singleGetMethod);
        var editTarget = LinkFactory.TryCreate(link: out var editLink, action: listPutMethod, controller: controller) ? editLink.Href : null;

        var searchForm = CreateListSearchForm<TDto>(resource.GetSelfLink().Href, oDataQueryOptions);
        var editForm = CreateListEditForm<TDto>(editTarget);
        var forms = new Dictionary<string, FormTemplate>
        {
            { searchForm.Title!, searchForm },
            { editForm.Title!, editForm }
        };

        var formResource = new FormsResource<Page>(forms) { Embedded = resource.Embedded, Links = resource.Links, State = resource.State };

        return formResource;
    }

    private FormTemplate CreateListEditForm<TDto>(string? target)
    {
        var cacheKey = typeof(TDto) + "_ListEdit";
        var template = Cache.GetOrCreate(cacheKey, entry => CreateEditFormTemplate<TDto>(target)) ?? throw new InvalidOperationException($"A form template for the type {cacheKey} exists in the cache but is null.");
        return template;
    }

    private FormTemplate CreateListSearchForm<TDto>(string listGetMethod, ODataRawQueryOptions queryOptions)
    {
        var cacheKey = typeof(TDto) + "_ListSearch";
        var template = Cache.GetOrCreate(cacheKey, entry => CreateSearchFormTemplate<TDto>(listGetMethod)) ?? throw new InvalidOperationException($"A form template for the type {cacheKey} exists in the cache but is null.");

        // A new template needs to be created here to add the $orderby property value.
        var searchForm = new FormTemplate
        {
            ContentType = template.ContentType,
            Method = template.Method,
            Properties = template.Properties
                ?.Select(p => new Property(p.Name)
                {
                    Cols = p.Cols,
                    Extensions = p.Extensions,
                    Max = p.Max,
                    MaxLength = p.MaxLength,
                    Min = p.Min,
                    MinLength = p.MinLength,
                    Options = p.Options,
                    Placeholder = p.Placeholder,
                    Prompt = p.Prompt,
                    PromptDisplay = p.PromptDisplay,
                    ReadOnly = p.ReadOnly,
                    Regex = p.Regex,
                    Required = p.Required,
                    Rows = p.Rows,
                    Step = p.Step,
                    Templated = p.Templated,
                    Templates = p.Templates,
                    Type = p.Type,
                    Value = p.Name == "$orderby" ? queryOptions.OrderBy : p.Value
                })
                .ToList(),
            Target = template.Target,
            Title = template.Title,
        };

        return searchForm;
    }

    private FormTemplate CreateEditFormTemplate<TDto>(string? target)
    {
        var editForm = TemplateFactory.CreateTemplateFor<TDto>(HttpMethod.Put.ToString(), "Edit");
        editForm.Target = target;

        editForm.Properties ??= new List<Property>();

        return editForm;
    }

    private FormTemplate CreateSearchFormTemplate<TDto>(string listGetMethod)
    {
        var searchForm = TemplateFactory.CreateTemplateFor<TDto>(HttpMethod.Get.ToString(), "Search", "application/x-www-form-urlencoded");
        searchForm.Target = listGetMethod;

        if (searchForm.Properties is not null)
        {
            foreach (var property in searchForm.Properties)
            {
                property.ReadOnly = false;
                property.Required = false;
                property.Value = null;
            }
        }

        searchForm.Properties ??= new List<Property>();
        searchForm.Properties.Add(new Property("$orderby") { Type = PropertyType.Hidden });

        return searchForm;
    }
}
