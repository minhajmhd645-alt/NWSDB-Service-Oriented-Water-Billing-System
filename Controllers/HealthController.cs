using Microsoft.AspNetCore.Mvc;

namespace NWSDB.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
        }
    }
}
