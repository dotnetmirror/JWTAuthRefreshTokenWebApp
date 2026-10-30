using DNM.JWTAuthRefreshTokenWebApp.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JWTAuthRefreshTokenWebApp.Models;

namespace DNM.JWTAuthRefreshTokenWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var refreshToken = GenerateRefreshToken();

            // Demo only: Hardcoded credential validation
            if (request.Username == "admin" && request.Password == "password123")
            {
                var token = GenerateJwtToken(request.Username, "Admin");
              

                // TODO: Save 'refreshToken.Token' to the database against the user record here

                SetRefreshTokenCookie(refreshToken);

                return Ok(new { Token = token });
            }
            if (request.Username == "user" && request.Password == "password123")
            {
                var token = GenerateJwtToken(request.Username, "User");

                SetRefreshTokenCookie(refreshToken);

                return Ok(new { Token = token });
            }

            return Unauthorized(new { Message = "Invalid credentials" });
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(60), // Expire in 60 seconds for testing
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { Message = "Refresh token is missing." });
            }

            // TODO: Look up the user in the database using this refreshToken string
            // TODO: Verify the token exists and refreshToken.Expires > DateTime.UtcNow

            // If valid, generate a new access token and a new refresh token (Rotation)
            // Example assuming user is 'admin':
            var newAccessToken = GenerateJwtToken("admin", "Admin");
            var newRefreshToken = GenerateRefreshToken();

            // TODO: Update the database to invalidate the old refresh token and save the new one

            SetRefreshTokenCookie(newRefreshToken);
            return Ok(new { Token = newAccessToken });
        }


        private RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7) // Long-lived token (e.g., 7 days)
            };
        }

        private void SetRefreshTokenCookie(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Ensures cookie is only sent over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expires
            };

            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
    }
}

