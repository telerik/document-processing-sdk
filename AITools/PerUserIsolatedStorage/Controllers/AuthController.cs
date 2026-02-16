using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Telerik.Documents.AI.AgentTools.Examples;

/// <summary>
/// Helper controller to generate JWT tokens for testing.
/// In production, use proper authentication (OAuth, Identity, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generate a JWT token for testing purposes.
    /// POST /api/auth/token with { "userId": "user123", "username": "testuser" }
    /// </summary>
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return BadRequest(new { error = "UserId is required" });
        }

        var key = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyForTestingPurposes12345!";
        var issuer = _configuration["Jwt:Issuer"] ?? "PerUserIsolatedStorage";
        var audience = _configuration["Jwt:Audience"] ?? "PerUserIsolatedStorage";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, request.UserId),
            new Claim(ClaimTypes.Name, request.Username ?? request.UserId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            expiresAt = token.ValidTo,
            userId = request.UserId
        });
    }
}

/// <summary>
/// Request model for token generation.
/// </summary>
public class TokenRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? Username { get; set; }
}
