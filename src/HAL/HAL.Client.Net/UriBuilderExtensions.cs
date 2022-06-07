using System.Collections;

namespace HAL.Client.Net
{
    /// <summary>
    /// Contains extension methods for <see cref="UriBuilder"/>.
    /// </summary>
    public static class UriBuilderExtensions
    {
        /// <summary>
        /// Sets the query to the given parameters overriding any existing query.
        /// </summary>
        /// <typeparam name="T">The type of the parameters.</typeparam>
        /// <param name="builder">The <see cref="UriBuilder"/> instance.</param>
        /// <param name="parameters">The parameters to set the query to.</param>
        /// <returns>The given <paramref name="builder"/>.</returns>
        public static UriBuilder SetQuery<T>(this UriBuilder builder, T parameters)
        {
            var query = CreateQuery(parameters);
            builder.Query = query;
            return builder;
        }

        /// <summary>
        /// Appends the given parameters to the existing query.
        /// </summary>
        /// <typeparam name="T">The type of the parameters.</typeparam>
        /// <param name="builder">The <see cref="UriBuilder"/> instance.</param>
        /// <param name="parameters">The parameters to append to the query.</param>
        /// <returns>The given <paramref name="builder"/>.</returns>
        public static UriBuilder AppendQuery<T>(this UriBuilder builder, T parameters)
        {
            var query = CreateQuery(parameters);
            if (!string.IsNullOrEmpty(query))
                builder.Query = string.Concat(builder.Query, "&", query);
            else
                builder.Query = query;
            return builder;
        }

        /// <summary>
        /// Creates a query string out of the given <paramref name="parameters"/>.
        /// The string is not prepended with a '?' sign.
        /// </summary>
        /// <typeparam name="T">The type of the parameters.</typeparam>
        /// <param name="parameters">The parameters to append to the query.</param>
        /// <returns>A query string from the given parameters.</returns>
        public static string CreateQuery<T>(T parameters)
        {
            var fragments = typeof(T).GetProperties()
                .Where(property => property.CanRead)
                .Select(property => new
                {
                    property.Name,
                    Value = property.GetMethod?.Invoke(parameters, null)
                })
                .Select(pair => new
                {
                    pair.Name,
                    List = (pair.Value is not string && pair.Value is IEnumerable list ? list.Cast<object>() : new[] { pair.Value })
                        .Select(element => element?.ToString())
                        .Where(element => !string.IsNullOrEmpty(element))
                })
                .Where(pair => pair.List.Any())
                .SelectMany(pair => pair.List.Where(value => value is not null).Select(value => Uri.EscapeDataString(pair.Name) + '=' + Uri.EscapeDataString(value!)));

            return string.Join("&", fragments);
        }
    }
}
