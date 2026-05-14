using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class ReportEngine
    {
        private readonly IConfiguration _config;
        private readonly DropdownService _dropdownService;
        private readonly string _dashboard;

        public ReportEngine(IConfiguration config, DropdownService dropdownService)
        {
            _config = config;
            _dashboard = _config["DASHBOARD"] ?? "";
            _dropdownService = dropdownService;
        }

        public async Task<object?> GetReportDataAsync(string reportName, string compId, Dictionary<string, string> filters)
        {
            
            var common = new Models.Common(_config);
            string conStr = string.IsNullOrWhiteSpace(compId) 
                            ? common.ConStr 
                            : common.GetDataBaseConnectionStringHardCoded(compId);

            using var con = new SqlConnection(conStr);
            string rName = reportName.ToLower();           
            NormalizeFilters(filters);
           
           if (rName == "customerinfo")
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "customerinfo.sql");
                if (!File.Exists(filePath)) return null;

                string sql = (await File.ReadAllTextAsync(filePath)).Replace("{dashboard}", _dashboard);
                
                
                var p = new DynamicParameters();
                p.Add("@compId", compId);
                p.Add("@custId", filters.GetValueOrDefault("custId", ""));
                using var multi = await con.QueryMultipleAsync(sql, p);
                return new { 
                    basic = (await multi.ReadAsync<dynamic>()).ToList(),
                    groupCode = (await multi.ReadAsync<dynamic>()).ToList(),
                    totalDue = (await multi.ReadAsync<dynamic>()).ToList(),
                    salesSummary = (await multi.ReadAsync<dynamic>()).ToList(),
                    salesDetails = (await multi.ReadAsync<dynamic>()).ToList()
                };
            }
            

            if (rName == "thirteenmonthcustomersalesforvendor")
            {
                string repId = filters.GetValueOrDefault("repId", "ALL");
                int supplierId = int.Parse(filters.GetValueOrDefault("supplierId", "0"));

                var model = new ThirteenMonthCustomerSalesforVendor(_config);
                var result = await model.GetDataAsync(compId, repId, supplierId);

                var excelData = ProcessHardReportExcel(result.Data, result.Months);
                return new { Data = result.Data, ExcelData = excelData, Months = result.Months };
            }
            

            return await HandleSimpleReport(con, rName, filters, compId,common);
        }
        private async Task<object?> HandleSimpleReport(SqlConnection con, string rName, Dictionary<string, string> filters, string compId, Models.Common common)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", $"{rName}.sql");
            if (!File.Exists(filePath)) return null;

            string sql = await File.ReadAllTextAsync(filePath);
            sql = sql.Replace("{dashboard}", _dashboard);
            string dateRange = GetDateRangeSnippet(filters);
            if (sql.Contains("{dateRange}"))
                sql = sql.Replace("{dateRange}", GetDateRangeSnippet(filters));

            var p = PrepareParameters(filters, compId);
            string finalQueryForDebug = sql; 
    System.Diagnostics.Debug.WriteLine($"\n🚀 REPORT: {rName} | DATES: {dateRange}");
             if (rName == "itemwithpriceandcost")
            {
               string locationList = await _dropdownService.GetLocationByListAsync(compId, "WAREHOUSE");
                p.Add("LocationList", locationList);
            }
            var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 300)).ToList();
            var excelData = rawData;
            if (filters.TryGetValue("isExport", out string? isExp) && isExp == "true")
            {

                if (rName == "binchangelocation" && filters.GetValueOrDefault("stockable") == "false")
                {
                    excelData = rawData.Select(d => {
                        var dict = (IDictionary<string, object>)d;
                        if (dict.ContainsKey("stockable")) dict.Remove("stockable");
                        return dict;
                    }).Cast<dynamic>().ToList();
                }
            }

            return new { Data = rawData, ExcelData = excelData };
        }
        private object ProcessUniversalData(IEnumerable<dynamic> rawData)
        {
            var excelData = rawData.Select(d => {
                var dict = (IDictionary<string, object>)d;
                var clean = new Dictionary<string, object>();
                foreach (var kvp in dict) {
                    string header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(kvp.Key.Replace("_", " ").ToLower());
                   
                    clean[header] = (kvp.Value is decimal || kvp.Value is double || kvp.Value is float) 
                        ? Math.Round(Convert.ToDecimal(kvp.Value), 2) : kvp.Value;
                }
                return clean;
            }).ToList();
            return new { Data = rawData, ExcelData = excelData };
        }
        
        private List<IDictionary<string, object>> ProcessHardReportExcel(List<Suppliers> data, List<Monthsupp> months)
        {
            var formattedList = new List<IDictionary<string, object>>();
            foreach (var item in data)
            {
                var row = new Dictionary<string, object>();
                row["Supplier ID"] = item.supplier_id;
                row["Supplier Name"] = item.supplier_name;
                row["Customer ID"] = item.customer_id;
                row["Customer Name"] = item.customer_name;
                row["Rep"] = item.rep;

                for (int i = 1; i <= months.Count; i++)
                {
                    var prop = typeof(Suppliers).GetProperty($"mon{i}");
                    string rawVal = prop?.GetValue(item)?.ToString() ?? "0";
                    if (decimal.TryParse(rawVal, out decimal d))
                    {
                        
                        row[months[i - 1].month] = Convert.ToInt64(Math.Round(d, 0)); 
                    }
                    else
                    {
                        row[months[i - 1].month] = 0;
                    }
                   
                }
                
                row["Total"] = Convert.ToInt64(Math.Round(item.Total, 0));
                formattedList.Add(row);
            }
            return formattedList;
        }

        // --- HELPERS ---
        private void NormalizeFilters(Dictionary<string, string> f)
        {
            if (f.ContainsKey("company")) f["compId"] = f["company"];
            if (f.ContainsKey("salesrep")) f["repId"] = f["salesrep"];
            if (f.ContainsKey("vendor")) f["vendorId"] = f["vendor"];
            if (f.ContainsKey("supplier") || f.ContainsKey("supplierop") || f.ContainsKey("locationsupplier"))
            { f["supplierId"] = f.GetValueOrDefault("supplier") ?? 
                                f.GetValueOrDefault("supplierop") ?? 
                                f.GetValueOrDefault("locationsupplier") ?? "ALL";
            }
            if (f.ContainsKey("customer")) f["custId"] = f["customer"];
            if (f.ContainsKey("location")) f["locationId"] = f["location"];
            if (f.ContainsKey("itemId")) f["itemId"] = f["itemId"].Trim();
            if (f.ContainsKey("ordernum")) f["ordernum"] = f["ordernum"].Trim();
            if (f.ContainsKey("custClass")) f["custclass"] = f["custClass"];
            if (f.ContainsKey("bank_no")) f["bank_no"] = f["bank_no"];
            if (f.ContainsKey("emailaddress")) f["emailaddress"] = f["emailaddress"];
            if (f.ContainsKey("minavail")) f["minavail"] = f["minavail"];
            if (f.ContainsKey("startperiod")) f["startperiod"] = f["startperiod"];
            if (f.ContainsKey("endperiod")) f["endperiod"] = f["endperiod"];
            if (f.ContainsKey("binzero")) {
                f["binzero"] = f["binzero"].ToLower() == "true" ? "true" : "false";
            } else {
                f["binzero"] = "false"; // Default false
            }
            if (f.ContainsKey("stockable")) {
                f["stockable"] = f["stockable"]?.ToLower() == "true" ? "true" : "false";
                } else {
                    f["stockable"] = "false";
                }
        }

        private string GetDateRangeSnippet(Dictionary<string, string> f)
        {
            string timeP = f.GetValueOrDefault("timeperiod") ?? f.GetValueOrDefault("timePeriod") ?? "";
            string from = f.GetValueOrDefault("fromdate") ?? f.GetValueOrDefault("fromDate") ?? "";
            string till = f.GetValueOrDefault("tilldate") ?? f.GetValueOrDefault("tillDate") ?? "";

            if (!string.IsNullOrWhiteSpace(timeP) && timeP != "Time Period")
            {
                var pd = Common.getPeriod(timeP);
                from = pd.from_date;
                till = pd.till_date;
            }

            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(till))
            {
               return $" '{from} 00:00:00' AND '{till} 23:59:59' ";
            }
            return " '1900-01-01' AND '2099-12-31' ";
        }

        private DynamicParameters PrepareParameters(Dictionary<string, string> f, string compId)
        {
            var p = new DynamicParameters();
            
           bool isExport = f.ContainsKey("isExport") && 
                    (f["isExport"].ToString().ToLower() == "true" || f["isExport"].ToString() == "1");
            p.Add("binzero", f.GetValueOrDefault("binzero", "false").ToLower() == "true" ? "true" : "false");
            
            string startVal = f.GetValueOrDefault("startperiod") ?? "";
    string endVal = f.GetValueOrDefault("endperiod") ?? "";

    p.Add("startperiod", startVal); 
    p.Add("endperiod", endVal);
    p.Add("locationId", f.GetValueOrDefault("locationId") ?? f.GetValueOrDefault("location") ?? "");
            
            foreach (var item in f)
            {
                string key = item.Key.ToLower();
            if (key.Contains("date") || key.Contains("period") || 
                key == "pagenumber" || key == "pagesize" || key == "isexport") continue;

                p.Add(item.Key, item.Value);
            }

            if (isExport) {
                p.Add("Offset", 0);
                p.Add("PageSize", 1000000); 
            } else {
                int pageNumber = int.TryParse(f.GetValueOrDefault("pageNumber"), out int pn) ? pn : 1;
                int pageSize = int.TryParse(f.GetValueOrDefault("pageSize"), out int ps) ? ps : 100;
                p.Add("Offset", (pageNumber - 1) * pageSize);
                p.Add("PageSize", pageSize);
            }
            // 3. CompId safety
            if (!string.IsNullOrEmpty(compId) && !p.ParameterNames.Contains("compId")) 
                p.Add("compId", compId);

            return p;
        }
    }
}