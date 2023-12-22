using System.Diagnostics.CodeAnalysis;

namespace HAL.AspNetCore.Utils;

/// <summary>
/// Helper for generating action method names.
/// </summary>
public static class ActionHelper
{
    /// <summary>
    /// Removes the "Async" suffix from the action method name if it exists.
    /// </summary>
    /// <param name="actionMethod">The name of the method with or without "Async" suffix.</param>
    /// <returns>The name of the method without "Async" suffix.</returns>
    [return: NotNullIfNotNull(nameof(actionMethod))]
    public static string? StripAsyncSuffix(string? actionMethod) => actionMethod is not null && actionMethod.EndsWith("Async", System.StringComparison.Ordinal) ? actionMethod[0..^5] : actionMethod;

    /// <summary>
    /// Removes the "Controller" suffix from the controller name if it exists.
    /// </summary>
    /// <param name="controllerName">The name of the controller with or without "Controller" suffix.</param>
    /// <returns>The name of the controller without "Controller suffix.</returns>
    [return: NotNullIfNotNull(nameof(controllerName))]
    public static string? StripControllerSuffix(string? controllerName) => controllerName is not null && controllerName.EndsWith("Controller", System.StringComparison.Ordinal) ? controllerName[0..^10] : controllerName;
}
