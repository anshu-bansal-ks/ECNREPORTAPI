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
        public static readonly Dictionary<string, dynamic> FOOTER_TOTAL_CONFIG = new()
        {
            { "lowprofitreport", new { labelColumn = "rep", totalColumns = new[] { "sales_amount", "shipping_cost", "gross_profit", "profit_percent" } } },
            { "itemwithpriceandcost", new { labelColumn = "item_desc", totalColumns = new[] { "price1", "cost" } } }
        };
        private async Task<object?> HandleSimpleReport(SqlConnection con, string rName, Dictionary<string, string> filters, string compId, Models.Common common)
        {
            try
            {
                string actualReportName = rName;
                
                // 🔥 SWITCHING SUFFIXES INSIDE ENGINE (Bina SQL ko chhede aur bina nayi function banaye)
                if (rName == "discitemswithbinqtyflags")
                {
                    string dataVersionInput = filters.GetValueOrDefault("Dataversion", "All Data").ToLower().Trim();
                    string suffix = "alldata";

                    if (dataVersionInput == "no qty") 
                        suffix = "noqty";
                    else if (dataVersionInput == "suppress from web flag") 
                        suffix = "suppressfromwebflag";

                    actualReportName = $"discitemswithbinqtyflags_{suffix}";
                }
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", $"{actualReportName}.sql");
                if (!File.Exists(filePath)) return null;

                string sql = await File.ReadAllTextAsync(filePath);
                sql = sql.Replace("{dashboard}", _dashboard);
                string dateRange = GetDateRangeSnippet(filters);
                if (sql.Contains("{dateRange}"))
                    sql = sql.Replace("{dateRange}", GetDateRangeSnippet(filters));
                string prevDateRange = GetPreviousYearDateRangeSnippet(filters);
                if (sql.Contains("{prevDateRange}"))
                    sql = sql.Replace("{prevDateRange}", prevDateRange);

                var p = PrepareParameters(filters, compId);   
                                       
                string locationid = filters.GetValueOrDefault("locationId") ?? "ALL";
                locationid = locationid.Trim();
                string LocationList = "";

                if (!String.IsNullOrEmpty(locationid) && (locationid == "All" || locationid == "ALL"))
                {
                    // Dropdown service se list lekar variable me pass kiya
                    string locType = filters.GetValueOrDefault("locType") ?? "WAREHOUSE";
                    LocationList = await _dropdownService.GetLocationByListAsync(compId, locType);
                   
                    p.Add("locationId", "ALL", DbType.String);
                    p.Add("LocationList", LocationList, DbType.String);
                }
                else
                {
                    p.Add("locationId", locationid, DbType.String);
                    p.Add("LocationList", DBNull.Value, DbType.String);
                }
                if (rName == "itemwithpriceandcost")
                {
                    string locType = filters.GetValueOrDefault("locType") ?? "WAREHOUSE";
                    string locationList = await _dropdownService.GetLocationByListAsync(compId, locType);
                    p.Add("LocationList", locationList);
                }
                var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 300)).ToList();
               var reportTotals = new Dictionary<string, decimal>();
                var config = FOOTER_TOTAL_CONFIG.ContainsKey(rName) ? FOOTER_TOTAL_CONFIG[rName] : null;
                int pageNum = int.TryParse(filters.GetValueOrDefault("pageNumber"), out int pn) ? pn : 1;
                
                if (pageNum == 1 && config != null)
                {
                    string totalSql = sql.Replace("OFFSET @Offset ROWS", "")
                                        .Replace("FETCH NEXT @PageSize ROWS ONLY", "");

                    var totalParams = new DynamicParameters(p);
                    totalParams.Add("Offset", 0);
                    totalParams.Add("PageSize", 10000000);

                    var allData = (await con.QueryAsync<dynamic>(
                        totalSql,
                        totalParams,
                        commandTimeout: 300)).ToList();

                    // Low Profit Report special calculation
                    if (rName == "lowprofitreport")
                    {
                        
                        decimal totalSalesAmount = allData.Sum(row =>
                        {
                            var dict = (IDictionary<string, object>)row;
                            return dict.ContainsKey("sales_amount") &&
                                decimal.TryParse(dict["sales_amount"]?.ToString(), out decimal d)
                                ? d : 0;
                        });

                        decimal totalShippingCost = allData.Sum(row =>
                        {
                            var dict = (IDictionary<string, object>)row;
                            return dict.ContainsKey("shipping_cost") &&
                                decimal.TryParse(dict["shipping_cost"]?.ToString(), out decimal d)
                                ? d : 0;
                        });

                        decimal totalGrossProfit = allData.Sum(row =>
                        {
                            var dict = (IDictionary<string, object>)row;
                            return dict.ContainsKey("gross_profit") &&
                                decimal.TryParse(dict["gross_profit"]?.ToString(), out decimal d)
                                ? d : 0;
                        });

                        reportTotals["sales_amount"] = totalSalesAmount;
                        reportTotals["shipping_cost"] = totalShippingCost;
                        reportTotals["gross_profit"] = totalGrossProfit;

                        reportTotals["profit_percent"] =
                            totalSalesAmount == 0
                                ? 0
                                : Math.Round((totalGrossProfit / totalSalesAmount) * 100, 4);
                        
                    }
                    
                    else
                    {
                        
                        foreach (var col in (string[])config!.totalColumns)
                        {
                            reportTotals[col] = allData.Sum(row =>
                            {
                                var dict = (IDictionary<string, object>)row;

                                return dict.ContainsKey(col) &&
                                    decimal.TryParse(dict[col]?.ToString(), out decimal d)
                                    ? d
                                    : 0;
                            });
                        }
                    }
                }
                if (rName == "ivdnewreleasereport")
                {
                    int totalCount = rawData.Count;
                    var summaryRow = new Dictionary<string, object?>();
                    summaryRow["item_id"] = "Number of New Release:";
                    summaryRow["item_desc"] = totalCount;summaryRow["Release_Date"] = null; 
                    rawData.Add(summaryRow);
                }
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

                return new { Data = rawData, ExcelData = excelData, Totals = reportTotals, Config = config };
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

        private string GetPreviousYearDateRangeSnippet(Dictionary<string, string> f)
        {
            string timeP = f.GetValueOrDefault("timeperiod") ??
                        f.GetValueOrDefault("timePeriod") ?? "";

            string from = f.GetValueOrDefault("fromdate") ??
                        f.GetValueOrDefault("fromDate") ?? "";

            string till = f.GetValueOrDefault("tilldate") ??
                        f.GetValueOrDefault("tillDate") ?? "";

            if (!string.IsNullOrWhiteSpace(timeP) && timeP != "Time Period")
            {
                var pd = Common.getPeriod(timeP);
                from = pd.from_date;
                till = pd.till_date;
            }

            if (!string.IsNullOrWhiteSpace(from) &&
                !string.IsNullOrWhiteSpace(till))
            {
                DateTime fromDt = DateTime.Parse(from).AddYears(-1);
                DateTime tillDt = DateTime.Parse(till).AddYears(-1);

                return $" '{fromDt:yyyy-MM-dd} 00:00:00' AND '{tillDt:yyyy-MM-dd} 23:59:59' ";
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
                    keyLower == "daysold" ||  keyLower == "minqty" || keyLower == "locationid" ||
                     keyLower == "locationlist") 
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