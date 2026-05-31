# HAL.Client.Net

A .NET client library for consuming HAL (Hypertext Application Language) APIs.

## Installation

```sh
dotnet add package HAL.Client.Net
```

## Registration

### Single client (`IHalClient`)

Register a single, singleton `IHalClient` in `Program.cs`:

```csharp
builder.Services.AddHalClient();
```

You can optionally configure the underlying `HttpClient` (base address, headers, etc.):

```csharp
builder.Services.AddHalClient((serviceProvider, httpClient) =>
{
    httpClient.BaseAddress = new Uri("https://api.example.com/");
    httpClient.DefaultRequestHeaders.Add("X-Api-Key", "my-key");
});
```

### Multiple named clients (`IHalClientFactory`)

When your application talks to several HAL APIs, register named clients and resolve them via `IHalClientFactory`:

```csharp
// All clients share the same configuration
builder.Services.AddHalClientFactoy(
    ["orders-api", "products-api"],
    (serviceProvider, httpClient) =>
    {
        httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");        httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
    });

// Or configure each client individually
builder.Services.AddHalClientFactoy(new Dictionary<string, Action<IServiceProvider, HttpClient>?>
{
    ["orders-api"] = (sp, client) => client.BaseAddress = new Uri("https://orders.example.com/"),
    ["products-api"] = (sp, client) => client.BaseAddress = new Uri("https://products.example.com/"),
});
```

Resolve a named client at runtime:

```csharp
public class OrderService(IHalClientFactory factory)
{
    private readonly IHalClient _client = factory.GetClient("orders-api");
}
```

## Making requests

Every method on `IHalClient` returns a `HalResponse<TResource>`. Check `Succeeded` (or call `EnsureSuccessStatusCode()`) before accessing `Resource`.

### GET

```csharp
// Typed – deserializes the resource state into MyDto
HalResponse<Resource<MyDto>> response =
    await halClient.GetAsync<MyDto>(new Uri("https://api.example.com/items/1"));

response.EnsureSuccessStatusCode();
MyDto item = response.Resource.State;
```

```csharp
// Untyped – use when you only need links / embedded resources
HalResponse<Resource> response =
    await halClient.GetAsync(new Uri("https://api.example.com/items/1"));
```

### POST

```csharp
var newItem = new CreateItemDto { Name = "Widget" };

HalResponse<Resource<ItemDto>> response =
    await halClient.PostAsync<CreateItemDto, ItemDto>(
        new Uri("https://api.example.com/items"),        new Uri("https://api.example.com/items"),
        newItem);        newItem);

response.EnsureSuccessStatusCode();
ItemDto created = response.Resource.State;
```

### PUT

```csharp
var update = new UpdateItemDto { Name = "Updated Widget" };

HalResponse<Resource<ItemDto>> response =
    await halClient.PutAsync<UpdateItemDto, ItemDto>(
        new Uri("https://api.example.com/items/1"),        new Uri("https://api.example.com/items/1"),
        update);        update);

response.EnsureSuccessStatusCode();
```

### DELETE

```csharp
HalResponse<Resource> response =
    await halClient.DeleteAsync(new Uri("https://api.example.com/items/1"));

response.EnsureSuccessStatusCode();
```

### Optimistic concurrency (ETag)

Pass an ETag value to `DeleteAsync` or `PutAsync` to enable optimistic concurrency:

```csharp
HalResponse<Resource<ItemDto>> response =
    await halClient.PutAsync<UpdateItemDto, ItemDto>(
        new Uri("https://api.example.com/items/1"),        new Uri("https://api.example.com/items/1"),
        update,        update,
        eTag: "\"abc123\"");        eTag: "\"abc123\"");
```

### URI templates

If a link's `href` is a URI template, supply the parameters in the `uriParameters` dictionary:

```csharp
var parameters = new Dictionary<string, object> { ["id"] = 42 };

HalResponse<Resource<ItemDto>> response =
    await halClient.GetAsync<ItemDto>(
        new Uri("https://api.example.com/items/{id}"),        new Uri("https://api.example.com/items/{id}"),
        uriParameters: parameters);        uriParameters: parameters);
```

### API versioning

Append an API version to the `Accept` header:

```csharp
HalResponse<Resource<ItemDto>> response =
    await halClient.GetAsync<ItemDto>(
        new Uri("https://api.example.com/items/1"),        new Uri("https://api.example.com/items/1"),
        version: "2");        version: "2");
// Accept: application/hal+json; v=2
```

