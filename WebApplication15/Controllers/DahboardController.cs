using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtAuthDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        // 1. Open to everyone (No token required)
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok("This endpoint is open to everyone.");
        }

        // 2. Requires a valid token (Authentication)
        [HttpGet("secure")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            var username = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Ok($"Hello {username}, your token was successfully verified.");
        }

        // 3. Requires a valid token AND the 'Admin' role claim (Authorization)
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminData()
        {
            return Ok("Welcome Admin! You have sufficient authorization privileges to view this.");
        }
    }
}