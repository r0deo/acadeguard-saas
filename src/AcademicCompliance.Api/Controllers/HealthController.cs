using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "OK" });
    }
}
