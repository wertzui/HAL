using HAL.AspNetCore;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.Forms.Customization;
using HAL.Common.Converters;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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
        services.AddSingleton<IPropertyTemplateGenerationCustomization, DefaultPropertyTemplateGeneration>();

        services.AddSingleton<IFormValueFactory, FormValueFactory>();
        services.AddSingleton<IPropertyValueGenerationCustomization, DefaultPropertyValueGeneration>();

        services.AddSingleton<IFormFactory, FormFactory>();
        services.AddSingleton<IFormsResourceGenerationCustomization, DefaultFormsResourceGenerationCustomization>();

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
                o.JsonSerializerOptions.Converters.Add(new JsonEnumConverter(JsonNamingPolicy.CamelCase));
                o.JsonSerializerOptions.Converters.Add(new ExceptionJsonConverterFactory());

                o.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, new FormsResourceOfTJsonConverterFactory());
                o.JsonSerializerOptions.TypeInfoResolverChain.Insert(1, new FormsResourceJsonConverter());
                o.JsonSerializerOptions.TypeInfoResolverChain.Insert(2, new ResourceOfTJsonConverterFactory());
                o.JsonSerializerOptions.TypeInfoResolverChain.Insert(3, new FormsResourceJsonConverter());

                // Ensure DefaultJsonTypeInfoResolver is in the chain to provide default serialization behavior
                // When using this for a WebApplicationBuilder, it is already added by default, but in some other scenarios it might not be.
                if (!o.JsonSerializerOptions.TypeInfoResolverChain.OfType<DefaultJsonTypeInfoResolver>().Any())
                    o.JsonSerializerOptions.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
            });

        return builder;
    }
}