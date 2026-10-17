using HerbRecognition_APIs.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration config): ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            var username = config["AdminAuth:Username"];
            var password = config["AdminAuth:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return StatusCode(500, "Logowaie nie jest skonfigorowane na serwerze.");

            if (!SecureEquals(dto.Username, username) || !SecureEquals(dto.Password, password))
                return Unauthorized("Nieprawidłowy login lub hasło.");

            var jwt = config.GetSection("Jwt");
            var expiresAt = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                },
                expires: expiresAt,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
                    SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt
            });
        }

        // porównanie w stałym czasie – utrudnia zgadywanie hasła po czasie odpowiedzi
        private static bool SecureEquals(string? a, string b) =>
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(a ?? ""), Encoding.UTF8.GetBytes(b));
    }
}
