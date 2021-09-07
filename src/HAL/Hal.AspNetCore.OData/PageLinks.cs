using HAL.Common;
using System.Collections.Generic;

namespace HAL.AspNetCore.OData
{
    /// <inheritdoc/>
    public record PageLinks(string? FirstHref = null, string? PrevHref = null, string? NextHref = null, string? LastHref = null) : IPageLinks
    {
        /// <inheritdoc/>
        public IEnumerable<Link> GetLinks()
        {
            if (FirstHref is not null)
                yield return new Link { Name = "first", Href = FirstHref };
            if (PrevHref is not null)
                yield return new Link { Name = "prev", Href = PrevHref };
            if (NextHref is not null)
                yield return new Link { Name = "next", Href = NextHref };
            if (LastHref is not null)
                yield return new Link { Name = "last", Href = LastHref };
        }

        /// <inheritdoc/>
        public void AddTo(Resource resource)
        {
            resource.AddLinks(GetLinks());
        }
    }
}