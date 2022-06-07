using HAL.Common;

namespace HAL.Client.Net
{
    /// <summary>
    /// Contains extension methods for <see cref="Resource"/>.
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Finds all links with the given rel. If the resource does not have any links, or no links with the rel, an empty collection is returned.
        /// </summary>
        /// <param name="resource">The resource in which to search for links.</param>
        /// <param name="rel">The relation of the links.</param>
        /// <returns>All links with the given rel or an empty collection.</returns>
        public static IEnumerable<Link> FindLinks(this Resource resource, string rel)
        {
            if (resource.Links is null)
                return Array.Empty<Link>();

            if (!resource.Links.TryGetValue(rel, out var links))
                return Array.Empty<Link>();

            if (links is null)
                return Array.Empty<Link>();

            return links.Where(l => l is not null);
        }

        /// <summary>
        /// Finds the first link with the given rel and optionally the given name.
        /// </summary>
        /// <param name="resource">The resource in which to search for links.</param>
        /// <param name="rel">The relation of the links.</param>
        /// <param name="name">Optionally the name of the link. Use it if the resource has multiple links with the same <paramref name="rel"/>.</param>
        /// <returns>The first link with the given rel and (optionally) name or null if no link is found.</returns>
        public static Link? FindLink(this Resource resource, string rel, string? name = default)
         => resource.FindLinks(rel).FirstOrDefault(l => name is null || l.Name == name);

        /// <summary>
        /// Tries to find the first link with the given rel and optionally the given name.
        /// </summary>
        /// <param name="resource">The resource in which to search for links.</param>
        /// <param name="rel">The relation of the links.</param>
        /// <param name="link">The first link if found.</param>
        /// <param name="name">Optionally the name of the link. Use it if the resource has multiple links with the same <paramref name="rel"/>.</param>
        /// <returns>True if the link was found; false otherwise.</returns>
        public static bool TryFindLink(this Resource resource, string rel, out Link? link, string? name = default)
        {
            link = resource.FindLink(rel, name);
            return link is not null;
        }
    }
}
