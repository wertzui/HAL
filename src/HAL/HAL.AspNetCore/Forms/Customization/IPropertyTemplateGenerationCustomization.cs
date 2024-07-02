using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Forms;
using System.Reflection;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// Provides a way to customize the template generation for HAL-Forms properties.
    /// </summary>
    /// <remarks>
    /// To customize the value generation, use <see cref="IPropertyValueGenerationCustomization"/>.
    /// </remarks>
    public interface IPropertyTemplateGenerationCustomization
    {
        /// <summary>
        /// Whether the logic defined in this customization should be executed exclusively or in addition to the default logic.
        /// When set to <c>false</c> (default) the logic will be executed after the default logic.
        /// </summary>
        public bool Exclusive { get; }

        /// <summary>
        /// The order in which multiple non exclusive customizations should be applied.
        /// Default is 0.
        /// A lower value means that the logic defined in this customization will be executed before the logic of customizations with a higher value.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Determines whether this customization applies to the given property.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo" /> of the property to which this customization has been added and for which a HAL-Forms property is being created.</param>
        /// <param name="halFormsProperty">The HAL Forms property which has been generated and can be modified.</param>
        /// <returns></returns>
        public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty);

        /// <summary>
        /// Applies the logic defined in this customization to the given property during template generation.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo" /> of the property to which this customization has been added and for which a HAL-Forms property is being created.</param>
        /// <param name="halFormsProperty">The HAL Forms property which has been generated and can be modified.</param>
        /// <param name="formTemplateFactory">The factory which is currently calling this customization.</param>
        public ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory);

        /// <summary>
        /// Whether the property should be included in the template or not.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo" /> of the property to which this customization has been added and for which a HAL-Forms property is being created.</param>
        /// <param name="halFormsProperty">The HAL Forms property which has been generated and can be modified.</param>
        /// <param name="formTemplateFactory">The factory which is currently calling this customization.</param>
        /// <returns><c>true</c> if the property should be included in the template; <c>false</c> otherwise.</returns>
        ValueTask<bool> IncludeAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory);
    }
}
