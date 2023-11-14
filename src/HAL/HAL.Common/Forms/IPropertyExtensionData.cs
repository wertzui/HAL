namespace HAL.Common.Forms;

/// <summary>
/// Every Attribute that implements this interface will have its properties serialized inside
/// the HAL-Forms property template when placed on a property of a DTO. The name will be the
/// name of the Attribute without the Attribute suffix.
/// </summary>
public interface IPropertyExtensionData
{
}