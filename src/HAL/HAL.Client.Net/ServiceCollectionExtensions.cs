using Microsoft.Extensions.DependencyInjection;

namespace HAL.Client.Net
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a singleton <see cref="IHalClient"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="configureClient">An optional method to further configure the underlying <see cref="HttpClient"/>.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddHalClient(this IServiceCollection services, Action<IServiceProvider, HttpClient>? configureClient = null)
        {
            if (configureClient is not null)
                services.AddHttpClient<HalClient>(configureClient);
            else
                services.AddHttpClient<HalClient>();

            services.AddSingleton<IHalClient, HalClient>();

            return services;
        }

        /// <summary>
        /// Adds a singleton <see cref="IHalClientFactory"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientConfigurations">A dictionary containing the client names and optional configurations for them.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddHalClientFactoy(this IServiceCollection services, IDictionary<string, Action<IServiceProvider, HttpClient>?> clientConfigurations)
        {
            services.AddHalClient();

            services.AddSingleton<IHalClientFactory, HalClientFactory>();

            foreach (var config in clientConfigurations)
            {
                if (config.Value is not null)
                    services.AddHttpClient(config.Key, config.Value);
                else
                    services.AddHttpClient(config.Key);
            }

            return services;
        }

        /// <summary>
        /// Adds a singleton <see cref="IHalClientFactory"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add the client to.</param>
        /// <param name="clientNames">The names of the clients to add.</param>
        /// <param name="configureClients">An optional configuration for each client.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddHalClientFactoy(this IServiceCollection services, IEnumerable<string> clientNames, Action<IServiceProvider, HttpClient>? configureClients = null)
        {
            services.AddHalClient();

            services.AddSingleton<IHalClientFactory, HalClientFactory>();

            foreach (var name in clientNames)
            {
                if (configureClients is not null)
                    services.AddHttpClient(name, configureClients);
                else
                    services.AddHttpClient(name);
            }

            return services;
        }
    }
}
