using Microsoft.AspNetCore.Mvc;

namespace HAL.AspNetCore.Controllers;

/// <summary>
/// A base class for a controller that serves HAL responses.
/// </summary>
[ApiController]
[Produces("application/hal+json")]
public abstract class HalControllerBase : ControllerBase
{
}