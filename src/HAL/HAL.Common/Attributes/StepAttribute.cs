using System;

namespace HAL.Common.Attributes
{
    /// <summary>
    /// Attribute to specify the step value for a property or field.
    /// It defines how far legal numbers of a property may be apart.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class StepAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepAttribute"/> class.
        /// </summary>
        public StepAttribute(double step)
        {
            if (step < 0)
                throw new ArgumentException("Step must be greater than or equal to 0.", nameof(step));

            Step = step;
        }

        /// <summary>
        /// Gets the step value.
        /// </summary>
        public double Step { get; }
    }
}
