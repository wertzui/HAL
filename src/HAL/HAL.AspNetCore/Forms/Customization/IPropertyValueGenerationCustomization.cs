using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Forms;
using System.Reflection;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// Provides a way to customize the value generation for HAL-Forms properties.
    /// </summary>
    /// <remarks>
    /// To customize the template generation, use <see cref="IPropertyTemplateGenerationCustomization"/>.
    /// </remarks>
    public interface IPropertyValueGenerationCustomization
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
        /// <param name="propertyInfo">The <see cref="PropertyInfo" /> of the property to which this customization has been added and for which a HAL-Forms property is being filled out.</param>
        /// <param name="halFormsProperty">The HAL Forms property which has been generated and can be modified.</param>
        /// <returns></returns>
        public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty);

        /// <summary>
        /// Applies the logic defined in this customization to the given property during value generation.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo" /> of the property to which this customization has been added and for which a HAL-Forms property is being filled out.</param>
        /// <param name="halFormsProperty">The HAL Forms property which has been generated and can be modified.</param>
        /// <param name="propertyValue">The value of the property in the DTO.</param>
        /// <param name="dtoValue">The value of the DTO holding this property.</param>
        /// <param name="formValueFactory">The factory which is currently calling this customization.</param>
        public ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, object? propertyValue, object? dtoValue, IFormValueFactory formValueFactory);
    }
}
