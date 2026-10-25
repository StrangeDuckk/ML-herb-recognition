using HerbRecognition_APIs.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HerbRecognition_APIs.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration config, ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost("login")]
        [EnableRateLimiting("login")]          // max 5 prób na 5 minut z jednego adresu
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            var username = config["AdminAuth:Username"];
            var password = config["AdminAuth:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return StatusCode(500, "Logowanie nie jest skonfigurowane na serwerze.");

            // oba porównania zawsze się wykonują (& zamiast &&) – czas odpowiedzi
            // nie zdradza, czy login był poprawny
            var usernameOk = SecureEquals(dto.Username, username);
            var passwordOk = SecureEquals(dto.Password, password);

            if (!(usernameOk & passwordOk))
            {
                logger.LogWarning("Nieudane logowanie do panelu admina z adresu {Ip}",
                    HttpContext.Connection.RemoteIpAddress);

                return Unauthorized("Nieprawidłowy login lub hasło.");
            }

            var jwt = config.GetSection("Jwt");
            var expiresAt = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

        // Porównanie w stałym czasie. Najpierw skrót SHA-256, żeby oba ciągi miały
        // tę samą długość – inaczej FixedTimeEquals kończy od razu i zdradza długość hasła.
        private static bool SecureEquals(string? provided, string expected) =>
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(provided ?? "")),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
    }
}