using Asp.Versioning;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Utils;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HAL.AspNetCore;

/// <inheritdoc/>
public partial class LinkFactory : ILinkFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkFactory"/> class.
    /// </summary>
    /// <param name="linkGenerator">The link generator from ASP.Net Core.</param>
    /// <param name="actionContextAccessor">The action context accessor.</param>
    /// <param name="apiExplorer">The API explorer.</param>
    /// <exception cref="System.ArgumentNullException">
    /// urlHelperFactory or actionContextAccessor or apiExplorer or linkGenerator
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// urlHelperFactory or actionContextAccessor or apiExplorer
    /// </exception>
    public LinkFactory(LinkGenerator linkGenerator, IActionContextAccessor actionContextAccessor, IApiDescriptionGroupCollectionProvider apiExplorer)
    {
        ActionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
        ApiExplorer = apiExplorer ?? throw new ArgumentNullException(nameof(apiExplorer));
        LinkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
    }

    private enum TemplatedType
    {
        NotTemplated,
        SimpleTemplated,
        ExplodeTemplated
    }

    /// <summary>
    /// Gets the action context accessor.
    /// </summary>
    protected IActionContextAccessor ActionContextAccessor { get; }

    /// <summary>
    /// Gets the API explorer.
    /// </summary>
    protected IApiDescriptionGroupCollectionProvider ApiExplorer { get; }

    /// <summary>
    /// Gets the link generator.
    /// </summary>
    protected LinkGenerator LinkGenerator { get; }

    /// <inheritdoc/>
    public TResource AddFormLinkForExistingLinkTo<TResource>(TResource resource, string existingRel, string? existingName = null, string? action = null, string? controller = null, object? routeValues = null)
        where TResource : Resource
    {
        ArgumentNullException.ThrowIfNull(resource);

        var selfHref = GetSelfHref(action, controller, routeValues);

        if (resource.Links is null || !resource.Links.TryGetValue(existingRel, out var links) || links.Count == 0)
            throw new ArgumentException($"The resource does not contain a link with the rel '{existingRel}'", nameof(existingRel));

        var link = links.FirstOrDefault(l => existingName == null || l.Name == existingName) ??
            throw new ArgumentException($"The resource does not contain links with the rel '{existingRel}', but none with the name '{existingName}'", nameof(existingName));

        return resource.AddLink(selfHref, link);
    }

    /// <inheritdoc/>
    public TResource AddSelfLinkTo<TResource>(TResource resource, string? action = null, string? controller = null, object? routeValues = null)
        where TResource : Resource
    {
        ArgumentNullException.ThrowIfNull(resource);

        var selfHref = GetSelfHref(action, controller, routeValues);
        return resource.AddSelfLink(selfHref);
    }

    /// <inheritdoc/>
    public TResource AddSwaggerUiCurieLinkTo<TResource>(TResource resource, string name)
        where TResource : Resource
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(name);

        var httpContext = GetHttpContext();
        var href = httpContext is null ?
            LinkGenerator.GetUriByAction("Index", "Home", null, "https", new HostString("example.org"), options: new LinkOptions { AppendTrailingSlash = true }) + "swagger/index.html#/{rel}" :
            LinkGenerator.GetUriByAction(httpContext, "Index", "Home", options: new LinkOptions { AppendTrailingSlash = true }) + "swagger/index.html#/{rel}";
        return resource.AddCurieLink(name, LinkGenerator.GetUriByAction(GetHttpContext(), "Index", "Home", options: new LinkOptions { AppendTrailingSlash = true }) + "swagger/index.html#/{rel}");
    }

    /// <inheritdoc/>
    public Link Create(string href) => new(href);

    /// <inheritdoc/>
    public Link Create(string? name, string href) => new(href) { Name = name };

    /// <inheritdoc/>
    public Link Create(string? name, string? title, string href) => new(href) { Name = name, Title = title };

    /// <inheritdoc/>
    public Link Create(string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
        => Create(null, action, controller, values, protocol, host, fragment);

    /// <inheritdoc/>
    public Link Create(string? name, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
        => Create(name, null, action, controller, values, protocol, host, fragment);

    /// <inheritdoc/>
    public Link Create(string? name, string? title, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
    {
        return TryCreate(name, title, out var link, action, controller, values, protocol, host, fragment)
            ? link
            : throw new InvalidOperationException($"Unable to generate the href value for a link. name: \"{name}\", title: \"{title}\", action: \"{action}\", controller: \"{controller}\", values: \"{values}\", protocol: \"{protocol}\", host: \"{host}\", fragment: \"{fragment}\"");
    }

    /// <inheritdoc/>
    public IDictionary<string, ICollection<Link>> CreateAllLinks(string? prefix = null, ApiVersion? version = null)
    {
        var actionDescriptors = GetControllerActionDescriptorsForVersion(version);
        return actionDescriptors
            .Where(descriptor => descriptor != GetActionContext().ActionDescriptor) // without the self link
            .Select(d => (ControllerName: RemoveVersionFrom(d.ControllerName), CreateTemplated(d)))
            .GroupBy(p => p.ControllerName)
            .ToDictionary(g => string.IsNullOrWhiteSpace(prefix) ? g.Key : $"{prefix}:{g.Key}", g => (ICollection<Link>)g.Select(p => p.Item2).ToHashSet());
    }

    /// <inheritdoc/>
    public ICollection<Link> CreateAllLinksWithoutParameters(ApiVersion? version = null)
    {
        var actionDescriptors = GetControllerActionDescriptorsForVersion(version);
        return actionDescriptors
            .Where(descriptor =>
                descriptor.Parameters.Count == 0 && // only without any parameters (we do not know how to fill these)
                descriptor != GetActionContext().ActionDescriptor) // without the self link
            .Cast<ControllerActionDescriptor>()
            .Select(descriptor => Create(name: $"{RemoveVersionFrom(descriptor.ControllerName)}.{descriptor.ActionName}", action: descriptor.ActionName, controller: descriptor.ControllerName))
            .ToList();
    }

    /// <inheritdoc/>
    public ICollection<Link> CreateTemplated(string action, string? controller = null, ApiVersion? version = null)
    {
        controller ??= GetCurrentControllerName();

        var actionDescriptors = GetControllerActionDescriptorsForVersion(version);
        return actionDescriptors
            .Where(descriptor =>
                descriptor.ControllerName.Equals(controller, StringComparison.OrdinalIgnoreCase) && // controller must match
                descriptor.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase)) // action must match
            .Select(d => CreateTemplated(d))
            .ToList();
    }

    /// <inheritdoc/>
    public Link CreateTemplated(string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
    {
        var link = Create(action, controller, values, protocol, host, fragment);
        if (values == null)
            return link;

        var uri = new Uri(link.Href);
        var path = uri.GetLeftPart(UriPartial.Path);
        var (nonTemplated, templated) = GetParameters(values);
        var hasNonTemplatedParameters = nonTemplated.Count != 0;
        var hasTemplatedParameters = templated.Count != 0;

        if (!hasNonTemplatedParameters && !hasTemplatedParameters)
            return link;

        var uriSb = new StringBuilder(path);

        // non templated parameters with value, separated by &
        if (hasNonTemplatedParameters)
        {
            uriSb.Append('?');

            var isFirst = true;
            foreach (var nonTemplatedParameter in nonTemplated)
            {
                var valueString = nonTemplatedParameter.Value?.ToString();
                if (string.IsNullOrEmpty(valueString))
                    continue;

                if (isFirst)
                    isFirst = false;
                else
                    uriSb.Append('&');

                uriSb.Append(nonTemplatedParameter.Key);
                uriSb.Append('=');
                uriSb.Append(Uri.EscapeDataString(valueString));
            }
        }

        // templated parameters as comma separated list
        if (hasTemplatedParameters)
        {
            uriSb.Append('{');
            if (hasNonTemplatedParameters)
                uriSb.Append('&');
            else
                uriSb.Append('?');

            var isFirst = true;
            foreach (var templatedParameter in templated)
            {
                if (isFirst)
                    isFirst = false;
                else
                    uriSb.Append(',');

                uriSb.Append(templatedParameter);
            }

            uriSb.Append('}');
        }

        link.Href = uriSb.ToString();
        return link;
    }

    /// <inheritdoc/>
    public Link CreateTemplated(ControllerActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var hrefBuilder = new StringBuilder();
        var request = GetHttpContext()?.Request;
        if (request is not null)
            hrefBuilder.Append($"{request.Scheme}://{request.Host}/{request.PathBase}"); // now we have a URL like https://localhost:5001/
        else
            hrefBuilder.Append("https://example.org");

        AppendPath(descriptor, hrefBuilder, out var pathIsTemplated);

        AppendQuery(descriptor, hrefBuilder, out var queryIsTemplated);

        return new Link(hrefBuilder.ToString())
        {
            Name = descriptor.ActionName,
            Templated = pathIsTemplated || queryIsTemplated
        };
    }

    /// <inheritdoc/>
    public string GetSelfHref(string? action = null, string? controller = null, object? routeValues = null, QueryString? queryString = null)
    {
        var httpContext = GetHttpContext();
        if (httpContext is null)
            return "https://example.org";

        var routeValueDictionary = MergeExplicitAndAmbientRouteValues(routeValues, httpContext);

        var path = LinkGenerator.GetUriByAction(httpContext, ActionHelper.StripAsyncSuffix(action), ActionHelper.StripControllerSuffix(controller), routeValueDictionary) ??
            throw new InvalidOperationException($"Unable to generate the self link. request: {httpContext.Request}, action: {action}, controller: {controller}, routeValues: {routeValues}");

        queryString ??= httpContext.Request.QueryString;

        return path + queryString;
    }

    private static RouteValueDictionary MergeExplicitAndAmbientRouteValues(object? routeValues, HttpContext? httpContext)
    {
        var routeValueDictionary = new RouteValueDictionary(routeValues);
        if (httpContext?.Request.RouteValues is not null)
        {
            foreach (var routeValue in httpContext.Request.RouteValues)
            {
                if (!routeValueDictionary.ContainsKey(routeValue.Key))
                    routeValueDictionary.Add(routeValue.Key, routeValue.Value);
            }
        }

        return routeValueDictionary;
    }

    /// <inheritdoc/>
    public bool TryCreate([NotNullWhen(true)] out Link? link, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
        => TryCreate(null, out link, action, controller, values, protocol, host, fragment);

    /// <inheritdoc/>
    public bool TryCreate(string? name, [NotNullWhen(true)] out Link? link, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
        => TryCreate(name, null, out link, action, controller, values, protocol, host, fragment);

    /// <inheritdoc/>
    public bool TryCreate(string? name, string? title, [NotNullWhen(true)] out Link? link, string? action = null, string? controller = null, object? values = null, string? protocol = null, string? host = null, string? fragment = null)
    {
        var httpContext = GetHttpContext();
        var href = httpContext is null ?
            LinkGenerator.GetUriByAction(action!, controller!, values, "https", host is null ? default : new HostString(host), fragment: fragment is null ? default : new FragmentString(fragment)) :
            LinkGenerator.GetUriByAction(GetHttpContext(), ActionHelper.StripAsyncSuffix(action), ActionHelper.StripControllerSuffix(controller), values, protocol, host is null ? null : new HostString(host), fragment: fragment is null ? default : new FragmentString(fragment));
        if (href is null)
        {
            link = null;
            return false;
        }

        link = Create(name, title, href);
        return true;
    }

    /// <summary>
    /// Appends the query part of the link.
    /// </summary>
    protected virtual void AppendQuery(ControllerActionDescriptor descriptor, StringBuilder sb, out bool isTemplated)
    {
        var queryStarted = false;
        foreach (var parameter in descriptor.Parameters)
        {
            if (parameter.BindingInfo?.BindingSource == BindingSource.Query)
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

                sb.Append(parameter.BindingInfo.BinderModelName ?? parameter.Name);

                var isEnumerable = parameter.ParameterType.IsGenericType && parameter.ParameterType.IsAssignableTo(typeof(IEnumerable));
                if (isEnumerable)
                    sb.Append('*');
            }
        }

        if (queryStarted)
            sb.Append('}');

        isTemplated = queryStarted;
    }

    /// <summary>
    /// Gets the current <see cref="HttpContext"/>.
    /// </summary>
    protected HttpContext GetHttpContext() => GetActionContext().HttpContext;

    private static void AppendDirectReplaceParameter(ControllerActionDescriptor descriptor, StringBuilder sb, string routeTemplate, CharEnumerator templateEnumerator)
    {
        var parameterToReplaceBuilder = new StringBuilder();
        while (templateEnumerator.MoveNext())
        {
            if (templateEnumerator.Current == ']')
            {
                AppendDirectReplaceParameter(descriptor, sb, parameterToReplaceBuilder.ToString());
                return;
            }
            else
                parameterToReplaceBuilder.Append(templateEnumerator.Current);
        }
        throw new FormatException($"The route template {routeTemplate} is missing a closing ']'");
    }

    private static void AppendDirectReplaceParameter(ControllerActionDescriptor descriptor, StringBuilder sb, string parameter)
    {
        switch (parameter)
        {
            case "controller":
                sb.Append(descriptor.ControllerName);
                break;

            default:
                throw new FormatException($"Unknown parameter in route template [{parameter}]");
        }
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
                using var templateEnumerator = provider.Template.GetEnumerator();
                char c;
                while (templateEnumerator.MoveNext())
                {
                    c = templateEnumerator.Current;

                    if (c == '[')
                    {
                        AppendDirectReplaceParameter(descriptor, sb, provider.Template, templateEnumerator);
                        continue;
                    }

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

    [GeneratedRegex(@"^(?<name>.*?)((?<controller>controller)(?<version>v(er(sion)?)?\d*))?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, "de-DE")]
    private static partial Regex GetControllerNameRegEx();

    private static (IReadOnlyDictionary<string, object> nonTemplated, IReadOnlyCollection<string> templated) GetParameters(object values)
    {
        var routeValues = new RouteValueDictionary(values);
        var nonTemplated = new Dictionary<string, object>();
        var templated = new List<string>();

        if (routeValues.Count == 0)
            return (nonTemplated, templated);

        var valuesType = values.GetType();

        foreach (var routeValue in routeValues)
        {
            // Check for reference and value types.
            var isTemplated = IsTemplated(routeValue);
            if (isTemplated)
            {
                // Check for collection types and append a '*' to them. This is called an explode modifier.
                var propertyType = valuesType.GetProperty(routeValue.Key)?.PropertyType;
                if (propertyType is not null && propertyType.IsAssignableTo(typeof(IEnumerable)))
                    templated.Add(routeValue.Key + '*');
                else
                    templated.Add(routeValue.Key);
            }
            else
            {
                nonTemplated.Add(routeValue.Key, routeValue.Value!);
            }
        }

        return (nonTemplated, templated);
    }

    private static bool IsTemplated(KeyValuePair<string, object?> routeValue)
    {
        if (routeValue.Value == default)
            return true;

        var valueType = routeValue.Value.GetType();
        if (valueType.IsArray)
            return false;

        var defaultInstance = Activator.CreateInstance(valueType);
        if (object.Equals(routeValue.Value, defaultInstance))
            return true;

        return false;
    }

    private static string RemoveVersionFrom(string controllerName)
    {
        var cleanControllerName = controllerName;

        var match = GetControllerNameRegEx().Match(controllerName);
        var groups = match.Groups;

        if (groups.TryGetValue("name", out var nameMatch) && nameMatch.Success)
            cleanControllerName = nameMatch.Value;

        return cleanControllerName;
    }

    private ActionContext GetActionContext() => ActionContextAccessor.ActionContext ?? throw new InvalidOperationException("Unable to get the current ActionContext.");

    private IEnumerable<ApiDescription> GetActionDescriptorsForVersion(ApiVersion? version)
    {
        var descriptorGroups = ApiExplorer.ApiDescriptionGroups.Items;

        if (version is not null)
        {
            return descriptorGroups
                .SelectMany(g => g.Items)
                .Where(d => d.GetProperty<ApiVersion>() == version);
        }

        return descriptorGroups[^1].Items;
    }

    private IEnumerable<ControllerActionDescriptor> GetControllerActionDescriptorsForVersion(ApiVersion? version)
    {
        var descriptors = GetActionDescriptorsForVersion(version);
        return descriptors
            .Select(description => description.ActionDescriptor as ControllerActionDescriptor)
            .Where(descriptor => descriptor is not null) // only ControllerActionDescriptors
            .Cast<ControllerActionDescriptor>();
    }

    private string GetCurrentControllerName()
    {
        if (GetActionContext().ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            return controllerActionDescriptor.ControllerName;

        throw new InvalidOperationException($"When no controller is given, this method must be executed inside a controller method.");
    }
}