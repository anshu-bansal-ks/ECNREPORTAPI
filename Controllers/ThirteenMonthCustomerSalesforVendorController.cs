using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Services.Excel;
using ECNREPORTAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThirteenMonthCustomerSalesforVendorController : BaseReportController<ThirteenMonthCustomerSalesforVendor> 
    {
        private readonly ThirteenMonthCustomerSalesforVendorService _service;

        public ThirteenMonthCustomerSalesforVendorController(ThirteenMonthCustomerSalesforVendorService service, ExcelExportService excelService) : base(excelService)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string compId, [FromQuery] string repId, [FromQuery] int supplierId)
        {
            if (string.IsNullOrWhiteSpace(compId)) return BadRequest("compId required");
            var result = await _service.GetAsync(compId, repId, supplierId);
            return Ok(result);
        }

        [HttpPost("excel")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportRequest req)
        {
            string repId = req.Filters.ContainsKey("salesrep") ? req.Filters["salesrep"] : "ALL";
            int supplierId = req.Filters.ContainsKey("supplier") ? int.Parse(req.Filters["supplier"]) : 0;

            DataTable dt = await _service.GetDataTableAsync(req.CompId, repId, supplierId);
            return ExportDataTableToExcel(dt, req); // Base helper used
        }
    }
}