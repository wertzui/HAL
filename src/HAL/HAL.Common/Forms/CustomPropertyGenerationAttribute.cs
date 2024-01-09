using System;
using System.Reflection;

namespace HAL.Common.Forms;

/// <summary>
/// Allows to customize the generation of a HAL Forms property.
/// Users need to derive from this class and implement the <see cref="Apply"/> method.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public abstract class CustomPropertyGenerationAttribute : Attribute
{
    /// <summary>
    /// Whether the logic defined in this attribute should be executed exclusively or in addition to the default logic.
    /// When set to <c>falce</c> the logic will be executed after the default logic.
    /// </summary>
    public bool Exclusive { get; set; }

    /// <summary>
    /// The order in which multiple non exclusive attributes should be applied.
    /// Default is 0.
    /// A lower value means that the logic defined in this attribute will be executed before the logic of attributes with a higher value.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Applies the logic defined in this attribute to the given property.
    /// </summary>
    /// <param name="dtoType">The type of the class containing the property. This is the class where the property is in to which this attribute has been added and is normally the DTO.</param>
    /// <param name="propertyInfo">The <see cref="PropertyInfo" /> of the property to which this attribute has been added and for which a HAL-Forms property is being created.</param>
    /// <param name="halFormsProperty">The HAL Forms property which has been generated and can be modified.</param>
    public abstract void Apply(Type dtoType, PropertyInfo propertyInfo, Property halFormsProperty);
}
