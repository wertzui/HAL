using System.ComponentModel;

namespace HAL.Common.Forms;

/// <summary>
/// The display behavior of the property prompt. 
/// </summary>
[Description("The display behavior of the property prompt.")]
public enum PropertyPromptDisplayType
{
    /// <summary>
    /// The prompt is visible to the user.
    /// </summary>
    [Description("The prompt is visible to the user.")]
    Visible,

    /// <summary>
    /// The prompt is invisible to the user but is still taken into account during layouting.
    /// </summary>
    [Description("The prompt is invisible to the user but is still taken into account during layouting.")]
    Hidden,

    /// <summary>
    /// The prompt is invisible to the user and doesn't affect the layout.
    /// </summary>
    [Description("The prompt is invisible to the user and doesn't affect the layout.")]
    Collapsed,
}
