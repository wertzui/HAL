using HAL.Common.Abstractions;
using System.Collections.Generic;

namespace HAL.AspNetCore.Abstractions
{
    /// <summary>
    /// A factory to create Links.
    /// </summary>
    public interface ILinkFactory
    {
        /// <summary>
        /// Adds the "self" link to the given resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        TResource AddSelfLinkTo<TResource>(TResource resource)
            where TResource : IResource;

        /// <summary>
        /// Creates a link to the specified action in the specified controller.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="values">The values.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="host">The host.</param>
        /// <param name="fragment">The fragment.</param>
        /// <returns></returns>
        ILink Create(string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null);

        /// <summary>
        /// Creates a link to the specified URL.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <returns></returns>
        ILink Create(string href);

        /// <summary>
        /// Creates a link to the specified action in the specified controller with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="values">The values.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="host">The host.</param>
        /// <param name="fragment">The fragment.</param>
        /// <returns></returns>
        ILink Create(string name, string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null);

        /// <summary>
        /// Creates a link to the specified URL with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="href">The href.</param>
        /// <returns></returns>
        ILink Create(string name, string href);

        /// <summary>
        /// Creates a link to the specified action in the specified controller with the given name and title.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="title">The title.</param>
        /// <param name="action">The action.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="values">The values.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="host">The host.</param>
        /// <param name="fragment">The fragment.</param>
        /// <returns></returns>
        ILink Create(string name, string title, string action = null, string controller = null, object values = null, string protocol = null, string host = null, string fragment = null);

        /// <summary>
        /// Creates a link to the specified URL with the given name and title.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="title">The title.</param>
        /// <param name="href">The href.</param>
        /// <returns></returns>
        ILink Create(string name, string title, string href);

        /// <summary>
        /// Creates all possible links to all controller actions that do not have any parameters in your application.
        /// </summary>
        /// <returns></returns>
        ICollection<ILink> CreateAllLinksWithoutParameters();
    }
}