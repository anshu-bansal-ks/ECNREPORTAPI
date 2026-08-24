using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPOTINGPORTAL.Models;
using Microsoft.AspNetCore.Authorization;   // ← YE ADD KARNA ZAROORI THA!

[ApiController]
[Route("[controller]")]
[Authorize]
public class ReportIndexController : ControllerBase
{
    private readonly ReportIndexService _service;

    public ReportIndexController(ReportIndexService service)
    {
        _service = service;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get([FromRoute] string userId, [FromQuery] string? keyword = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "UserId is required" });

        if (!int.TryParse(userId, out int parsedUserId))
            return BadRequest(new { message = "UserId must be a valid number" });

        if (parsedUserId <= 0)
            return BadRequest(new { message = "Invalid UserId" });

        var data = await _service.GetUserReportsAsync(parsedUserId, keyword);

        // Fixed: ?? operator with proper type
        return Ok(data ?? new List<ReportIndex>());
    }
}