using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Routing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAL.AspNetCore.OData
{
    /// <summary>
    /// A link factory that supports OData query options.
    /// </summary>
    public class ODataLinkFactory : LinkFactory
    {
        private static readonly IReadOnlySet<string> _oDataParameters = new HashSet<string> { "$filter", "$orderby", "$skip", "$top" };
        private static readonly string _oDataParametersAsString = string.Join(",", _oDataParameters);

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataLinkFactory"/> class.
        /// </summary>
        /// <param name="linkGenerator">The link generator from ASP.Net Core.</param>
        /// <param name="actionContextAccessor">The action context accessor.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        public ODataLinkFactory(
            LinkGenerator linkGenerator,
            IActionContextAccessor actionContextAccessor,
            IApiDescriptionGroupCollectionProvider apiExplorer)
            : base(linkGenerator, actionContextAccessor, apiExplorer)
        {
        }

        protected override void AppendQuery(ControllerActionDescriptor descriptor, StringBuilder sb, out bool isTemplated)
        {
            var queryStarted = false;
            var containsODataQueryOptionsAsExtraParameters = descriptor.Parameters.Any(p => _oDataParameters.Contains(p.BindingInfo?.BinderModelName ?? p.Name));

            foreach (var parameter in descriptor.Parameters)
            {
                var shouldAppendODataQueryOptions = !containsODataQueryOptionsAsExtraParameters
                    && parameter.BindingInfo?.BindingSource == BindingSource.Custom
                    && parameter.ParameterType.IsGenericType
                    && parameter.ParameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>);

                var isNormalQueryParameter = parameter.BindingInfo?.BindingSource == BindingSource.Query;

                if (shouldAppendODataQueryOptions || isNormalQueryParameter)
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

                    if (shouldAppendODataQueryOptions)
                    {
                        sb.Append(_oDataParametersAsString);
                    }
                    else
                    {
                        sb.Append(parameter.BindingInfo.BinderModelName ?? parameter.Name);

                        var isEnumerable = parameter.ParameterType.IsGenericType && parameter.ParameterType.IsAssignableTo(typeof(IEnumerable));
                        if (isEnumerable)
                            sb.Append('*');
                    }
                }
            }

            if (queryStarted)
                sb.Append('}');

            isTemplated = queryStarted;
        }
    }
}
