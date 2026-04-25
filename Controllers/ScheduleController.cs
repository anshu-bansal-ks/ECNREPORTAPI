//Controller/ScheduleController
using ECNREPORTAPI.Models;
using ECNREPORTAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECNREPORTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly ScheduleService _service;

        public ScheduleController(ScheduleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Save(
            [FromQuery] string compId,
            [FromQuery] int userId,
            [FromBody] ScheduleReport model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(compId))
                    return BadRequest("compId required");

                if (userId <= 0)
                    return BadRequest("userId required");

                if (string.IsNullOrWhiteSpace(model.recepients))
                    return BadRequest("Recipients required");

               if (model.DeliveryDateTime == DateTime.MinValue)
                return BadRequest("Invalid date format received");

                var result = await _service.SaveScheduleAsync(model, compId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}