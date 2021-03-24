using System;
using System.Collections.Generic;

namespace HAL.Common
{
    /// <summary>
    /// Contains extension methods for <see cref="Resource"/>
    /// </summary>
    public static class ResourceExtensions
    {
        /// <summary>
        /// Adds a curie link.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="name">The name of the curie.</param>
        /// <param name="href">The URL to the documentation.</param>
        /// <returns></returns>
        public static TResource AddCurieLink<TResource>(this TResource resource, string name, string href)
            where TResource : Resource
        {
            if (!href.Contains("{rel}"))
                throw new ArgumentException("A curie must contain a {rel} template.");

            var link = new Link { Name = name, Href = href, Templated = true };

            return resource.AddLink(Constants.CuriesLinkRel, link);
        }

        /// <summary>
        /// Adds an embedded resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="key">The key of the embedded resource.</param>
        /// <param name="embedded">The embedded resource to add.</param>
        /// <returns></returns>
        public static TResource AddEmbedded<TResource>(this TResource resource, string key, TResource embedded)
            where TResource : Resource
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
            where TResource : Resource
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
        /// Adds a link and uses its name as rel.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="link">The link to add.</param>
        /// <returns></returns>
        public static TResource AddLink<TResource>(this TResource resource, Link link)
            where TResource : Resource
        {
            var key = link.Name;

            return resource.AddLink(key, link);
        }

        /// <summary>
        /// Adds a link with the specified key.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="rel">The key to add the link to.</param>
        /// <param name="link">The link to add.</param>
        /// <returns></returns>
        public static TResource AddLink<TResource>(this TResource resource, string rel, Link link)
            where TResource : Resource
        {
            var collection = GetOrCreateLinkCollection(resource, rel);

            collection.Add(link);

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
        public static TResource AddLink<TResource, TSource>(this TResource resource, IEnumerable<TSource> source, Func<TSource, Link> linkSelector)
            where TResource : Resource
        {
            foreach (var item in source)
            {
                var value = linkSelector(item);
                resource.AddLink(value);
            }

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
        /// <param name="relSelector">A function to select the key of link.</param>
        /// <param name="linkSelector">A function to convert each item of the source collection into a link.</param>
        /// <returns></returns>
        public static TResource AddLinks<TResource, TSource, TKey>(this TResource resource, IEnumerable<TSource> source, Func<TSource, TKey> relSelector, Func<TSource, Link> linkSelector)
            where TResource : Resource
        {
            foreach (var item in source)
            {
                var key = relSelector(item)?.ToString();
                var value = linkSelector(item);
                resource.AddLink(key, value);
            }

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
        public static TResource AddLinks<TResource, TKey, TLinkCollection>(this TResource resource, IEnumerable<KeyValuePair<TKey, TLinkCollection>> source)
            where TResource : Resource
            where TLinkCollection : IEnumerable<Link>
        {
            foreach (var rel in source)
            {
                foreach (var link in rel.Value)
                {
                    resource.AddLink(rel.Key?.ToString(), link);
                }
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
            where TResource : Resource
        {
            var link = new Link { Name = Constants.SelfLinkName, Href = href };

            return resource.AddLink(link);
        }

        private static ICollection<Resource> GetOrCreateEmbeddedCollection(Resource resource, string key)
        {
            if (resource.Embedded is null)
                resource.Embedded = new Dictionary<string, ICollection<Resource>>();

            if (!resource.Embedded.TryGetValue(key, out var collection))
            {
                collection = new List<Resource>();
                resource.Embedded[key] = collection;
            }

            return collection;
        }

        private static ICollection<Link> GetOrCreateLinkCollection(Resource resource, string key)
        {
            if (resource.Links is null)
                resource.Links = new Dictionary<string, ICollection<Link>>();

            if (!resource.Links.TryGetValue(key, out var collection))
            {
                collection = new List<Link>();
                resource.Links[key] = collection;
            }

            return collection;
        }
    }
}