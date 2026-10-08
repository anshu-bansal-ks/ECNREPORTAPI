using ECNREPORTAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECNREPORTAPI.Controllers
{
    [Route("api/accountssuppressedfromagingcollection")]
    [ApiController]
    [Authorize]
    public class AccountSuppressedFromAgingController : ControllerBase
    {
        private readonly AccountSuppressedFromAgingService _service;

        public AccountSuppressedFromAgingController(AccountSuppressedFromAgingService service)
        {
            _service = service;
        }

        [HttpPost("data")]
        public async Task<IActionResult> GetData([FromBody] AgingRequestDto req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CompId))
                return BadRequest("CompId is required.");

            var result = await _service.GetAsync(req.CompId);
            return Ok(result);
        }
    }

    public class AgingRequestDto
    {
        public string CompId { get; set; } = "";
    }
}