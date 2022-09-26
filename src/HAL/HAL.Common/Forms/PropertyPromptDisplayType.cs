using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAL.Common.Forms
{
    /// <summary>
    /// The display behavior of the property prompt. 
    /// </summary>
    public enum PropertyPromptDisplayType
    {
        /// <summary>
        /// The prompt is visible to the user.
        /// </summary>
        Visible,

        /// <summary>
        /// The prompt is invisible to the user but is still taken into account during layouting.
        /// </summary>
        Hidden,

        /// <summary>
        /// The prompt is invisible to the user and doesn't affect the layout.
        /// </summary>
        Collapsed,
    }
}
