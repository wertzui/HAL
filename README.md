# HAL

This project aims to bring a simple to use implementation of the Hypertext Application language.

## Specification

- The formal specification is published as in IETF draft and can be found under <https://tools.ietf.org/html/draft-kelly-json-hal-08>.
- A more informal documentation can be found under <http://stateless.co/hal_specification.html>.
- The specification of HAL-Forms can be found here: <https://rwcbook.github.io/hal-forms/>

## Usage

 The project consists of multiple packages

 1. `HAL.Common` which contains the `Resource`, `Resource<T>`, `FormsResource`, `FormsResource<T>` and `Link` implementations and the converters needed for serialization with `System.Text.Json`.
 2. `HAL.AspNetCore` adds `IResourceFactory`, `IFormFactory` and `ILinkFactory` which can be used in your controllers to easily generate resources from your models. It also comes with a `HalControllerBase` class which can be used for all Controllers which return HAL.
 3. `HAL.AspNetCore.OData` adds `IODataResourceFactory` and `IODataFormFactory` which can be used in your controllers to easily generate list endoints with paging from OData $filter, $skip and $top syntax.  
 4. `Hal.Client.Net` is a client library to consume HAL APIs in .Net applications. When using it, you should call `app.Services.AddHalClientFactoy()` to inject the `IHalClientFactory` which can then be resolved in your application.
 5. `Hal.Client.Angular`/`@wertzui/ngx-hal-client` is a client library to consume HAL APIs in Angular applications. It exposes the `HalClientModule` which then provides the `HalClient` and a `FormService`.

### Without OData support

#### In Program.cs

```csharp
builder.Services
    .AddControllers() // or .AddMvc()
    .AddHal()
    .AddJsonOptions(o => // not neccessary, but creates a much nicer output
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; 
    });
```

#### In your controller

```csharp
[Route("[controller]")]
public class MyController : HalControllerBase
{
    private readonly IResourceFactory _resourceFactory;

    public Table(IResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
    }

    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public ActionResult<IResource> GetList()
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
    public ActionResult<IResource<ModelFullDto>> Get(int id)
    {
        var model = new ModelFullDto { Id = id, Name = $"Test{id}", Description = "Very important!" };

        var result = _resourceFactory.CreateForGetEndpoint(model);

        return Ok(result);
    }

    // PUT, POST, ...
}
```

### With OData support

#### In Program.cs with OData support

```csharp
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<MyModelListDto>(typeof(MyModelListDto).Name);
builder.Services.AddSingleton(_ => modelBuilder.GetEdmModel());

builder.Services
    .AddControllers(options => // or .AddMvc()
    {
        options.OutputFormatters.RemoveType<ODataOutputFormatter>();
        options.InputFormatters.RemoveType<ODataInputFormatter>();
    })
    .AddOData()
    .AddHALOData()
    .AddJsonOptions(o => // not neccessary, but creates a much nicer output
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; 
    });
}

var app = builder.Build();

app.UseRouting();

// ...

app.UseEndpoints(_ => { });
app.MapControllers();
}
```

#### In your controller with OData support

```csharp
[Route("[controller]")]
public class MyController : HalControllerBase
{
    private readonly IODataResourceFactory _resourceFactory;

    public Table(IODataResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
    }

    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public ActionResult<Resource> GetList(
            // The SwaggerIgnore attribute and all parameters beside the options are just here to give you a nice swagger experience.
            // If you do not need that, you can remove everything except the options parameter.
            // If you are using RESTworld, you can also remove everything except the options parameter, because there is a custom Swagger filter for that.
            [SwaggerIgnore] ODataQueryOptions<TEntity> options,
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
        models = options.Apply(models.AsQueryable()).Cast<MyModelListDto>().ToArray()

        var result = _resourceFactory.CreateForOdataListEndpointUsingSkipTopPaging(models, _ => "items", m => m.Id, options);

        return Ok(result);
    }

    // GET, PUT, POST, ...
}
```
