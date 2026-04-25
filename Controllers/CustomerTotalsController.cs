using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Services.Excel;
using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerTotalsController : BaseReportController<CustomerTotals>
    {
        private readonly CustomerTotalsService _service;

        public CustomerTotalsController(
            CustomerTotalsService service,
            ExcelExportService excelExportService
        ) : base(excelExportService)
        {
            _service = service;
        }

        // ✅ GET
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("companyId is required");

            var data = await _service.GetCustomerTotalsAsync(compId.ToLower());
            return Ok(data);
        }

        // ✅ EXCEL (CLEAN)
        [HttpPost("excel")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportRequest req)
        {
            var compId = req.Filters?["company"] ?? req.CompId;

            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("companyId is required");

            return await ExportListToExcel(
                req,
                _service.GetCustomerTotalsAsync(compId.ToLower())
            );
        }
    }
}