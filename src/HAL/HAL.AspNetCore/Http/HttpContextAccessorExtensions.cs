using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for retrieving action descriptor information from an <see cref="IHttpContextAccessor"/>
/// instance.
/// </summary>
public static class HttpContextAccessorExtensions
{
    /// <summary>
    /// Retrieves the <see cref="ActionDescriptor"/> associated with the current HTTP request, if available.
    /// </summary>
    /// <remarks>This method returns <see langword="null"/> if the request does not match an MVC action or if
    /// the endpoint metadata does not include an <see cref="ActionDescriptor"/>. Typically used in ASP.NET Core
    /// applications to access routing information for the current request.</remarks>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> representing the current HTTP request. Cannot be null.</param>
    /// <returns>The <see cref="ActionDescriptor"/> for the current request if one is available; otherwise, <see
    /// langword="null"/>.</returns>
    public static ActionDescriptor? GetActionDescriptor(this IHttpContextAccessor httpContextAccessor) => httpContextAccessor.HttpContext?.GetActionDescriptor();

    /// <summary>
    /// Retrieves the <see cref="ControllerActionDescriptor"/> associated with the current HTTP request, if available.
    /// </summary>
    /// <remarks>This method is typically used in ASP.NET Core applications to access metadata about the
    /// controller action being executed. Returns <see langword="null"/> if the request does not correspond to a
    /// controller action endpoint.</remarks>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> representing the current HTTP request. Cannot be null.</param>
    /// <returns>The <see cref="ControllerActionDescriptor"/> for the current request if one is available; otherwise, <see
    /// langword="null"/>.</returns>
    public static ControllerActionDescriptor? GetControllerActionDescriptor(this IHttpContextAccessor httpContextAccessor) => httpContextAccessor.HttpContext?.GetControllerActionDescriptor();
}
