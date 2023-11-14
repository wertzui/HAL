using HAL.Common;
using System.Collections.Generic;

namespace HAL.AspNetCore.Abstractions;

/// <summary>
/// The links for navigating through a paged list.
/// </summary>
public interface IPageLinks
{
    /// <summary>
    /// Gets or sets the href to the first page.
    /// </summary>
    string? FirstHref { get; init; }

    /// <summary>
    /// Gets or sets the href to the last page.
    /// </summary>
    string? LastHref { get; init; }

    /// <summary>
    /// Gets or sets the href to the next page.
    /// </summary>
    string? NextHref { get; init; }

    /// <summary>
    /// Gets or sets the href to the previous page.
    /// </summary>
    string? PrevHref { get; init; }

    /// <summary>
    /// Adds all links which are not null to the resource.
    /// </summary>
    /// <param name="resource">The resource to add the links to.</param>
    void AddTo(Resource resource);

    /// <summary>
    /// Gets all links which are not null.
    /// </summary>
    /// <returns>All links which are not null.</returns>
    IEnumerable<Link> GetLinks();
}