
//Controllers/AdminAuthController

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ECNREPORTAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class AdminLoginController : ControllerBase
{
    private readonly DashboardContext _context;
    private readonly IConfiguration _configuration;

    public AdminLoginController(DashboardContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest req) // ✅ Name changed to AdminLoginRequest
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username and Password are required" });

        try
        {
            
            var user = await _context.UserUnifiedPortals
                .FirstOrDefaultAsync(u => u.Username.Trim().ToLower() == req.Username.Trim().ToLower()); 
                                          
                if (user == null) 
                    return Unauthorized("User not found");

                // Explicit 0/1 check if they are integers or bits
                bool isActive = user.IsActive == true || (object)user.IsActive is int i && i == 1;
                bool isAdmin = user.IsAdmin == true || (object)user.IsAdmin is int j && j == 1;

                if (!isActive || !isAdmin)
                return Unauthorized(new { message = "Unauthorized: Admin access only or account inactive" });

            // 2. Active Directory Authentication
            string domain = _configuration["ActiveDirectory:Domain"]!;
            string ldapPath = _configuration["ActiveDirectory:Path"]!;
            
            if (!AuthenticateWithAD(domain, req.Username.Trim(), req.Password, ldapPath))
            {
                return Unauthorized(new { message = "Invalid credentials - AD authentication failed" });
            }

            // 3. Generate Token
            var token = GenerateJwtToken(user);

            // 4. Update Last Login
            user.LastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                token = token,
                userId = user.UserId,
                username = user.Username,
                name = user.Name ?? user.Username,
                role = "Admin"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
        }
    }

    // ✅ Suppression for Windows-only DirectoryServices
#pragma warning disable CA1416
    private bool AuthenticateWithAD(string domain, string username, string password, string ldapPath)
    {
        try
        {
            using (var entry = new DirectoryEntry(ldapPath, $@"{domain}\{username}", password))
            {
                object native = entry.NativeObject;
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
#pragma warning restore CA1416

    private string GenerateJwtToken(UserUnifiedPortal user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Jwt Key is missing in config");

        var key = Encoding.UTF8.GetBytes(jwtKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("IsAdmin", "true")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var createdToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(createdToken);
    }
}

// ✅ Name change kiya taaki AuthController wali class se clash na ho
public class AdminLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}