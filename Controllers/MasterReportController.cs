
using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Services.Excel;
using ECNREPORTAPI.Services.Pdf;
using Microsoft.AspNetCore.Authorization;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MasterReportController : ControllerBase
    {
        private readonly ReportEngine _engine;
        private readonly ExcelExportService _excel;
        private readonly PdfExportService _pdf;
        public MasterReportController(ReportEngine engine, ExcelExportService excel,PdfExportService pdf)
        {
            _engine = engine;
            _excel = excel;
            _pdf = pdf;
        }

        [HttpGet("{reportName}")]
        public async Task<IActionResult> Get(string reportName, [FromQuery] string compId="")
        {
            var filters = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var result = await _engine.GetReportDataAsync(reportName, compId, filters);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("{reportName}/excel")]
        public async Task<IActionResult> Export(string reportName, [FromBody] Models.ExportRequest req)
        {
            var filters = req.Filters ?? new Dictionary<string, string>();
            if (!filters.ContainsKey("isExport")) filters["isExport"] = "true";
            var result = await _engine.GetReportDataAsync(reportName, req.CompId, filters);

            if (result == null) return BadRequest("No data found.");
            IEnumerable<dynamic>? dataToExport = null;

            // ReportEngine se Data property nikalna
        var propData = result.GetType().GetProperty("Data");
        var propExcel = result.GetType().GetProperty("ExcelData");

        // ExcelData ko priority do agar ReportEngine ne process karke bheja hai
        if (propExcel != null)
        {
            dataToExport = propExcel.GetValue(result) as IEnumerable<dynamic>;
        }
        
        if (dataToExport == null && propData != null )
        {
            dataToExport = propData.GetValue(result) as IEnumerable<dynamic>;
        }

        if (dataToExport == null || !dataToExport.Any()) 
            return BadRequest("No records found to export.");

        string[]? excludeColumns = null;

        if (reportName.Equals("customerpayments", StringComparison.OrdinalIgnoreCase))
        {
            excludeColumns = new[] { "payment_number" };
        }

        Dictionary<string, string>? columnHeaderOverrides = null;

        string ytdComparison = filters.GetValueOrDefault(
            "ytdcomparison",
            "YTD v LYTD"
        );

        if (ytdComparison.Equals("YTD v LY", StringComparison.OrdinalIgnoreCase))
        {
            columnHeaderOverrides = new Dictionary<string, string>
            {
                ["SALES_LY"] = "SALES_LY"
            };
        }
        else
        {
            columnHeaderOverrides = new Dictionary<string, string>
            {
                ["SALES_LY"] = "SALES_LYTD"
            };
        }

    
        string reportTitle = req.ReportTitle ?? reportName;
            // 4. Generate Excel
            var fileBytes = _excel.ExportDynamicListToExcel(
                dataToExport, 
                reportName, 
                req.TotalColumns, 
                req.LabelColumn,
                excludeColumns,
                columnHeaderOverrides
            );
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
        }

        [HttpPost("{reportName}/pdf")]
        public async Task<IActionResult> ExportPdf(string reportName, [FromBody] Models.ExportRequest req)
        {
            var filters = req.Filters ?? new Dictionary<string, string>();
           if (!filters.ContainsKey("isExport")) filters["isExport"] = "true";
            var result = await _engine.GetReportDataAsync(reportName, req.CompId, filters);

            // 1. SAFE NULL CHECK (Yahan warning solve ho jayegi)
            if (result == null) 
                return BadRequest("No data found.");

            IEnumerable<dynamic>? dataToExport = null;

            // 2. Ab 'result' null nahi hai, safely reflection use karein
            var type = result.GetType();
            var propData = type.GetProperty("Data");
            var propExcel = type.GetProperty("ExcelData");

            if (propExcel != null) { dataToExport = (propExcel.GetValue(result) as IEnumerable<dynamic>) ?.Cast<object>(); } 
            if (dataToExport == null && propData != null) { dataToExport = (propData.GetValue(result) as IEnumerable<dynamic>) ?.Cast<object>(); }

            if (dataToExport == null || !dataToExport.Any()) return BadRequest("No records found.");

            // PDF Generate
           string reportTitle = req.ReportTitle ?? reportName; // PDF Generate 
           var pdfBytes = _pdf.ExportToPdf(
             dataToExport, reportTitle );
            return File(pdfBytes, "application/pdf", $"{reportName}.pdf");
        }
    
    
    }

    
}