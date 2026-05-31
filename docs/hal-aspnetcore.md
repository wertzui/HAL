# HAL.AspNetCore

The server-side ASP.NET Core library for building HAL (Hypertext Application Language) APIs.

## Installation

```sh
dotnet add package HAL.AspNetCore
```

## Setup

Call `AddHAL()` on your `IMvcBuilder` in `Program.cs`. It registers all required services and configures JSON serialization automatically:

```csharp
builder.Services
    .AddControllers()          // or .AddMvc()
    .AddHAL();
```

`AddHAL()` configures:

- Custom `System.Text.Json` converters for `Resource`, `Resource<T>`, `FormsResource`, and `FormsResource<T>`
- `DefaultIgnoreCondition = WhenWritingDefault` for clean HAL output
- `IResourceFactory`, `IFormFactory`, `ILinkFactory`, and related services

You only need to call `AddJsonOptions` afterwards if you want to customize settings beyond the defaults.

### Content negotiation middleware (optional)

If you want the server to inspect and track `Accept` header preferences per request, add the middleware:

```csharp
// Program.cs
app.UseAcceptHeaders(); // from HAL.AspNetCore.ContentNegotiation
app.MapControllers();
```

This adds an `IAcceptHeaderFeature` to each request that you can resolve from `HttpContext.Features`.

## `HalControllerBase`

Derive your HAL-returning controllers from `HalControllerBase` instead of `ControllerBase`. It sets `[ApiController]` and `[Produces("application/hal+json")]` for you:

```csharp
[Route("[controller]")]
public class ItemsController : HalControllerBase
{
    // ...
}
```

## `IResourceFactory`

The primary factory for creating HAL resources. Inject it into your controller.

### Single resource – `CreateForEndpoint`

Use this in GET-by-id endpoints. It automatically adds a `self` link pointing to the current action:

```csharp
[HttpGet("{id}")]
public ActionResult<Resource<ItemDto>> Get(int id)
{
    var item = new ItemDto { Id = id, Name = "Widget" };
    var result = _resourceFactory.CreateForEndpoint(item);
    return Ok(result);
}
```

The `action`, `controller`, and `routeValues` parameters are optional. By default the current action name is used.

### List resource – `CreateForListEndpoint`

Use this in GET-all endpoints. Each item in the collection is embedded under the key returned by `keyAccessor`, and each embedded item automatically receives a link to its individual GET endpoint:

```csharp
[HttpGet]
public ActionResult<Resource> GetList()
{
    var items = new[]
    {
        new ItemListDto { Id = 1, Name = "Widget" },        new ItemListDto { Id = 1, Name = "Widget" },
        new ItemListDto { Id = 2, Name = "Gadget" },        new ItemListDto { Id = 2, Name = "Gadget" },
    };

    // keyAccessor: name of the embedded relation (e.g. "items")
    // idAccessor: value used to construct each item's self link
    var result = _resourceFactory.CreateForListEndpoint(
        items,        items,
        _ => "items",        _ => "items",
        item => item.Id);        item => item.Id);

    return Ok(result);
}
```

### List resource with paging – `CreateForListEndpointWithPaging`

Returns a `Resource<Page>` that includes total-count and page-size metadata alongside the embedded items:

```csharp
[HttpGet]
public ActionResult<Resource<Page>> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
{
    var allItems = GetAllItemsFromDb();
    var pageItems = allItems
        .Skip((page - 1) * pageSize)        .Skip((page - 1) * pageSize)
        .Take(pageSize)        .Take(pageSize)
        .ToArray();        .ToArray();

    var pageState = new Page
    {
        CurrentPage = page,        CurrentPage = page,
        PageSize = pageSize,        PageSize = pageSize,
        TotalItems = allItems.Count        TotalItems = allItems.Count
    };

    var result = _resourceFactory.CreateForListEndpointWithPaging(
        pageItems,        pageItems,
        _ => "items",        _ => "items",
        item => item.Id,        item => item.Id,
        state: pageState);        state: pageState);

    return Ok(result);
}
```

