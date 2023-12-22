using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData;
using HAL.AspNetCore.OData.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all services needed for HAL, so you can inject <see cref="IODataResourceFactory" />.
    /// Also calls AddHAL().
    /// </summary>
    /// <param name="builder">The MVC builder.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">services</exception>
    public static IMvcBuilder AddHALOData(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddHAL();

        var services = builder.Services;
        services.AddSingleton<IODataQueryFactory, ODataQueryFactory>();
        services.AddSingleton<IODataResourceFactory, ODataResourceFactory>();
        services.AddSingleton<IODataFormFactory, ODataFormFactory>();
        services.AddSingleton<ILinkFactory, ODataLinkFactory>();


        return builder;
    }
}