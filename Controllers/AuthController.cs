using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECNREPORTAPI.Models;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DashboardContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(DashboardContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and Password are required");

        var user = await _context.UserUnifiedPortals
            .FirstOrDefaultAsync(u => u.Username.Trim().ToLower() == req.Username.Trim().ToLower() && u.IsActive);

        if (user == null)
            return Unauthorized("User not found or inactive in system");

        string domain = _configuration["ActiveDirectory:Domain"]!;
        string ldapPath = _configuration["ActiveDirectory:Path"]!;

        if (!AuthenticateWithActiveDirectory(domain, req.Username.Trim(), req.Password, ldapPath))
            return Unauthorized("Invalid credentials - Active Directory authentication failed");

        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.Name ?? user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),

            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = jwt,
            username = user.Username,
            email = user.Email,
            userId = user.UserId,
            name =user.Name,
            message = "Login Successful via Active Directory!"
        });
    }

#pragma warning disable CA1416
    private bool AuthenticateWithActiveDirectory(string domain, string username, string password, string ldapPath)
    {
        try
        {
            var entry = new DirectoryEntry(ldapPath, $@"{domain}\{username}", password);
            var native = entry.NativeObject; 
            return true;
        }
        catch
        {
            return false;
        }
    }
#pragma warning restore CA1416
}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}