### Home endpoint – `CreateForHomeEndpoint` / `CreateForHomeEndpointWithSwaggerUi`

These methods auto-discover all controller actions and generate a HAL resource with links to every entry point in your API. Use it on a root/home endpoint so clients can navigate the entire API from one starting URL:

```csharp
[HttpGet]
public ActionResult<Resource> Get()
{
    // Links to all parameterless actions; uses a custom curie template for documentation
    var result = _resourceFactory.CreateForHomeEndpoint(
        curieName: "acme",        curieName: "acme",
        curieUrlTemplate: "https://docs.acme.com/rels/{rel}");        curieUrlTemplate: "https://docs.acme.com/rels/{rel}");

    return Ok(result);
}
```

If you use Swagger UI, point the curie at it instead:

```csharp
// Requires deep linking enabled in app.UseSwaggerUI(c => c.EnableDeepLinking())
var result = _resourceFactory.CreateForHomeEndpointWithSwaggerUi(curieName: "acme");
```

Both overloads accept an optional `TState` generic parameter if you want to embed application-level metadata alongside the links.

## `ILinkFactory`

Use `ILinkFactory` when you need fine-grained control over individual links rather than having the resource factory add them automatically.

### Creating links

```csharp
// Link to a specific action in the current controller (resolved via route generation)
Link editLink = _linkFactory.Create(action: "Update", values: new { id = 42 });

// Link to an absolute URL
Link docsLink = _linkFactory.Create("https://docs.example.com");

// Named link (name appears in the HAL _links object)
Link namedLink = _linkFactory.Create(name: "edit", action: "Update", values: new { id = 42 });

// Named + titled link
Link titledLink = _linkFactory.Create(name: "edit", title: "Edit this item", action: "Update", values: new { id = 42 });

// Templated link (all parameters become URI-template variables)
ICollection<Link> templated = _linkFactory.CreateTemplated(action: "Get", controller: "Items");
```

`TryCreate` variants return `false` instead of throwing when route generation fails.

### Adding links to a resource

```csharp
// Add the self link (points to the current action)
resource = _linkFactory.AddSelfLinkTo(resource);

// Add a Swagger UI curie for documenting your rels
resource = _linkFactory.AddSwaggerUiCurieLinkTo(resource, name: "acme");

// Add a HAL-Forms link alongside an existing link
resource = _linkFactory.AddFormLinkForExistingLinkTo(resource, existingRel: "self");
```

### Generating all API links

```csharp
// Every action (with URI-template parameters where applicable)
IDictionary<string, ICollection<Link>> allLinks = _linkFactory.CreateAllLinks(prefix: "acme");

// Only parameterless actions
ICollection<Link> rootLinks = _linkFactory.CreateAllLinksWithoutParameters();
```

## `IFormFactory` (HAL-Forms)

`IFormFactory` generates HAL-Forms `FormTemplate` objects from your C# model types. Inject it alongside `IResourceFactory` when you want to expose forms metadata.

### Creating a form template

```csharp
// Returns a FormTemplate with properties reflected from ItemDto,
// pre-filled with the values from `item`.
FormTemplate template = await _formFactory.CreateFormAsync(
    value: item,
    target: Url.Action("Update", "Items", new { id = item.Id })!,
    method: HttpMethod.Put,
    title: "Edit item");
```

### Creating a complete forms resource

```csharp
// Shortcut that wraps the template in a FormsResource with a "default" template
FormsResource formsResource = await _formFactory.CreateResourceForEndpointAsync(
    value: item,
    method: HttpMethod.Put,
    title: "Edit item",
    action: "Get",       // action whose route becomes the submit target
    controller: "Items",
    routeValues: new { id = item.Id });

return Ok(formsResource);
```

You can build a resource manually for more control:

```csharp
FormTemplate createTemplate = await _formFactory.CreateFormAsync(
    value: new CreateItemDto(),
    target: Url.Action("Create", "Items")!,
    method: HttpMethod.Post,
    title: "Create item");

FormsResource resource = _formFactory.CreateResource(createTemplate);
_linkFactory.AddSelfLinkTo(resource);

return Ok(resource);
```

