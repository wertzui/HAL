using Microsoft.AspNet.OData.Query;

namespace HAL.AspNetCore.OData
{
    /// <summary>
    /// Parsed $skip and $top values from <see cref="ODataRawQueryOptions"/>.
    /// </summary>
    public record ODataParsedQueryOptions(long Skip, long Top)
    {
    }
}