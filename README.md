# HAL
This project aims to bring a simple to use implementation of the Hypertext Application language.

## Specification
 - The formal specification is published as in IETF draft and can be found under https://tools.ietf.org/html/draft-kelly-json-hal-08.
 - A more informal documentation can be found under http://stateless.co/hal_specification.html.

 ## Usage
 The project consists of three packages
 1. `HAL.Common` which contains the `IResource`, `IResource<T>` and `ILink` implementations and the converters needed for serialization with `System.Text.Json`.
 2. `HAL.AspNetCore` adds `IResourceFactory` and `ILinkFactory` which can be used in your controllers to easily generate resources from your models.
 3. `HAL.AspNetCore.OData` adds `IODataResourceFactory` which can be used in your controllers to easily generate list endoints with paging from OData $skip and $top syntax.

### Without OData support
#### In Startup.cs
 ```
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddControllers() // or .AddMvc()
        .AddHal()
        .AddJsonOptions(o => // not neccessary, but creates a much nicer output
        {
            o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; 
        });
}
 ```

 #### In your controller
 ```
[Route("[controller]")]
[ApiController]
public class MyController : ControllerBase
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
#### In Startup.cs
 ```
public void ConfigureServices(IServiceCollection services)
{
    services.AddOData();

    var modelBuilder = new ODataConventionModelBuilder();
    modelBuilder.EntitySet<MyModelListDto>(typeof(MyModelListDto).Name);
    services.AddSingleton(_ => modelBuilder.GetEdmModel());

    services
        .AddControllers() // or .AddMvc()
        .AddHALOData()
        .AddJsonOptions(o => // not neccessary, but creates a much nicer output
        {
            o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; 
        });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.EnableDependencyInjection();  // Required for OData to work.
    });

    // ...
}
 ```

 #### In your controller
  ```
[Route("[controller]")]
[ApiController]
public class MyController : ControllerBase
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
            [SwaggerIgnore] ODataQueryOptions<TEntity> options,
            [FromQuery(Name = "$filter")] string? filter = default,
            [FromQuery(Name = "$orderby")] string? orderby = default,
            [FromQuery(Name = "$top")] long? top = default,
            [FromQuery(Name = "$skip")] long? skip = default)
    {
        var models = new[]
        {
            new MyModelListDto {Id = 1, Name = "Test1"},
            new MyModelListDto {Id = 2, Name = "Test2"},
        };

        // Apply the OData filtering
        models = options.Apply(models.AsQueryable()).Cast<MyModelListDto>().ToArray()

        var result = _resourceFactory.CreateForOdataListEndpointUsingSkipTopPaging(models, _ => "items", m => m.Id, options);

        return Ok(result);
    }

    // GET, PUT, POST, ...
}
 ```