## HAL-Forms customization

For a full attribute reference and a deep-dive into how HAL-Forms templates are generated from C# types, see [HAL-Forms Generation](hal-forms-generation.md).

You can hook into the form-generation pipeline

Register customizations in `Program.cs` using the host-builder extension methods:

```csharp
// Customize the FormsResource as a whole
builder.AddFormsResourceGenerationCustomization<MyFormsCustomization>();

// Customize how each Property template is generated
builder.AddPropertyTemplateGenerationCustomization<MyPropertyTemplateCustomization>();

// Customize the runtime value of each Property
builder.AddPropertyValueGenerationCustomization<MyPropertyValueCustomization>();
```

Implement the corresponding interface to add your logic:

```csharp
// Example: mark all properties named "InternalCode" as hidden
public class HideInternalCodeCustomization : IPropertyTemplateGenerationCustomization
{
    public void Customize(Property property, PropertyInfo propertyInfo, Type resourceType)
    {
        if (property.Name == "InternalCode")        if (property.Name == "InternalCode")
            property.Type = PropertyType.Hidden;            property.Type = PropertyType.Hidden;            property.Type = PropertyType.Hidden;
    }
}
```

Multiple customizations of the same type are applied in registration order.

## Full controller example

```csharp
[Route("[controller]")]
public class ItemsController : HalControllerBase
{
    private readonly IResourceFactory _resourceFactory;
    private readonly IFormFactory _formFactory;

    public ItemsController(IResourceFactory resourceFactory, IFormFactory formFactory)
    {
        _resourceFactory = resourceFactory;        _resourceFactory = resourceFactory;
        _formFactory = formFactory;        _formFactory = formFactory;
    }

    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public ActionResult<Resource> GetList()
    {
        var items = new[]        var items = new[]
        {        {
            new ItemListDto { Id = 1, Name = "Widget" },            new ItemListDto { Id = 1, Name = "Widget" },            new ItemListDto { Id = 1, Name = "Widget" },
            new ItemListDto { Id = 2, Name = "Gadget" },            new ItemListDto { Id = 2, Name = "Gadget" },            new ItemListDto { Id = 2, Name = "Gadget" },
        };        };
        return Ok(_resourceFactory.CreateForListEndpoint(items, _ => "items", i => i.Id));        return Ok(_resourceFactory.CreateForListEndpoint(items, _ => "items", i => i.Id));
    }

    [HttpGet("{id}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public ActionResult<Resource<ItemDto>> Get(int id)
    {
        var item = new ItemDto { Id = id, Name = "Widget", Description = "A fine widget." };        var item = new ItemDto { Id = id, Name = "Widget", Description = "A fine widget." };
        return Ok(_resourceFactory.CreateForEndpoint(item));        return Ok(_resourceFactory.CreateForEndpoint(item));
    }

    [HttpGet("{id}/form")]
    public async Task<ActionResult<FormsResource>> GetForm(int id)
    {
        var item = new ItemDto { Id = id, Name = "Widget", Description = "A fine widget." };        var item = new ItemDto { Id = id, Name = "Widget", Description = "A fine widget." };
        var form = await _formFactory.CreateResourceForEndpointAsync(        var form = await _formFactory.CreateResourceForEndpointAsync(
            item, HttpMethod.Put, "Edit item", routeValues: new { id });            item, HttpMethod.Put, "Edit item", routeValues: new { id });            item, HttpMethod.Put, "Edit item", routeValues: new { id });
        return Ok(form);        return Ok(form);
    }

    [HttpPut("{id}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
    public ActionResult<Resource<ItemDto>> Update(int id, [FromBody] ItemDto dto)
    {
        // ... update logic ...        // ... update logic ...
        return Ok(_resourceFactory.CreateForEndpoint(dto));        return Ok(_resourceFactory.CreateForEndpoint(dto));
    }
}
```

## OData support

For OData-powered endpoints see [`HAL.AspNetCore.OData`](../src/HAL/Hal.AspNetCore.OData) and the [OData usage section in README.md](../README.md#with-odata).
