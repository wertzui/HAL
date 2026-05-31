# HAL

A .NET 10 library for implementing the Hypertext Application Language (HAL) and HAL-Forms in web APIs and clients.

## Purpose

HAL is a standard for building discoverable, self-describing REST APIs using hypermedia links. This library helps you:

- Expose resources with links and forms for easy client navigation
- Support both standard and OData-based endpoints
- Integrate HAL in .NET and Angular applications

**Use HAL if you want your API clients to discover available actions and related resources dynamically, following the HATEOAS principle.**

## Overview

This project provides:

- Core HAL resource and link models
- JSON serialization support
- ASP.NET Core integration for easy HAL responses
- OData support for list endpoints and paging
- .NET and Angular client libraries

## Specifications

- [IETF HAL Draft](https://tools.ietf.org/html/draft-kelly-json-hal-08)
- [Informal HAL Documentation](http://stateless.co/hal_specification.html)
- [HAL-Forms Specification](https://rwcbook.github.io/hal-forms/)

## Packages

 1. `HAL.Common` which contains the `Resource`, `Resource<T>`, `FormsResource`, `FormsResource<T>` and `Link` implementations and the converters needed for serialization with `System.Text.Json`.
 2. `HAL.AspNetCore` adds `IResourceFactory`, `IFormFactory` and `ILinkFactory` which can be used in your controllers to easily generate resources from your models. It also comes with a `HalControllerBase` class which can be used for all Controllers which return HAL.
 3. `HAL.AspNetCore.OData` adds `IODataResourceFactory` and `IODataFormFactory` which can be used in your controllers to easily generate list endpoints with paging from OData `$filter`, `$skip` and `$top` syntax.
 4. `HAL.Client.Net` is a client library to consume HAL APIs in .NET applications. Call `services.AddHalClient()` to register `IHalClient`, or `services.AddHalClientFactoy(clientNames)` to register `IHalClientFactory` for named clients. ([Full documentation](docs/hal-client-net.md))
 5. `HAL.Client.Angular` / `@wertzui/ngx-hal-client` is a client library to consume HAL APIs in Angular applications. Call `provideHalClient()` in your `app.config.ts` providers to register `HalClient` and `FormService`. ([Full documentation](docs/hal-client-angular.md))

## Installation

Add the desired package(s) via NuGet:

```sh
# Example for HAL.AspNetCore
dotnet add package HAL.AspNetCore
```

### Angular Client Installation

To use the Angular HAL client, install the npm package:

```sh
npm install @wertzui/ngx-hal-client
```

## Usage

### Without OData

Use this approach for standard REST endpoints.

**Configure services in `Program.cs` to enable HAL serialization:**

```csharp
builder.Services
    .AddControllers()
    .AddHAL();
```

> `AddHAL()` already configures all required JSON options (converters, `DefaultIgnoreCondition`, etc.).
> You only need to call `AddJsonOptions` afterwards if you want to customize settings beyond the defaults.

**Return HAL resources from your controller:**

```csharp
[Route("[controller]")]
public class MyController : HalControllerBase
{
    private readonly IResourceFactory _resourceFactory;

    public MyController(IResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
    }

    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public ActionResult<Resource> GetList()
    {
        var models = new[]
        {
            new MyModelListDto {Id = 1, Name = "Test1"},
            new MyModelListDto {Id = 2, Name = "Test2"},
        };

        var result = _resourceFactory.CreateForListEndpoint(models, _ => "items", m => m.Id);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<Resource<ModelFullDto>> Get(int id)
    {
        var model = new ModelFullDto { Id = id, Name = $"Test{id}", Description = "Very important!" };

        var result = _resourceFactory.CreateForEndpoint(model);

        return Ok(result);
    }

    // PUT, POST, ...
}
```

### With OData

Use this approach if your API supports OData queries for filtering, paging, and sorting.

**Configure services in `Program.cs` to enable OData and HAL:**

```csharp
builder.Services
    .AddControllers(options => // or .AddMvc()
    {
        options.OutputFormatters.RemoveType<ODataOutputFormatter>();
        options.InputFormatters.RemoveType<ODataInputFormatter>();
    })
    .AddOData()
    .AddHALOData();

var app = builder.Build();

app.UseRouting();

// ...

app.UseEndpoints(_ => { });
app.MapControllers();
```

**Return HAL resources with OData support from your controller:**

```csharp
[Route("[controller]")]
public class MyController : HalControllerBase
{
    private readonly IODataResourceFactory _resourceFactory;

    public MyController(IODataResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
    }

    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public ActionResult<Resource<Page>> GetList(
            // The SwaggerIgnore attribute and all parameters besides options are just here to give you a nice Swagger experience.
            // If you do not need that, you can remove everything except the options parameter.
            [SwaggerIgnore] ODataQueryOptions<MyModelListDto> options,
        [FromQuery(Name = "$filter")] string? filter = default,
        [FromQuery(Name = "$orderby")] string? orderby = default,
        [FromQuery(Name = "$top")] long? top = default,
        [FromQuery(Name = "$skip")] long? skip = default)
    {
        var models = new[]
        {
            new MyModelListDto { Id = 1, Name = "Test1" },
            new MyModelListDto { Id = 2, Name = "Test2" },
        };

        // Apply the OData filtering
        var filtered = options.ApplyTo(models.AsQueryable())
            .Cast<MyModelListDto>()
            .ToArray();

        var result = _resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(
            filtered, _ => "items", m => m.Id, options.RawValues);

        return Ok(result);
    }

    // GET, PUT, POST, ...
}
```

## Documentation

| Guide | Description |
|-------|-------------|
| [HAL.AspNetCore](docs/hal-aspnetcore.md) | Full reference for the server-side library: resource factory, form factory, link factory, home endpoint, paging, HAL-Forms customization, and content negotiation. |
| [HAL-Forms Generation](docs/hal-forms-generation.md) | Deep-dive into how HAL-Forms templates are generated from C# types: attribute reference, type mapping, and custom generation hooks. |
| [HAL.Client.Net](docs/hal-client-net.md) | Full reference for the .NET client library: registration, HTTP methods, link navigation, response handling, and HAL-Forms. |
| [HAL.Client.Angular](docs/hal-client-angular.md) | Full reference for the Angular client library: setup, `HalClient` API, `FormService`, `SignalFormService`, and date serialization. |

## When to use HAL

- You want your API to be self-describing and discoverable by clients
- You want to support hypermedia-driven navigation and forms
- You want to enable dynamic client behavior based on available links/actions

## Contributing

Contributions are welcome! Please open issues or submit pull requests.

## License

This project is licensed under the Unlicense.
