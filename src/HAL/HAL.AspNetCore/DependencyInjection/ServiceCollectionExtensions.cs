using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Converters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all services needed for HAL and HAL-Forms, so you can inject <see cref="ILinkFactory" />, <see cref="IResourceFactory" /> and <see cref="IFormFactory"/>.
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

            services.AddScoped<IFormTemplateFactory, FormTemplateFactory>();
            services.AddScoped<IFormValueFactory, FormValueFactory>();
            services.AddScoped<IFormFactory, FormFactory>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddMemoryCache();

            builder
                // This is needed for Action Link generation.
                // See https://github.com/dotnet/aspnetcore/issues/14606
                .AddMvcOptions(o => o.SuppressAsyncSuffixInActionNames = true)
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                    o.JsonSerializerOptions.Converters.Insert(0, new DateOnlyJsonConverter());
                    o.JsonSerializerOptions.Converters.Insert(1, new TimeOnlyJsonConverter());
                });

            return builder;
        }
    }
}