//Controllers/ItemDetailsController

using ECNREPORTAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECNREPORTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemDetailsController : ControllerBase
    {
        private readonly ItemDetailsService _service;

        public ItemDetailsController(ItemDetailsService service)
        {
            _service = service;
        }

        // ✅ SINGLE API 
         [HttpGet("data")]
        public async Task<IActionResult> ItemDetailsData(
            [FromQuery] string compId,
            [FromQuery] string itemIdList)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            if (string.IsNullOrWhiteSpace(itemIdList))
                return BadRequest("itemIdList required");

            var result = await _service.GetItemDetailsAsync(compId, itemIdList);

            return Ok(result);
        }
    }
}