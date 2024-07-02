using HAL.Common.Forms;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Abstractions;

/// <summary>
/// This factory is used to fill values into an existing template.
/// You probably want to use <see cref="IFormFactory"/> instead.
/// </summary>
public interface IFormValueFactory
{
    /// <summary>
    /// Creates a copy of the template which has the values filled in.
    /// </summary>
    /// <param name="template">The template to fill with values.</param>
    /// <param name="value">The values to fill the template with.</param>
    /// <returns>A copy of the template, filled with values.</returns>
    ValueTask<FormTemplate> FillWithAsync(FormTemplate template, object? value);
}