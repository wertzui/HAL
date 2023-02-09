using HAL.AspNetCore.OData;
using HAL.AspNetCore.OData.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
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
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddHAL();

            var services = builder.Services;
            services.AddSingleton<IODataQueryFactory, ODataQueryFactory>();
            services.AddScoped<IODataResourceFactory, ODataResourceFactory>();
            services.AddScoped<IODataFormFactory, ODataFormFactory>();

            return builder;
        }
    }
}