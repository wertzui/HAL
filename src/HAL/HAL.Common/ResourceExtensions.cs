using HAL.Common.Abstractions;
using System;
using System.Collections.Generic;

namespace HAL.Common
{
    /// <summary>
    /// Contains extension methods for <see cref="IResource"/>
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Adds an embedded resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="key">The key of the embedded resource.</param>
        /// <param name="embedded">The embedded resource to add.</param>
        /// <returns></returns>
        public static TResource AddEmbedded<TResource>(this TResource resource, string key, TResource embedded)
            where TResource : IResource
        {
            var collection = GetOrCreateEmbeddedCollection(resource, key);

            collection.Add(embedded);

            return resource;
        }

        /// <summary>
        /// Adds multiple embedded resources from a collection of states.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="source">The source collection containing the states that will be converted to embedded resources.</param>
        /// <param name="keySelector">A function to select the key of every resource.</param>
        /// <param name="embeddedSelector">A function to convert each state of the source collection into a resource.</param>
        /// <returns></returns>
        public static TResource AddEmbedded<TResource, TSource, TKey>(this TResource resource, IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TResource> embeddedSelector)
            where TResource : IResource
        {
            foreach (var item in source)
            {
                var key = keySelector(item)?.ToString();
                var value = embeddedSelector(item);
                resource.AddEmbedded(key, value);
            }

            return resource;
        }

        /// <summary>
        /// Adds a link and uses its name as key.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="link">The link to add.</param>
        /// <returns></returns>
        public static TResource AddLink<TResource>(this TResource resource, ILink link)
            where TResource : IResource
        {
            var key = link.Name;

            return resource.AddLink(key, link);
        }

        /// <summary>
        /// Adds a link with the specified key.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="key">The key to add the link to.</param>
        /// <param name="link">The link to add.</param>
        /// <returns></returns>
        public static TResource AddLink<TResource>(this TResource resource, string key, ILink link)
            where TResource : IResource
        {
            var collection = GetOrCreateLinkCollection(resource, key);

            collection.Add(link);

            return resource;
        }

        /// <summary>
        /// Adds multiple links from a collection.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="source">The source collection containing the items that will be converted to links.</param>
        /// <param name="keySelector">A function to select the key of link.</param>
        /// <param name="linkSelector">A function to convert each item of the source collection into a link.</param>
        /// <returns></returns>
        public static TResource AddLink<TResource, TSource, TKey>(this TResource resource, IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, ILink> linkSelector)
            where TResource : IResource
        {
            foreach (var item in source)
            {
                var key = keySelector(item)?.ToString();
                var value = linkSelector(item);
                resource.AddLink(key, value);
            }

            return resource;
        }

        /// <summary>
        /// Adds multiple links from a collection while using the links names as keys.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="source">The source collection containing the items that will be converted to links.</param>
        /// <param name="linkSelector">A function to convert each item of the source collection into a link.</param>
        /// <returns></returns>
        public static TResource AddLink<TResource, TSource>(this TResource resource, IEnumerable<TSource> source, Func<TSource, ILink> linkSelector)
            where TResource : IResource
        {
            foreach (var item in source)
            {
                var value = linkSelector(item);
                resource.AddLink(value);
            }

            return resource;
        }

        /// <summary>
        /// Adds a "self" link.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="href">The URL to the resource itself.</param>
        /// <returns></returns>
        public static TResource AddSelfLink<TResource>(this TResource resource, string href)
            where TResource : IResource
        {
            var link = new Link { Name = Constants.SelfLinkName, Href = href };

            return resource.AddLink(link);
        }

        private static ICollection<IResource> GetOrCreateEmbeddedCollection(IResource resource, string key)
        {
            if (resource.Embedded is null)
                resource.Embedded = new Dictionary<string, ICollection<IResource>>();

            if (!resource.Embedded.TryGetValue(key, out var collection))
            {
                collection = new List<IResource>();
                resource.Embedded[key] = collection;
            }

            return collection;
        }

        private static ICollection<ILink> GetOrCreateLinkCollection(IResource resource, string key)
        {
            if (resource.Links is null)
                resource.Links = new Dictionary<string, ICollection<ILink>>();

            if (!resource.Links.TryGetValue(key, out var collection))
            {
                collection = new List<ILink>();
                resource.Links[key] = collection;
            }

            return collection;
        }
    }
}