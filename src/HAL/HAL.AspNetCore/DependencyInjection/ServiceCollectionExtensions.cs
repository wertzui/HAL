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
        /// Adds all services needed for HAL, so you can inject <see cref="ILinkFactory" /> and <see cref="IResourceFactory" />.
        /// </summary>
        /// <param name="builder">The MVC builder.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IMvcBuilder AddHAL(this IMvcBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;
            services.AddScoped<ILinkFactory, LinkFactory>();
            services.AddScoped<IResourceFactory, ResourceFactory>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

            // This is needed for Action Link generation.
            // See https://github.com/dotnet/aspnetcore/issues/14606
            builder.AddMvcOptions(o => o.SuppressAsyncSuffixInActionNames = true);

            return builder;
        }
    }
}