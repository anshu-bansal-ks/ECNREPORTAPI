
using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Services.Excel;
using Microsoft.AspNetCore.Authorization;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MasterReportController : ControllerBase
    {
        private readonly ReportEngine _engine;
        private readonly ExcelExportService _excel;

        public MasterReportController(ReportEngine engine, ExcelExportService excel)
        {
            _engine = engine;
            _excel = excel;
        }

        // ✅ DATA API: /api/MasterReport/afterhoursreport?compId=ECN
        [HttpGet("{reportName}")]
        public async Task<IActionResult> Get(string reportName, [FromQuery] string compId="")
        {
            // Frontend se saare filters uthao
            var filters = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            
            var result = await _engine.GetReportDataAsync(reportName, compId, filters);
            
            if (result == null) 
                return NotFound(new { message = $"SQL file for {reportName} not found." });

            return Ok(result);
        }

        // [HttpPost("{reportName}/excel")]
        // public async Task<IActionResult> Export(string reportName, [FromBody] Models.ExportRequest req)
        // {
        //     try
        //     {
        //         var filters = req.Filters ?? new Dictionary<string, string>();
        //         var result = await _engine.GetReportDataAsync(reportName, req.CompId, filters);

        //         if (result == null) return BadRequest("No data found.");

        //         IEnumerable<dynamic>? dataToExport = null;

        //         if (result is IEnumerable<dynamic> list)
        //         {
        //             dataToExport = list;
        //         }
        //         else
        //         {
        //             // 🔥 JSON serialization anonymous types ke internal access issue ko fix karta hai
        //             var json = System.Text.Json.JsonSerializer.Serialize(result);
        //             using var doc = System.Text.Json.JsonDocument.Parse(json);
                    
        //             if (doc.RootElement.TryGetProperty("Data", out var dataElement) || 
        //                 doc.RootElement.TryGetProperty("data", out dataElement))
        //             {
        //                 var dataJson = dataElement.GetRawText();
        //                 dataToExport = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(dataJson);
        //             }
        //         }

        //         if (dataToExport != null && dataToExport.Any())
        //         {
        //             var fileBytes = _excel.ExportDynamicListToExcel(
        //                 dataToExport, 
        //                 req.ReportName ?? reportName, 
        //                 req.TotalColumns, 
        //                 req.LabelColumn
        //             );
        //             return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
        //         }
        //         return BadRequest("No data available to export.");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"EXCEL ERROR: {ex.Message}");
        //         return StatusCode(500, $"Internal error: {ex.Message}");
        //     }
        // }
        //
         [HttpPost("{reportName}/excel")]
public async Task<IActionResult> Export(string reportName, [FromBody] Models.ExportRequest req)
{
    try
    {
        var filters = req.Filters ?? new Dictionary<string, string>();
        var result = await _engine.GetReportDataAsync(reportName, req.CompId, filters);

        if (result == null) return BadRequest("No data found.");

        List<IDictionary<string, object>> dataToExport = new List<IDictionary<string, object>>();

        // 1. Agar result pehle se hi List hai (Normal Reports)
        if (result is IEnumerable<dynamic> list)
        {
            dataToExport = list.Select(x => (IDictionary<string, object>)x).ToList();
        }
        else
        {
            // 2. Agar result Anonymous Object hai (ThirteenMonth Case)
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("Data", out var dataElement))
            {
                // 🔥 Yahan hum JsonElement ko Dictionary mein convert karenge
                foreach (var item in dataElement.EnumerateArray())
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in item.EnumerateObject())
                    {
                        // JsonElement ki value nikal kar object mein daalna
                        dict[prop.Name] = prop.Value.ValueKind switch
                        {
                            System.Text.Json.JsonValueKind.Number => prop.Value.GetDecimal(),
                            System.Text.Json.JsonValueKind.String => prop.Value.GetString()!,
                            System.Text.Json.JsonValueKind.True => true,
                            System.Text.Json.JsonValueKind.False => false,
                            System.Text.Json.JsonValueKind.Null => null!,
                            _ => prop.Value.GetRawText()
                        };
                    }
                    dataToExport.Add(dict);
                }
            }
        }

        // 3. Excel generation
        if (dataToExport.Any())
        {
            var fileBytes = _excel.ExportDynamicListToExcel(
                dataToExport.Cast<dynamic>(), 
                req.ReportName ?? reportName, 
                req.TotalColumns, 
                req.LabelColumn
            );

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
        }

        return BadRequest("No data available to export.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"EXCEL ERROR: {ex.Message}");
        return StatusCode(500, $"Internal error: {ex.Message}");
    }
}
    
    }
}