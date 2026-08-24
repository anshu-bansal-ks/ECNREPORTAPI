using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using ECNREPORTAPI.Models;
using System.Text;

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
                    sql = ApplyTopN(sql, filters);
                    
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
                else if (rName == "thirteenmonthsales")
                {
                    string repId = filters.GetValueOrDefault("repId", "ALL");

                    var model = new ThirteenMonthSales(_config);
                    var result = await model.GetDataAsync(compId, repId);

                    var excelData = ProcessSalesExcel(result.Data, result.Months);
                    return new { Data = result.Data, ExcelData = excelData, Months = result.Months };
                }

              else if (rName == "thirteenmonthvendorsalesforcustomer")
                {
                    string repId = filters.GetValueOrDefault("repId", "ALL");
                    int customerId = int.Parse(filters.GetValueOrDefault("custId", "0"));

                    var model = new ThirteenMonthVendorSalesForCustomer(_config);
                    var result = await model.GetDataAsync(compId, repId, customerId);

                    var excelData = ProcessVendorSalesExcel(result.Data, result.Months);
                    return new { Data = result.Data, ExcelData = excelData, Months = result.Months };
                }    
                else if (rName == "topcustomersytdvlytdwithmargin")
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "topcustomersytdvlytdwithmargin.sql");
                    if (!File.Exists(filePath)) return null;

                    string sql = await File.ReadAllTextAsync(filePath);
                    
                    sql = ApplyTopN(sql, filters);

                    var p = new DynamicParameters();
                    p.Add("repId", filters.GetValueOrDefault("repId", "ALL"));

                    var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 300)).ToList();
                    return new { Data = rawData, ExcelData = rawData };
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
            { "itemwithpriceandcost", new { labelColumn = "item_desc", totalColumns = new[] { "price1", "cost" } } },
            { "saleshistoryforitemprefixitemtotalsforallitems", new {  labelColumn = "item_desc", totalColumns = new[] { "qty", "SALES" }}},
            { "saleshistoryforitemprefixitemtotalsforallitemswithprofit", new {  labelColumn = "item_desc", totalColumns = new[] { "qty", "SALES" }}}

        };
        private async Task<object?> HandleSimpleReport(SqlConnection con, string rName, Dictionary<string, string> filters, string compId, Models.Common common)
        {
            try
            {
                string actualReportName = rName;
                
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
                sql = ApplyTopN(sql, filters);

                if (sql.Contains("{subSql}"))
                {
                    string subSql = compId.Equals("XG", StringComparison.OrdinalIgnoreCase)
                        ? ",CAST(PA_QTY AS INT) AS PA"
                        : @",
                        CAST(NJ_QTY AS INT) AS NJ
                        ,CAST(FL_QTY AS INT) AS FL
                        ,CAST(CA_QTY AS INT) AS CA";

                    sql = sql.Replace("{subSql}", subSql);
                }
                if (sql.Contains("{subQuery}"))
                {
                    string subQuery = compId.Equals("XG", StringComparison.OrdinalIgnoreCase)
                        ? ",CAST(vq.PA_Qty AS INT) AS PA"
                        : @",
                         CAST(vq.NJ_QTY AS INT) AS NJ
                        ,CAST(vq.FL_Qty AS INT) AS FL
                        ,CAST(vq.CA_Qty AS INT) AS CA";

                    sql = sql.Replace("{subQuery}", subQuery);
                }
                if (sql.Contains("{Qtydisstats}"))
                {
                    string Qtydisstats = compId.Equals("XG", StringComparison.OrdinalIgnoreCase)
                        ? @",CAST(V_QTY.PA_QTY AS INT) AS PA_QTY
                        ,CAST(V_QTY.PA_On_Order AS INT) AS PA_On_Order"
                                : @",
                        CAST(V_QTY.nj_Qty AS INT) AS nj_Qty
                        ,CAST(V_QTY.nj_On_Order AS INT) AS nj_On_Order
                        ,CAST(V_QTY.FL_Qty AS INT) AS FL_Qty
                        ,CAST(V_QTY.FL_On_Order AS INT) AS FL_On_Order
                        ,CAST(V_QTY.ca_Qty AS INT) AS ca_Qty
                        ,CAST(V_QTY.ca_On_Order AS INT) AS ca_On_Order";

                    sql = sql.Replace("{Qtydisstats}", Qtydisstats);
                }
                 if (sql.Contains("{QtySubQuery}"))
                {
                    string QtySubQuery = compId.Equals("XG", StringComparison.OrdinalIgnoreCase)
                        ? @",CAST(V_QTY.PA_QTY AS INT) AS PA"
                                : @",
                        CAST(V_QTY.NJ_QTY AS INT) AS NJ
                        ,CAST(V_QTY.FL_Qty AS INT) AS FL
                        ,CAST(V_QTY.CA_Qty AS INT) AS CA";

                    sql = sql.Replace("{QtySubQuery}", QtySubQuery);
                }

                if (sql.Contains("{Qtystats}"))
                {
                    string Qtystats;

                    if (compId.Equals("XG", StringComparison.OrdinalIgnoreCase))
                    {
                        Qtystats = @"
                        ,CAST(V_QTY.PA_Qty AS INT) AS PA";
                    }
                    else if (compId.Equals("ADV", StringComparison.OrdinalIgnoreCase))
                    {
                        Qtystats = @"
                        ,CAST(V_QTY.NJ_QTY AS INT) AS NJ
                        ,CAST(V_QTY.FL_Qty AS INT) AS FL
                        ,CAST(V_QTY.CA_Qty AS INT) AS CA
                        ,CAST(V_QTY.LV_Qty AS INT) AS LV";
                    }
                    else
                    {
                        Qtystats = @"
                        ,CAST(V_QTY.NJ_QTY AS INT) AS NJ
                        ,CAST(V_QTY.FL_Qty AS INT) AS FL
                        ,CAST(V_QTY.CA_Qty AS INT) AS CA";
                    }

                    sql = sql.Replace("{Qtystats}", Qtystats);
                }

                if (sql.Contains("{QtyColumns}"))
                {
                    string qtyColumns = compId.Equals("XG", StringComparison.OrdinalIgnoreCase)
                        ? @",CAST(DA_INVQTY.PA_Qty AS INT) AS PA,
                        CAST(DA_INVQTY.PA_On_Order AS INT) AS PA_PO"
                                : @",
                        CAST(DA_INVQTY.NJ_QTY AS INT) AS NJ,
                        CAST(DA_INVQTY.NJ_On_Order AS INT) AS NJ_PO,
                        CAST(DA_INVQTY.FL_QTY AS INT) AS FL,
                        CAST(DA_INVQTY.FL_On_Order AS INT) AS FL_PO,
                        CAST(DA_INVQTY.CA_Qty AS INT) AS CA,
                        CAST(DA_INVQTY.CA_On_Order AS INT) AS CA_PO";

                    sql = sql.Replace("{QtyColumns}", qtyColumns);
                }

                string dateRange = GetDateRangeSnippet(filters);
                if (sql.Contains("{dateRange}"))
                    sql = sql.Replace("{dateRange}", GetDateRangeSnippet(filters));
                string prevDateRange = GetPreviousYearDateRangeSnippet(filters);
                if (sql.Contains("{prevDateRange}"))
                    sql = sql.Replace("{prevDateRange}", prevDateRange);
                
                if (rName == "salesbysupplierforgroupcodeytdcomparison")
                {
                    DateTime today = DateTime.Today;
                    DateTime currentFrom = new DateTime(today.Year, 1, 1);
                    DateTime currentTill = today;
                    DateTime lastFrom = new DateTime(today.Year - 1, 1, 1);
                    DateTime lastTill;

                    string comparison = filters.GetValueOrDefault(
                        "ytdcomparison",
                        "YTD v LYTD"
                    );

                    if (comparison.Equals("YTD v LY", StringComparison.OrdinalIgnoreCase))
                    {
                        lastTill = new DateTime(today.Year - 1, 12, 31);
                    }
                    else
                    {
                        lastTill = today.AddYears(-1);
                    }

                    string currentYearDateRange =
                        $"'{currentFrom:MM/dd/yyyy} 00:00:00' AND '{currentTill:MM/dd/yyyy} 23:59:59'";

                    string lastYearDateRange =
                        $"'{lastFrom:MM/dd/yyyy} 00:00:00' AND '{lastTill:MM/dd/yyyy} 23:59:59'";

                    sql = sql.Replace(
                        "{currentYearDateRange}",
                        currentYearDateRange
                    );

                    sql = sql.Replace(
                        "{lastYearDateRange}",
                        lastYearDateRange
                    );
                }

               if (sql.Contains("{classnumber}"))
                {
                    sql = sql.Replace(
                        "{classnumber}",
                        filters.GetValueOrDefault("classnumber")
                    );
                }

                var p = PrepareParameters(filters, compId); 

                if (sql.Contains("{locationColumns}"))
                {
                string locType = filters.GetValueOrDefault("locType","WAREHOUSE");

                var locList = await _dropdownService.GetLocationListAsync(compId, locType
                );

                var locQuery = new StringBuilder();

                foreach (var loc in locList)
                {
                    if (string.IsNullOrWhiteSpace(loc.location_id) ||
                        string.IsNullOrWhiteSpace(loc.state))
                        continue;

                    string state = loc.state.Replace("]", "]]");

                    locQuery.AppendLine(
                        $",CAST(SUM(IIF(l.location_id = {loc.location_id}, qty_shipped, 0)) AS INT) AS [{state}]"
                    );

                    p.Add(
                        $"{loc.location_id}",
                        loc.location_id,
                        DbType.String
                    );
                }

                sql = sql.Replace(
                    "{locationColumns}",
                    locQuery.ToString()
                );
            }

                if (rName == "salesbycustomerforvendor" || rName == "salesbycustomerfromaspecifiedstateforvendor")
                {
                    p.Add("year", DateTime.Now.Year - 2, DbType.Int32);
                }

                                       
                string locationid = filters.GetValueOrDefault("locationId") ?? "ALL";
                locationid = locationid.Trim();
                string LocationList = "";

               if (locationid.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
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
                
                string? salesLyLabel = null;

                if (rName == "salesbysupplierforgroupcodeytdcomparison")
                {
                    string comparison = filters.GetValueOrDefault(
                        "ytdcomparison",
                        "YTD v LYTD"
                    );

                    salesLyLabel = comparison.Equals(
                        "YTD v LY",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? "Sales LY"
                        : "Sales LYTD";
                }
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
                  Console.WriteLine(ex.ToString());
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
        
        private string ApplyTopN(string sql, Dictionary<string, string> filters)
        {
            string topn = filters.GetValueOrDefault("topn", "ALL");

            string topsub = topn switch
            {
                "10" => "TOP 10",
                "25" => "TOP 25",
                "50" => "TOP 50",
                "100" => "TOP 100",
                "200" => "TOP 200",
                _ => ""
            };

            return sql.Replace("{topsub}", topsub);
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

        private List<IDictionary<string, object>> ProcessSalesExcel(List<SalesCustomer> data, List<SalesMonthsupp> months)
        {
            var formattedList = new List<IDictionary<string, object>>();
            foreach (var item in data)
            {
                var row = new Dictionary<string, object>();
                row["Customer ID"] = item.customer_id;
                row["Customer Name"] = item.customer_name;
                row["Rep"] = item.rep;

                for (int i = 1; i <= months.Count; i++)
                {
                    var prop = typeof(SalesCustomer).GetProperty($"mon{i}");
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

        private List<IDictionary<string, object>> ProcessVendorSalesExcel(List<VendorCustomerItem> data, List<VendorMonthsupp> months)
        {
            var formattedList = new List<IDictionary<string, object>>();
            foreach (var item in data)
            {
                var row = new Dictionary<string, object>();
                row["Customer ID"] = item.customer_id;
                row["Customer Name"] = item.customer_name;
                row["Supplier ID"] = item.supplier_id;
                row["Supplier Name"] = item.supplier_name;
                row["Rep"] = item.rep;

                for (int i = 1; i <= months.Count; i++)
                {
                    var prop = typeof(VendorCustomerItem).GetProperty($"mon{i}");
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


        private void NormalizeFilters(Dictionary<string, string> f)
        {
            
            if (f.ContainsKey("binzero")) {
                f["binzero"] = f["binzero"].ToLower() == "true" ? "true" : "false";
            } else {
                f["binzero"] = "false"; 
            }
            if (f.ContainsKey("stockable")) {
                f["stockable"] = f["stockable"]?.ToLower() == "true" ? "true" : "false";
            } else {
                f["stockable"] = "false";
            }
            if (f.ContainsKey("discontinued")) {
                f["discontinued"] = f["discontinued"]?.ToLower() == "true" ? "true" : "false";
            } else {
                f["discontinued"] = "false";
            }
            if (f.ContainsKey("status"))
            {
                f["status"] = f["status"]?.ToLower() == "true" ? "true" : "false";
            }
            else
            {
                f["status"] = "false";
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
            AddMonthYearParameters(p, f);
            
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
            if (f.ContainsKey("mcat"))
                p.Add("mcat", f["mcat"], DbType.String);
            else
                p.Add("mcat", "", DbType.String);

            if (f.ContainsKey("scat"))
                p.Add("scat", f["scat"], DbType.String);
            else
                p.Add("scat", "", DbType.String);
            if (f.ContainsKey("AllPO"))
                p.Add("AllPO", f["AllPO"], DbType.String);
            else
                p.Add("AllPO", "false", DbType.String);

            if (f.ContainsKey("pono"))
                p.Add("pono", f["pono"], DbType.String);
            else
                p.Add("pono", "", DbType.String);
            if (f.ContainsKey("supplierId"))
                p.Add("supplierId", f["supplierId"], DbType.String);
            else
                p.Add("supplierId", "", DbType.String);

            if (f.ContainsKey("productgroup"))
                p.Add("productgroup", f["productgroup"], DbType.String);
            else
                p.Add("productgroup", "", DbType.String);

            if (f.ContainsKey("rankType"))
            {
                string rankType = f["rankType"];

                rankType = rankType.Equals("Qty", StringComparison.OrdinalIgnoreCase)
                    ? "QTY"
                    : "DOLLAR";

                p.Add("rankType", rankType, DbType.String);
            }
            else
            {
                p.Add("rankType", "QTY", DbType.String);
            }

            if (f.ContainsKey("Alljobname"))
                p.Add("Alljobname", f["Alljobname"], DbType.String);
            else
                p.Add("Alljobname", "false", DbType.String);

            if (f.ContainsKey("job_name"))
                p.Add("job_name", f["job_name"], DbType.String);
            else
                p.Add("job_name", "", DbType.String);
            
            if (f.ContainsKey("roles"))
                p.Add("roles", f["roles"], DbType.String);
            else
                p.Add("roles", "ALL_ROLE", DbType.String);

            if (f.ContainsKey("buyer"))
            {
                p.Add("buyer", f["buyer"], DbType.String);
            }
            else
            {
                p.Add("buyer", "", DbType.String);
            }
            if (f.ContainsKey("rolesreports"))
            {
                p.Add("rolesreports", f["rolesreports"], DbType.String);
            }
            else
            {
                p.Add("rolesreports", "", DbType.String);
            }

          string startRelease = f.GetValueOrDefault("startrelease", "");

            if (DateTime.TryParseExact(
                startRelease,
                new[]
                {
                    "yyyy-MM-dd",
                    "MM/dd/yyyy",
                    "dd/MM/yyyy"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime startReleaseDate))
            {
                p.Add(
                    "startrelease",
                    startReleaseDate,
                    DbType.DateTime
                );
            }
            else
            {
                p.Add(
                    "startrelease",
                    DBNull.Value,
                    DbType.DateTime
                );
            }

            string endRelease =  f.GetValueOrDefault("endrelease", "");

            if (DateTime.TryParseExact(
                endRelease,
                new[]
                {
                    "yyyy-MM-dd",
                    "MM/dd/yyyy",
                    "dd/MM/yyyy"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime endReleaseDate))
            {
                p.Add(
                    "endrelease",
                    endReleaseDate,
                    DbType.DateTime
                );
            }
            else
            {
                p.Add(
                    "endrelease",
                    DBNull.Value,
                    DbType.DateTime
                );
            }

            string maxReceivedValue = f.GetValueOrDefault("maxRecieved", "1");

            if (int.TryParse(
                maxReceivedValue,
                out int maxReceived))
            {
                p.Add(
                    "maxXrecd",
                    maxReceived,
                    DbType.Int32
                );
            }
            else
            {
                p.Add(
                    "maxXrecd",
                    1,
                    DbType.Int32
                );
            }
            foreach (var item in f)
            {
                string key = item.Key;
                string keyLower = key.ToLower();
                if (keyLower == "fromdate" || keyLower == "tilldate" || keyLower == "timeperiod" || 
                    keyLower == "pagenumber" || keyLower == "pagesize" || keyLower == "isexport" || 
                    keyLower == "daysold" ||  keyLower == "minqty" || keyLower == "locationId" ||
                    keyLower == "locationlist" ||keyLower == "minqty" || keyLower == "mcat" || keyLower == "allpo" ||
                    keyLower == "pono"  || keyLower == "supplierid" || keyLower == "productgroup" ||  keyLower == "ranktype"
                    || keyLower == "Alljobname" || keyLower == "job_name" || keyLower == "startrelease" || keyLower == "endrelease" ||
                    keyLower == "maxrecieved" || keyLower == "roles" || keyLower == "buyer" || keyLower == "rolesreports" || 
                    keyLower == "ytdcomparison")
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
    
    private void AddMonthYearParameters(
    DynamicParameters p,
    Dictionary<string, string> filters)
    {
    string beginDate = filters.GetValueOrDefault("begindate", "");

    if (DateTime.TryParseExact(
        beginDate,
        new[] { "MM/yyyy", "MM/yy", "yyyy-MM" },
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out DateTime beginDt))
    {
        p.Add("begper", beginDt.Month, DbType.Int32);
        p.Add("begyr", beginDt.Year, DbType.Int32);
    }
    else
    {
        p.Add("begper", 1, DbType.Int32);
        p.Add("begyr", 1900, DbType.Int32);
    }

    string endDate = filters.GetValueOrDefault("enddate", "");

    if (DateTime.TryParseExact(
        endDate,
        new[] { "MM/yyyy", "MM/yy", "yyyy-MM" },
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out DateTime endDt))
    {
        p.Add("endper", endDt.Month, DbType.Int32);
        p.Add("endyr", endDt.Year, DbType.Int32);
    }
    else
    {
        p.Add("endper", 12, DbType.Int32);
        p.Add("endyr", 2099, DbType.Int32);
    }
}
    
    
    }
}