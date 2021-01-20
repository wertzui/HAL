# HAL
This project aims to bring a simple to use implementation of the Hypertext Application language.

## Specification
 - The formal specification is published as in IETF draft and can be found under https://tools.ietf.org/html/draft-kelly-json-hal-08.
 - A more informal documentation can be found under http://stateless.co/hal_specification.html.

 ## Usage
 The project consists of two packages
 1. `HAL.Common` which contains the `IResource`, `IResource<T>` and `ILink` implementations and the converters needed for serialization with `System.Text.Json`.
 2. `HAL.AspNetCore` adds `IResourceFactory` and `ILinkFactory` which can be used in your controllers to easily generate resources from your models.

### In Startup.cs
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

 ### In your controller
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
    public ActionResult<IResource> Get()
    {
        var models = new[]
        {
            new MyModelListDto {Id = 1, Name = "Test1"},
            new MyModelListDto {Id = 2, Name = "Test2"},
        };

        var result = _resourceFactory.CreateForListEndpoint(models, m => m.Id);

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