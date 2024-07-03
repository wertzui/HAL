using HAL.AspNetCore.Forms.Customization;
using HAL.Common.Forms;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for adding HAL-Forms customizations to the <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class HostApplicationBuilderFormsCustomizationExtensions
{
    /// <summary>
    /// Adds a custom implementation of <see cref="IFormsResourceGenerationCustomization"/> which will be used to customize the generation of <see cref="FormsResource"/>.
    /// </summary>
    /// <typeparam name="TFormsResourceGenerationCustomization">The customization to add.</typeparam>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    public static IHostApplicationBuilder AddFormsResourceGenerationCustomization<TFormsResourceGenerationCustomization>(this IHostApplicationBuilder builder)
        where TFormsResourceGenerationCustomization : class, IFormsResourceGenerationCustomization
    {
        builder.Services.AddSingleton<IFormsResourceGenerationCustomization, TFormsResourceGenerationCustomization>();
        return builder;
    }

    /// <summary>
    /// Adds a custom implementation of <see cref="IPropertyTemplateGenerationCustomization"/> which will be used to customize the generation of <see cref="Property">Properties</see>.
    /// </summary>
    /// <typeparam name="TPropertyTemplateGenerationCustomization">The customization to add.</typeparam>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    public static IHostApplicationBuilder AddPropertyTemplateGenerationCustomization<TPropertyTemplateGenerationCustomization>(this IHostApplicationBuilder builder)
        where TPropertyTemplateGenerationCustomization : class, IPropertyTemplateGenerationCustomization
    {
        builder.Services.AddSingleton<IPropertyTemplateGenerationCustomization, TPropertyTemplateGenerationCustomization>();
        return builder;
    }

    /// <summary>
    /// Adds a custom implementation of <see cref="IPropertyValueGenerationCustomization"/> which will be used to customize the generation of <see cref="Property"/> values.
    /// </summary>
    /// <typeparam name="TPropertyValueGenerationCustomization"></typeparam>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    public static IHostApplicationBuilder AddPropertyValueGenerationCustomization<TPropertyValueGenerationCustomization>(this IHostApplicationBuilder builder)
        where TPropertyValueGenerationCustomization : class, IPropertyValueGenerationCustomization
    {
        builder.Services.AddSingleton<IPropertyValueGenerationCustomization, TPropertyValueGenerationCustomization>();
        return builder;
    }
}