## Handling responses

`HalResponse<TResource>` always contains either a `Resource` (on success) or `ProblemDetails` (on failure). The `Succeeded` property (and the `[MemberNotNullWhen]` attributes) guide the compiler:

```csharp
var response = await halClient.GetAsync<ItemDto>(uri);

if (response.Succeeded)
{
    // response.Resource is guaranteed non-null here
    Console.WriteLine(response.Resource.State.Name);
}
else
{
    // response.ProblemDetails is guaranteed non-null here
    Console.WriteLine(response.ProblemDetails.Detail);
}
```

## Navigating links

Use `ResourceExtensions` to find links on a resource, then follow them with `LinkExtensions`:

```csharp
var listResponse = await halClient.GetAsync(new Uri("https://api.example.com/items"));
listResponse.EnsureSuccessStatusCode();

// Find the "next" page link
Link? nextLink = listResponse.Resource.FindLink("next");

if (nextLink is not null)
{
    var nextPage = await nextLink.FollowGetAsync<ItemDto>(halClient);
    nextPage.EnsureSuccessStatusCode();
}
```

Available helper methods on `Resource`:

| Method | Description |
|--------|-------------|
| `FindLinks(rel)` | Returns all links with the given relation. |
| `FindLink(rel, name?)` | Returns the first link, optionally filtered by name. |
| `TryFindLink(rel, out link, name?)` | Non-throwing variant of `FindLink`. |

Available follow helpers on `Link` (from `LinkExtensions`):

| Method | Description |
|--------|-------------|
| `FollowGetAsync<TState>(client)` | Sends GET and returns `Resource<TState>`. |
| `FollowPostAsync<TRequest, TState>(client, content)` | Sends POST. |
| `FollowPutAsync<TRequest, TState>(client, content)` | Sends PUT. |
| `FollowDeleteAsync<TState>(client, eTag?)` | Sends DELETE. |
| `FollowAsync<TRequest, TState>(client, method, content?)` | Sends an arbitrary HTTP method. |

## HAL-Forms support

Use the `*FormAsync` variants when the endpoint returns a `FormsResource`:

```csharp
HalResponse<FormsResource<ItemDto>> response =
    await halClient.GetFormAsync<ItemDto>(new Uri("https://api.example.com/items/1"));

response.EnsureSuccessStatusCode();
FormsResource<ItemDto> formsResource = response.Resource;
```

## Full example

```csharp
// Program.cs
builder.Services.AddHalClient(
    (_, client) => client.BaseAddress = new Uri("https://api.example.com/"));

// ItemService.cs
public class ItemService(IHalClient halClient)
{
    public async Task<IReadOnlyList<ItemDto>> GetAllAsync()
    {
        var response = await halClient.GetAsync(new Uri("/items", UriKind.Relative));        var response = await halClient.GetAsync(new Uri("/items", UriKind.Relative));
        response.EnsureSuccessStatusCode();        response.EnsureSuccessStatusCode();

        // Embedded items are stored under the "items" rel        // Embedded items are stored under the "items" rel
        var embedded = response.Resource.FindLinks("items");        var embedded = response.Resource.FindLinks("items");
        var results = new List<ItemDto>();        var results = new List<ItemDto>();

        foreach (var link in embedded)        foreach (var link in embedded)
        {        {
            var itemResponse = await halClient.GetAsync<ItemDto>(new Uri(link.Href));            var itemResponse = await halClient.GetAsync<ItemDto>(new Uri(link.Href));            var itemResponse = await halClient.GetAsync<ItemDto>(new Uri(link.Href));
            if (itemResponse.Succeeded)            if (itemResponse.Succeeded)            if (itemResponse.Succeeded)
                results.Add(itemResponse.Resource.State);                results.Add(itemResponse.Resource.State);                results.Add(itemResponse.Resource.State);                results.Add(itemResponse.Resource.State);
        }        }

        return results;        return results;
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto)
    {
        var response = await halClient.PostAsync<CreateItemDto, ItemDto>(        var response = await halClient.PostAsync<CreateItemDto, ItemDto>(
            new Uri("/items", UriKind.Relative), dto);            new Uri("/items", UriKind.Relative), dto);            new Uri("/items", UriKind.Relative), dto);
        response.EnsureSuccessStatusCode();        response.EnsureSuccessStatusCode();
        return response.Resource.State;        return response.Resource.State;
    }
}
```
