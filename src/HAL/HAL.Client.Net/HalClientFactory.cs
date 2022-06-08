namespace HAL.Client.Net
{
    /// <summary>
    /// A factory to get <see cref="IHalClient"/>s.
    /// </summary>
    public class HalClientFactory : IHalClientFactory
    {
        private readonly IHttpClientFactory _clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HalClientFactory"/> class.
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HalClientFactory(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        /// <inheritdoc/>
        public IHalClient GetClient(string name) => new HalClient(_clientFactory, name);

        /// <inheritdoc/>
        public IHalClient GetClient() => new HalClient(_clientFactory);
    }
}
