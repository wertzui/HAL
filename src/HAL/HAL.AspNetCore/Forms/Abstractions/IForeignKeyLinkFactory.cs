using HAL.Common.Forms;
using System;

namespace HAL.AspNetCore.Forms.Abstractions
{
    /// <summary>
    /// This interface describes a factory which can provide a link for a given type.
    /// </summary>
    public interface IForeignKeyLinkFactory
    {
        /// <summary>
        /// Whether this factory can create a link for the given type.
        /// </summary>
        /// <param name="listDtoType">The type to create a link to.</param>
        /// <returns><c>true</c> if the link can be generated; <c>false</c> otherwise.</returns>
        public bool CanCreateLink(Type listDtoType);

        /// <summary>
        /// Creates the link to endpoint for the given type.
        /// </summary>
        /// <param name="listDtoType">The type to create a link to.</param>
        /// <returns>The link to the list endpoint of the given type.</returns>
        public OptionsLink? CreateLink(Type listDtoType);
    }
}