using Microsoft.AspNetCore.Mvc;

namespace DockerAutotestesEcosystemSwagger.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Root()
        {
            return Ok(new
            {
                message = "Service API",
                version = "1.0.0",
                docs = "/swagger",
                endpoints = new
                {
                    login = "/api/auth/login",
                    logout = "/api/auth/logout",
                    check_session = "/api/auth/check-session"
                }
            });
        }
    }
}