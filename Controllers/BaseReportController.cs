
using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services.Excel;
using ECNREPORTAPI.Models;
using System.Data;
using System.Reflection;

namespace ECNREPORTAPI.Controllers
{
    public class BaseReportController<T> : ControllerBase
    {
        protected readonly ExcelExportService _excelService;
        public BaseReportController(ExcelExportService excelService) => _excelService = excelService;

        [NonAction]
        protected async Task<IActionResult> ExecuteReportAsync(Func<int, int, Task<List<T>>> fetchData)
        {
            try {
                int pageNumber = int.TryParse(Request.Query["pageNumber"], out var pn) ? pn : 1;
                int pageSize = int.TryParse(Request.Query["pageSize"], out var ps) ? ps : 5000;
                var result = await fetchData(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [NonAction]
        protected async Task<IActionResult> ExportListToExcel(ExportRequest req, Task<List<T>> dataTask)
        {
            try {
                var list = await dataTask;
                if (list == null || list.Count == 0) return BadRequest("No data");

        // 🔥 FIX: Purana 'ExportToExcel' hata kar naya generic method call kiya
        // Hum list ko direct dynamic IEnumerable mein cast kar rahe hain
        var bytes = _excelService.ExportDynamicListToExcel((IEnumerable<dynamic>)list, req.ReportName);
        
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{req.ReportName}.xlsx");
               
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
      
       private DataTable ToDataTable(List<T> items)
{
    DataTable dt = new DataTable(typeof(T).Name);
    PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    // 🔥 STEP 1: Columns with correct datatype
    foreach (var p in props)
    {
        var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
        dt.Columns.Add(p.Name, type); // ✅ datatype set
    }

    // 🔥 STEP 2: Fill rows
    foreach (var item in items)
    {
        var vals = new object[props.Length];

        for (int i = 0; i < props.Length; i++)
        {
            var val = props[i].GetValue(item, null);

            vals[i] = val ?? DBNull.Value; // ✅ null handle
        }

        dt.Rows.Add(vals);
    }

    return dt;
}
        [NonAction]
        protected IActionResult ExportDataTableToExcel(DataTable dt, ExportRequest req)
        {
            var bytes = _excelService.ExportDynamicListToExcel(dt.AsEnumerable(), req.ReportName);
    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{req.ReportName}.xlsx");
        }
    }
}