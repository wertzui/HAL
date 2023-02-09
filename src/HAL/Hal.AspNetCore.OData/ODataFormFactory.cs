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
using System.Net.Http;

namespace HAL.AspNetCore.OData
{
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
        public FormsResource<Page> CreateForODataListEndpointUsingSkipTopPaging<TDto, TEntity, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, ODataQueryOptions<TEntity> oDataQueryOptions, long maxTop = 50, long? totalCount = null, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get")
        {
            var resource = _resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(resources, keyAccessor, idAccessor, oDataQueryOptions, maxTop, totalCount, controller, listGetMethod, singleGetMethod);

            var searchForm = CreateSearchForm<TDto>(resource.GetSelfLink().Href);

            var formResource = new FormsResource<Page>(searchForm) { Embedded = resource.Embedded, Links = resource.Links, State = resource.State };

            return formResource;
        }

        /// <inheritdoc/>
        public FormsResource<Page> CreateForODataListEndpointUsingSkipTopPaging<TDto, TKey, TId>(IEnumerable<TDto> resources, Func<TDto, TKey> keyAccessor, Func<TDto, TId> idAccessor, IPageLinks links, Page page, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get")
        {
            var resource = _resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(resources, keyAccessor, idAccessor, links, page, controller, listGetMethod, singleGetMethod);

            var searchForm = CreateSearchForm<TDto>(resource.GetSelfLink().Href);

            var formResource = new FormsResource<Page>(searchForm) { Embedded = resource.Embedded, Links = resource.Links, State = resource.State };

            return formResource;
        }

        private FormTemplate CreateSearchForm<TDto>(string listGetMethod)
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

            return searchForm;
        }
    }
}
