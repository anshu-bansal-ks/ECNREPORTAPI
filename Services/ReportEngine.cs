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
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in GetReportDataAsync: {ex.Message}");
                throw; 
            }
        }
        private async Task<object?> HandleSimpleReport(SqlConnection con, string rName, Dictionary<string, string> filters, string compId, Models.Common common)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", $"{rName}.sql");
                if (!File.Exists(filePath)) return null;

                string sql = await File.ReadAllTextAsync(filePath);
                sql = sql.Replace("{dashboard}", _dashboard);
                string dateRange = GetDateRangeSnippet(filters);
                if (sql.Contains("{dateRange}"))
                    sql = sql.Replace("{dateRange}", GetDateRangeSnippet(filters));

                var p = PrepareParameters(filters, compId);
            
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in HandleSimpleReport ({rName}): {ex.Message}");
                return null;
            }
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
            
            bool isExport = f.ContainsKey("isExport") && f["isExport"].ToLower() == "true";
            if (f.ContainsKey("daysold"))
            {
                if (int.TryParse(f["daysold"], out int daysOldVal))
                {
                    p.Add("daysold", daysOldVal, DbType.Int32); 
                }
                else
                {
                    p.Add("daysold", 180, DbType.Int32); 
                }
            }
            if (f.ContainsKey("minqty"))
            {
                if (int.TryParse(f["minqty"], out int minQtyVal))
                {
                    p.Add("minqty", minQtyVal, DbType.Int32); 
                }
                else
                {
                    p.Add("minqty", 0, DbType.Int32); 
                }
            }
            foreach (var item in f)
            {
                string key = item.Key;
                string keyLower = key.ToLower();
                if (keyLower == "fromdate" || keyLower == "tilldate" || keyLower == "timeperiod" || 
                    keyLower == "pagenumber" || keyLower == "pagesize" || keyLower == "isexport" || 
                    keyLower == "daysold" ||  keyLower == "minqty") 
                    continue;

                p.Add(key, item.Value);
            }
            
            if (isExport) {
                p.Add("Offset", 0);
                p.Add("PageSize", 1000000); 
            } 
            else if (f.ContainsKey("pageNumber")) 
            {
                int pageNumber = int.TryParse(f.GetValueOrDefault("pageNumber"), out int pn) ? pn : 1;
                int pageSize = int.TryParse(f.GetValueOrDefault("pageSize"), out int ps) ? ps : 100;
                p.Add("Offset", (pageNumber - 1) * pageSize);
                p.Add("PageSize", pageSize);
            }
            if (!p.ParameterNames.Any(x => x.Equals("compId", StringComparison.OrdinalIgnoreCase))) 
                p.Add("compId", compId);

            return p;
        }
    }
}