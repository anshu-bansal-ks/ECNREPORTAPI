using ECNREPORTAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECNREPORTAPI.Controllers
{
    [Route("api/listofskusupcspricescosts")]
    [ApiController]
    [Authorize]
    public class ListOfSkusUpcsPricesCostsController : ControllerBase
    {
        private readonly ListOfSkusUpcsPricesCostsService _service;

        public ListOfSkusUpcsPricesCostsController(ListOfSkusUpcsPricesCostsService service)
        {
            _service = service;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetData([FromQuery] string compId, [FromQuery] string itemIdList)
        {
            if (string.IsNullOrWhiteSpace(compId) || string.IsNullOrWhiteSpace(itemIdList))
                return BadRequest("compId and itemIdList are required.");

            var result = await _service.GetAsync(compId, itemIdList);
            return Ok(result);
        }
    }
}