using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Processing login for email: {Email}", request.Email);
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT id, email, password, role FROM users WHERE email = @email";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("email", request.Email);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var storedPassword = reader.GetString(reader.GetOrdinal("password"));
                                if (request.Password == storedPassword)
                                {
                                    var email = reader.GetString(reader.GetOrdinal("email"));
                                    var role = reader.GetString(reader.GetOrdinal("role"));
                                    var token = GenerateJwtToken(email, role);
                                    _logger.LogInformation("JWT generated for user: {Email}, Role: {Role}", email, role);
                                    return Ok(new { Token = token });
                                }
                            }
                        }
                    }
                }
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return Unauthorized("Invalid credentials");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateJwtToken(string email, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogDebug("Generated JWT: {Token}", tokenString);
            return tokenString;
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}