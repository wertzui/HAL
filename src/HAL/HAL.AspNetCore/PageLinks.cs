using HAL.AspNetCore.Abstractions;
using HAL.Common;
using System.Collections.Generic;

namespace HAL.AspNetCore;

/// <inheritdoc/>
public record PageLinks(string? FirstHref = null, string? PrevHref = null, string? NextHref = null, string? LastHref = null) : IPageLinks
{
    /// <inheritdoc/>
    public IEnumerable<Link> GetLinks()
    {
        if (FirstHref is not null)
            yield return new Link(FirstHref) { Name = "first" };
        if (PrevHref is not null)
            yield return new Link(PrevHref) { Name = "prev" };
        if (NextHref is not null)
            yield return new Link(NextHref) { Name = "next" };
        if (LastHref is not null)
            yield return new Link(LastHref) { Name = "last" };
    }

    /// <inheritdoc/>
    public void AddTo(Resource resource)
    {
        resource.AddLinks(GetLinks());
    }
}