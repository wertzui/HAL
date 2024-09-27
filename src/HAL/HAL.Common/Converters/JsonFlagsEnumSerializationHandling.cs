namespace HAL.Common
{
    /// <summary>
    /// Defines how flags enums are serialized.
    /// </summary>
    public enum JsonFlagsEnumSerializationHandling
    {
        /// <summary>
        /// Serialize flags enum as an array of strings.
        /// </summary>
        Array,
        /// <summary>
        /// Serialize flags enum as a comma separated string.
        /// </summary>
        String
    }
}