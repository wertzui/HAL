using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAL.Common.Forms;

namespace HAL.AspNetCore.Forms
{
    /// <summary>
    /// Allows to specify the <see cref="PropertyPromptDisplayType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PromptDisplayTypeAttribute : Attribute
    {
        /// <summary>
        /// Creates a new <see cref="PromptDisplayTypeAttribute"/>.
        /// </summary>
        /// <param name="displayType">The <see cref="PropertyPromptDisplayType"/>.</param>
        public PromptDisplayTypeAttribute(PropertyPromptDisplayType displayType)
        {
            PromptDisplay = displayType;
        }

        /// <summary>
        /// The <see cref="PropertyPromptDisplayType"/>
        /// </summary>
        public PropertyPromptDisplayType PromptDisplay { get; }
    }
}
