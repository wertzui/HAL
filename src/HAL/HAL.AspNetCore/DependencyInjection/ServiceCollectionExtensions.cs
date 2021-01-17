using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all services needed for HAL, so you can inject <see cref="ILinkFactory"/> and <see cref="IResourceFactory"/>.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IServiceCollection AddHAL(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.AddScoped<ILinkFactory, LinkFactory>();
            services.AddScoped<IResourceFactory, ResourceFactory>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

            return services;
        }
    }
}