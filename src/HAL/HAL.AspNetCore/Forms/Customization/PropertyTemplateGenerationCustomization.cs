using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Forms;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Customization
{
    /// <summary>
    /// Provides a base class for an easy type safe way to customize the generation of a single HAL-Forms property.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO which has the property.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public abstract class PropertyTemplateGenerationCustomization<TDto, TProperty> : PropertyTemplateGenerationCustomization
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PropertyTemplateGenerationCustomization{TDto, TProperty}"/> class.
        /// </summary>
        /// <param name="property">The property of which the template generation will be changed.</param>
        /// <param name="order">
        /// The order in which to execute this in relation to other customizations for the same
        /// property. The default generation runs at order = 0.
        /// </param>
        /// <param name="exclusive">
        /// Whether this customization is exclusive and should overwrite the default generation.
        /// </param>
        protected PropertyTemplateGenerationCustomization(Expression<Func<TDto, TProperty>> property, int order = 1, bool exclusive = false)
            : base(GetPropertyInfo(property), order, exclusive)
        {
        }

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
    /// You should probably use <see cref="PropertyTemplateGenerationCustomization{TDto, TProperty}"/> instead for a type safe way.
    /// </remarks>
    public abstract class PropertyTemplateGenerationCustomization : IPropertyTemplateGenerationCustomization
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PropertyTemplateGenerationCustomization{TDto, TProperty}"/> class.
        /// </summary>
        /// <param name="property">The property of which the template generation will be changed.</param>
        /// <param name="order">
        /// The order in which to execute this in relation to other customizations for the same
        /// property. The default generation runs at order = 0.
        /// </param>
        /// <param name="exclusive">
        /// Whether this customization is exclusive and should overwrite the default generation.
        /// </param>
        protected PropertyTemplateGenerationCustomization(PropertyInfo property, int order = 1, bool exclusive = false)
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
        public virtual ValueTask ApplyAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory) => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public virtual ValueTask<bool> IncludeAsync(PropertyInfo propertyInfo, Property halFormsProperty, IFormTemplateFactory formTemplateFactory) => ValueTask.FromResult(true);
    }
}