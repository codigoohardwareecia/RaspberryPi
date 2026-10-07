using Microsoft.AspNetCore.Mvc;

namespace JarvisWizard.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { msg = "Jarvis Vivo!" });
        }
    }
}
