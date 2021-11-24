using System.Runtime.Serialization;

namespace HAL.Common.Forms
{
    public enum PropertyType
    {
        Hidden,
        Text,
        Textarea,
        Search,
        Tel,
        Url,
        Email,
        Password,
        Date,
        Month,
        Week,
        Time,

        [EnumMember(Value = "datetime-local")]
        DatetimeLocal,

        Number,
        Range,
        Color,
        Bool,

        [EnumMember(Value = "datetime-offset")]
        DatetimeOffset,

        Duration,
        Image,
        File,
        Collection,
        Object
    }
}