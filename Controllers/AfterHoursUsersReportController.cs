using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Services.Excel;
using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AfterHoursUsersReportController : BaseReportController<AfterHoursUsersReport>
    {
        private readonly AfterHoursUsersReportService _service;

        public AfterHoursUsersReportController(AfterHoursUsersReportService service,ExcelExportService excelExportService): base(excelExportService)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? fromDate,
            [FromQuery] string? tillDate,
            [FromQuery] string? timePeriod)
        {
            try
            {
                var data = await _service.GetReportAsync(fromDate, tillDate, timePeriod);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load After Hours Users Report",
                    error = ex.Message
                });
            }
        }

        [HttpPost("excel")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportRequest req)
        {
            var filters = req.Filters;

            string? fromDate = filters.ContainsKey("fromdate") ? filters["fromdate"] : null;
            string? tillDate = filters.ContainsKey("tilldate") ? filters["tilldate"] : null;
            string? timePeriod = filters.ContainsKey("timeperiod") ? filters["timeperiod"] : null;

            return await ExportListToExcel(
                req,
               _service.GetReportAsync(fromDate, tillDate, timePeriod)
            );
        }

    }
}
