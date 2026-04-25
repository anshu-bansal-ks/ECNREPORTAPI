
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Services.Excel;
using ECNREPORTAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECNREPORTAPI.Controllers
{
    [Route("api/[controller]")]
[ApiController]
[Authorize]
public class OpenPOController : BaseReportController<OpenPO> // 👈 1. Model Name yahan dalo
{
    private readonly OpenPOService _service;
    public OpenPOController(OpenPOService s, ExcelExportService e) : base(e) => _service = s;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string compId, [FromQuery] string? supplierId)
    {
        // 👈 2. Data ke liye bas ye line
        return await ExecuteReportAsync((page, size) => _service.GetOpenPOAsync(compId, supplierId, page, size));
    }

// ✅ EXCEL EXPORT API
        [HttpPost("excel")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportRequest req)
        {
           string? supplierId = null;

            if (req.Filters != null)
            {
                if (!req.Filters.TryGetValue("supplierop", out supplierId) || string.IsNullOrEmpty(supplierId))
                {
                    req.Filters.TryGetValue("supplier", out supplierId);
                }
            }

            return await ExportListToExcel(
                req,
                _service.GetOpenPOAsync(req.CompId, supplierId, 1, 1000000)
            );
            
        }
    }
}