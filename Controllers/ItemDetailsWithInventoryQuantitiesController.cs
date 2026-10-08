using ECNREPORTAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECNREPORTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemDetailsWithInventoryQuantitiesController : ControllerBase
    {
        private readonly ItemDetailsWithInventoryQuantitiesService _service;

        public ItemDetailsWithInventoryQuantitiesController(ItemDetailsWithInventoryQuantitiesService service)
        {
            _service = service;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetData([FromQuery] string compId, [FromQuery] string itemIdList)
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