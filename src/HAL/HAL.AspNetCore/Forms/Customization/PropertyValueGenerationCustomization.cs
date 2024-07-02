using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Forms;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// Provides a base class for an easy type safe way to customize the generation of a single
    /// HAL-Forms property.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO which has the property.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public abstract class PropertyValueGenerationCustomization<TDto, TProperty> : PropertyValueGenerationCustomization
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PropertyValueGenerationCustomization{TDto, TProperty}"/> class.
        /// </summary>
        /// <param name="property">The property of which the template generation will be changed.</param>
        /// <param name="order">
        /// The order in which to execute this in relation to other customizations for the same
        /// property. The default generation runs at order = 0.
        /// </param>
        /// <param name="exclusive">
        /// Whether this customization is exclusive and should overwrite the default generation.
        /// </param>
        protected PropertyValueGenerationCustomization(Expression<Func<TDto, TProperty>> property, int order = 1, bool exclusive = false)
            : base(GetPropertyInfo(property), order, exclusive)
        {
        }

        /// <inheritdoc/>
        public override ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, object? propertyValue, object? dtoValue, IFormValueFactory formValueFactory)
            => ApplyAsync(propertyInfo, halFormsProperty, (TProperty?)propertyValue, (TDto?)dtoValue, formValueFactory);

        /// <summary>
        /// Override this method to implement the customization logic.
        /// </summary>
        /// <param name="propertyInfo">
        /// The <see cref="PropertyInfo"/> of the property to which this customization has been
        /// added and for which a HAL-Forms property is being filled out.
        /// </param>
        /// <param name="halFormsProperty">
        /// The HAL Forms property which has been generated and can be modified.
        /// </param>
        /// <param name="propertyValue">The value of the property in the DTO.</param>
        /// <param name="dtoValue">The value of the DTO holding this property.</param>
        /// <param name="formValueFactory">The factory which is currently calling this customization.</param>
        public abstract ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, TProperty? propertyValue, TDto? dtoValue, IFormValueFactory formValueFactory);

        private static PropertyInfo GetPropertyInfo(Expression<Func<TDto, TProperty>> property)
        {
            if (property.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Expression {property} is not a {nameof(MemberExpression)}");

            if (memberExpression.Member is not PropertyInfo propertyInfo)
                throw new ArgumentException($"Expression {property} is not a {nameof(MemberExpression)} pointing to a Property.");

            return propertyInfo;
        }
    }

    /// <summary>
    /// Provides a base class for an easy way to customize the generation of a single HAL-Forms property.
    /// </summary>
    /// <remarks>
    /// You should probably use <see cref="PropertyValueGenerationCustomization{TDto, TProperty}"/>
    /// instead for a type safe way.
    /// </remarks>
    public abstract class PropertyValueGenerationCustomization : IPropertyValueGenerationCustomization
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PropertyValueGenerationCustomization{TDto, TProperty}"/> class.
        /// </summary>
        /// <param name="property">The property of which the template generation will be changed.</param>
        /// <param name="order">
        /// The order in which to execute this in relation to other customizations for the same
        /// property. The default generation runs at order = 0.
        /// </param>
        /// <param name="exclusive">
        /// Whether this customization is exclusive and should overwrite the default generation.
        /// </param>
        protected PropertyValueGenerationCustomization(PropertyInfo property, int order = 1, bool exclusive = false)
        {
            if (property.DeclaringType is null)
                throw new ArgumentException("Property must have a declaring type.", nameof(property));

            Property = property;
            Order = order;
            Exclusive = exclusive;
        }

        /// <inheritdoc/>
        public bool Exclusive { get; }

        /// <inheritdoc/>
        public int Order { get; }

        /// <summary>
        /// The property to apply the customization to.
        /// </summary>
        protected PropertyInfo Property { get; }

        /// <inheritdoc/>
        public bool AppliesTo(PropertyInfo propertyInfo, Property halFormsProperty)
            => propertyInfo == Property;

        /// <inheritdoc/>
        public virtual ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, object? propertyValue, object? dtoValue, IFormValueFactory formValueFactory) => ValueTask.CompletedTask;
    }
}