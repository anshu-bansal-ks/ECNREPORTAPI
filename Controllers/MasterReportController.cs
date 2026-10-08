
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
            var propData = result.GetType().GetProperty("Data");
            var rawList = propData?.GetValue(result) as List<Dictionary<string, object>>;

            if (reportName.Equals("invoiceexport2", StringComparison.OrdinalIgnoreCase))
            {
                var excelBytes = _excel.ExportInvoiceExport2ToExcel(rawList ?? new(), req.CompId);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
            }

            if (reportName.Equals("invoiceexport3", StringComparison.OrdinalIgnoreCase))
            {
                var excelBytes = _excel.ExportInvoiceExport3ToExcel(rawList ?? new(), req.CompId);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
            }
            if (reportName.Equals("customerbreakdownmonthovermonthgroupcode", StringComparison.OrdinalIgnoreCase))
            {
                var localPropData = result.GetType().GetProperty("Data");
                var groupedList = localPropData?.GetValue(result) as List<List<Dictionary<string, object>>>;

                if (groupedList != null && groupedList.Any())
                {
                    var excelBytes = _excel.ExportCustomerBreakdownGroupedToExcel(groupedList, req.CompId);
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
                }
            }
            if (reportName.Equals("groupcodes", StringComparison.OrdinalIgnoreCase) || reportName.Equals("groupcode", StringComparison.OrdinalIgnoreCase))
            {
                var localPropData = result.GetType().GetProperty("GroupedData");
                var groupedDict = localPropData?.GetValue(result) as Dictionary<string, List<Dictionary<string, object>>>;

                if (groupedDict != null && groupedDict.Any())
                {
                    var excelBytes = _excel.ExportGroupCodesGroupedToExcel(groupedDict);
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
                }
            }
            if (reportName.Equals("listofskusupcspricescosts", StringComparison.OrdinalIgnoreCase))
            {
                var listProp = result.GetType().GetProperty("Data");
                var listData = listProp?.GetValue(result) as List<Dictionary<string, object>>;
                var excelBytes = _excel.ExportListOfSkusToExcel(listData ?? new());
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
            }
            if (reportName.Equals("backorders", StringComparison.OrdinalIgnoreCase))
            {
                var listProp = result.GetType().GetProperty("Data");
                var listData = listProp?.GetValue(result) as IEnumerable<dynamic>;

                if (listData != null && listData.Any())
                {
                    var dataList = listData.Select(x => (IDictionary<string, object>)x).ToList();
                    
                    // Fixed type conversion to List<string> to prevent CS0173 compilation error
                    List<string> columnsToExport = (req.ColumnNames != null && req.ColumnNames.Any()) 
                        ? req.ColumnNames.ToList() 
                        : dataList[0].Keys.ToList();

                    var filteredExcelData = dataList.Select(item => {
                        var newDict = new Dictionary<string, object>();
                        foreach (var colName in columnsToExport)
                        {
                            var matchedKey = item.Keys.FirstOrDefault(k => k.Equals(colName, StringComparison.OrdinalIgnoreCase));
                            newDict[colName] = matchedKey != null ? (item[matchedKey] ?? "") : "";
                        }
                        return (object)newDict;
                    }).ToList();

                    var labels = req.ColumnLabels != null ? req.ColumnLabels.ToList() : columnsToExport;
                    var types = req.ColumnDataTypes ?? columnsToExport.Select(_ => "text").ToArray();

                    var excelBytes = _excel.ExportDynamicListToExcel(
                        filteredExcelData, 
                        reportName, 
                        req.TotalColumns, 
                        req.LabelColumn,
                        null,
                        null,
                        columnsToExport.ToArray(),
                        types,
                        labels.ToArray()
                    );
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
                }
            }
            if (reportName.Equals("arcallnotes", StringComparison.OrdinalIgnoreCase))
            {
                var type = result.GetType();

                var dataProp = type.GetProperty("Data") ?? type.GetProperty("data");
                var detailRaw = dataProp?.GetValue(result) as IEnumerable<object> ?? Enumerable.Empty<object>();

                var detailDicts = detailRaw
                    .Select(x =>
                    {
                        if (x is IDictionary<string, object> d1)
                            return d1.ToDictionary(k => k.Key, k => k.Value ?? (object)"");

                        if (x is IDictionary<string, object?> d2)
                            return d2.ToDictionary(k => k.Key, k => (object)(k.Value ?? ""));

                        var dict = new Dictionary<string, object>();
                        foreach (var prop in x.GetType().GetProperties())
                        {
                            dict[prop.Name] = prop.GetValue(x) ?? "";
                        }
                        return dict;
                    })
                    .ToList();
                var summaryProp = type.GetProperty("SummaryData")
                            ?? type.GetProperty("summaryData")
                            ?? type.GetProperty("Summary");
                var summaryRaw = summaryProp?.GetValue(result) as IEnumerable<object> ?? Enumerable.Empty<object>();

                var summaryDicts = summaryRaw
                    .Select(x =>
                    {
                        if (x is IDictionary<string, object> d1)
                            return d1.ToDictionary(k => k.Key, k => k.Value ?? (object)"");

                        if (x is IDictionary<string, object?> d2)
                            return d2.ToDictionary(k => k.Key, k => (object)(k.Value ?? ""));

                        var dict = new Dictionary<string, object>();
                        foreach (var prop in x.GetType().GetProperties())
                        {
                            dict[prop.Name] = prop.GetValue(x) ?? "";
                        }
                        return dict;
                    })
                    .ToList();

                var excelBytes = _excel.ExportArcallNotesToExcel(detailDicts, summaryDicts);
                return File(excelBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "AR_Call_Notes.xlsx");
            }
            if (reportName.Equals("creditholdswithrelease", StringComparison.OrdinalIgnoreCase))
            {
                var type = result.GetType();

                var groupedProp = type.GetProperty("GroupedData")?? type.GetProperty("groupedData")?? type.GetProperty("Data")?? type.GetProperty("data");

                var raw = groupedProp?.GetValue(result);

                Dictionary<string, List<Dictionary<string, object>>>? grouped = null;

                if (raw is Dictionary<string, List<Dictionary<string, object>>> d1)
                {
                    grouped = d1;
                }
                else if (raw is System.Collections.IDictionary dict)
                {
                    grouped = new Dictionary<string, List<Dictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);

                    foreach (System.Collections.DictionaryEntry entry in dict)
                    {
                        string key = entry.Key?.ToString() ?? "";
                        var list = new List<Dictionary<string, object>>();

                        if (entry.Value is System.Collections.IEnumerable rows)
                        {
                            foreach (var row in rows)
                            {
                                if (row is null) continue;

                                if (row is IDictionary<string, object> d)
                                {
                                    list.Add(d.ToDictionary(k => k.Key, k => k.Value ?? (object)""));
                                }
                                else if (row is IDictionary<string, object?> d2)
                                {
                                    list.Add(d2.ToDictionary(k => k.Key, k => (object)(k.Value ?? "")));
                                }
                                else
                                {
                                    var tmp = new Dictionary<string, object>();
                                    foreach (var p in row.GetType().GetProperties())
                                    {
                                        tmp[p.Name] = p.GetValue(row) ?? "";
                                    }
                                    list.Add(tmp);
                                }
                            }
                        }

                        grouped[key] = list;
                    }
                }

                if (grouped == null || !grouped.Any())
                    return BadRequest("No records found to export.");

                var excelBytes = _excel.ExportCreditHoldsWithReleaseToExcel(grouped);
                return File(excelBytes,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","Credit_Holds_With_Release.xlsx");
            }
           
            IEnumerable<dynamic>? dataToExport = null;
        var propExcel = result.GetType().GetProperty("ExcelData");
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
            var fileBytes = _excel.ExportDynamicListToExcel(
                dataToExport, 
                reportName, 
                req.TotalColumns, 
                req.LabelColumn,
                excludeColumns,
                columnHeaderOverrides,
                req.ColumnNames,
                req.ColumnDataTypes,
                req.ColumnLabels
            );
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");
        }

        [HttpPost("{reportName}/pdf")]
        public async Task<IActionResult> ExportPdf(string reportName, [FromBody] Models.ExportRequest req)
        {
            var filters = req.Filters ?? new Dictionary<string, string>();
           if (!filters.ContainsKey("isExport")) filters["isExport"] = "true";
            var result = await _engine.GetReportDataAsync(reportName, req.CompId, filters);

            if (result == null) 
                return BadRequest("No data found.");

            IEnumerable<dynamic>? dataToExport = null;

            var type = result.GetType();
            var propData = type.GetProperty("Data");
            var propExcel = type.GetProperty("ExcelData");

            if (propExcel != null) { dataToExport = (propExcel.GetValue(result) as IEnumerable<dynamic>) ?.Cast<object>(); } 
            if (dataToExport == null && propData != null) { dataToExport = (propData.GetValue(result) as IEnumerable<dynamic>) ?.Cast<object>(); }

            if (dataToExport == null || !dataToExport.Any()) return BadRequest("No records found.");

           string reportTitle = req.ReportTitle ?? reportName; 
           var pdfBytes = _pdf.ExportToPdf(
             dataToExport, reportTitle );
            return File(pdfBytes, "application/pdf", $"{reportName}.pdf");
        }
    
    
    }

    
}