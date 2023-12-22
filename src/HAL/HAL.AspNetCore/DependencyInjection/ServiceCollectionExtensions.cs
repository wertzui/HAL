using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Converters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

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
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;

        services.AddSingleton<ILinkFactory, LinkFactory>();
        services.AddSingleton<IResourceFactory, ResourceFactory>();

        services.AddSingleton<IFormTemplateFactory, FormTemplateFactory>();
        services.AddSingleton<IFormValueFactory, FormValueFactory>();
        services.AddSingleton<IFormFactory, FormFactory>();

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
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase));
                o.JsonSerializerOptions.Converters.Add(new ExceptionJsonConverterFactory());
            });

        return builder;
    }
}