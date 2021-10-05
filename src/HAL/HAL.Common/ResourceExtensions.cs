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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(href))
                throw new ArgumentException($"'{nameof(href)}' cannot be null or whitespace.", nameof(href));

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
        public static TResource AddEmbedded<TResource>(this TResource resource, string key, Resource embedded)
            where TResource : Resource
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));

            if (embedded is null)
                throw new ArgumentNullException(nameof(embedded));

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
        public static TResource AddEmbedded<TResource, TSource, TKey>(this TResource resource, IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Resource> embeddedSelector)
            where TResource : Resource
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector is null)
                throw new ArgumentNullException(nameof(keySelector));

            if (embeddedSelector is null)
                throw new ArgumentNullException(nameof(embeddedSelector));

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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (link is null)
                throw new ArgumentNullException(nameof(link));

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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrWhiteSpace(rel))
                throw new ArgumentException($"'{nameof(rel)}' cannot be null or whitespace.", nameof(rel));

            if (link is null)
                throw new ArgumentNullException(nameof(link));

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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (linkSelector is null)
                throw new ArgumentNullException(nameof(linkSelector));

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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (relSelector is null)
                throw new ArgumentNullException(nameof(relSelector));

            if (linkSelector is null)
                throw new ArgumentNullException(nameof(linkSelector));

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
        /// <param name="resource">The resource.</param>
        /// <param name="links">The links to be added to the resource. The Name of the link will become the rel.</param>
        /// <returns></returns>
        public static TResource AddLinks<TResource>(this TResource resource, IEnumerable<Link> links)
            where TResource : Resource
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (links is null)
                throw new ArgumentNullException(nameof(links));

            foreach (var link in links)
            {
                resource.AddLink(link);
            }

            return resource;
        }

        /// <summary>
        /// Adds multiple links from a collection.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TLinkCollection">The type of the link collection.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="source">The source collection containing the items that will be converted to links.</param>
        /// <returns></returns>
        public static TResource AddLinks<TResource, TKey, TLinkCollection>(this TResource resource, IEnumerable<KeyValuePair<TKey, TLinkCollection>> source)
            where TResource : Resource
            where TLinkCollection : IEnumerable<Link>
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrWhiteSpace(href))
                throw new ArgumentException($"'{nameof(href)}' cannot be null or whitespace.", nameof(href));

            var link = new Link { Name = Constants.SelfLinkName, Href = href };

            return resource.AddLink(link);
        }

        /// <summary>
        /// Casts the state to the new type.
        /// If the <paramref name="resource"/> is <see cref="Resource"/> then the state will be initialized with the default value.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="resource">The resource.</param>
        public static Resource<TState> CastState<TState>(this Resource resource)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            var type = resource.GetType();

            if (!type.IsGenericType)
                return resource.ChangeStateTo<TState>(default);

            return resource.ChangeStateTo((TState)type.GetProperty(nameof(Resource<object>.State)).GetValue(resource));
        }

        /// <summary>
        /// Creates a new resource with the given new state and preserves the Embedded and Links properties.
        /// </summary>
        /// <typeparam name="TState">The type of the new state.</typeparam>
        /// <param name="resource">The old resource.</param>
        /// <param name="state">The new state.</param>
        /// <returns></returns>
        public static Resource<TState> ChangeStateTo<TState>(this Resource resource, TState state)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            return new Resource<TState> { Embedded = resource.Embedded, Links = resource.Links, State = state };
        }

        /// <summary>
        /// Creates a new resource without the State.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public static Resource RemoveState(this Resource resource)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            return new() { Embedded = resource.Embedded, Links = resource.Links };
        }

        private static ICollection<Resource> GetOrCreateEmbeddedCollection(Resource resource, string key)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));

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
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));

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