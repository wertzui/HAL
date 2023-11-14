namespace HAL.Client.Net;

/// <summary>
/// A factory to get <see cref="IHalClient"/>s.
/// </summary>
public interface IHalClientFactory
{
    /// <summary>
    /// Gets a named client. The name corresponds to the name of the underlying <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <returns>A client with the given name.</returns>
    IHalClient GetClient(string name);

    /// <summary>
    /// Get an unnamed client. The underlying <see cref="HttpClient"/> has its name set to "HalClient".
    /// </summary>
    /// <returns>A client with name "HalClient".</returns>
    IHalClient GetClient();
}