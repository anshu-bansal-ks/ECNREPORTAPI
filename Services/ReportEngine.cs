using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Services
{
    public class ReportEngine
    {
        private readonly IConfiguration _config;
        public ReportEngine(IConfiguration config) => _config = config;

        public async Task<object?> GetReportDataAsync(string reportName, string compId, Dictionary<string, string> filters)
        {
            var conStr = new Models.Common(_config).GetDataBaseConnectionStringHardCoded(compId);
            using var con = new SqlConnection(conStr);
            var dashboard = _config["DASHBOARD"]!;
            string rName = reportName.ToLower();

            // 🔥 GLOBAL KEY MAPPING
            if (filters.ContainsKey("company")) filters["compId"] = filters["company"];
            if (filters.ContainsKey("salesrep")) filters["repId"] = filters["salesrep"];
            if (filters.ContainsKey("supplier")) filters["supplierId"] = filters["supplier"];
            if (filters.ContainsKey("customer")) filters["custId"] = filters["customer"];

            // 🔥 DATE LOGIC
            string timePeriod = filters.GetValueOrDefault("timeperiod") ?? filters.GetValueOrDefault("timePeriod") ?? "";
            string fromDate = filters.GetValueOrDefault("fromdate") ?? filters.GetValueOrDefault("fromDate") ?? "";
            string tillDate = filters.GetValueOrDefault("tilldate") ?? filters.GetValueOrDefault("tillDate") ?? "";

            if (!string.IsNullOrWhiteSpace(timePeriod) && timePeriod != "Time Period")
            {
                var pd = Models.Common.getPeriod(timePeriod); 
                fromDate = pd.from_date;
                tillDate = pd.till_date;
            }
            string dateRangeSnippet = (!string.IsNullOrWhiteSpace(fromDate) && !string.IsNullOrWhiteSpace(tillDate)) 
                ? $" '{fromDate} 00:00:00' AND '{tillDate} 23:59:59' " : "''";

            var p = new DynamicParameters();
            foreach (var item in filters) 
            {
                if (item.Key.ToLower().Contains("date") || item.Key.ToLower().Contains("period")) continue;
                p.Add(item.Key, item.Value);
            }

            if (!filters.ContainsKey("pageNumber")) p.Add("pageNumber", 1);
            if (!filters.ContainsKey("pageSize")) p.Add("pageSize", 100);
            if (!p.ParameterNames.Contains("compId")) p.Add("compId", compId);

            // ================= SPECIAL CASE: Thirteen Month (Pivot Logic) =================
            if (rName == "thirteenmonthcustomersalesforvendor")
            {
                var rawMonths = (await con.QueryAsync<dynamic>(GetMonthsQuery())).ToList();
                var months = rawMonths.Select(m => new { 
                    yr_mnth = m.yr_mnth.ToString().Trim(), 
                    pername = m.pername.ToString().Trim(),
                    row_num = m.row_num.ToString()
                }).ToList();
                
                string selectCols = string.Join(",", months.Select(m => $"[{m.yr_mnth}] as [mon{m.row_num}]"));
                string pivotIn = string.Join(",", months.Select(m => $"[{m.yr_mnth}]"));

                string sqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", $"{rName}.sql");
                string baseQuery = (await File.ReadAllTextAsync(sqlPath)).Replace("{dashboard}", dashboard);

                // SQL sirf pivot karega, totals hum C# mein handle karenge (Safe Zone)
                string finalPivotSql = $@"
                    SELECT * FROM (
                        SELECT supplier_id, supplier_name, customer_id, customer_name, rep, {selectCols}
                        FROM ( {baseQuery.Replace("{subquery}", "")} ) AS src 
                        PIVOT ( SUM(total_amount) FOR yr_mnth IN ({pivotIn}) ) AS pvt
                    ) AS FinalTable
                    ORDER BY supplier_name
                    OFFSET (@pageNumber - 1) * @pageSize ROWS
                    FETCH NEXT @pageSize ROWS ONLY";

                var pParams = new { 
                    repId = filters.GetValueOrDefault("repId", "ALL"), 
                    supplierId = filters.GetValueOrDefault("supplierId", "0"),
                    pageNumber = int.Parse(filters.GetValueOrDefault("pageNumber", "1")),
                    pageSize = int.Parse(filters.GetValueOrDefault("pageSize", "100000")) 
                };

                var rawData = await con.QueryAsync<dynamic>(finalPivotSql, pParams);
                var processedData = new List<IDictionary<string, object>>();

                foreach (var row in rawData)
                {
                    var dict = (IDictionary<string, object>)row;
                    decimal rowTotal = 0;
                    for (int i = 1; i <= months.Count; i++)
                    {
                        string key = $"mon{i}";
                        decimal val = 0;
                        if (dict.ContainsKey(key) && dict[key] != null) decimal.TryParse(dict[key].ToString(), out val);
                        // dict[key] = val; // Force numeric for Excel
                        // rowTotal += val;
                        decimal roundedVal = Math.Round(val, 0); 
        
                        dict[key] = roundedVal; 
                        rowTotal += roundedVal;
                    }
                    dict["total"] = Math.Round(rowTotal, 0);
                    processedData.Add(dict);
                }
                return new { Data = processedData, Months = months.Select(m => new { id = "mon" + m.row_num, month = m.pername }) };
            }

            // ================= DEFAULT CASE =================
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", $"{rName}.sql");
            if (!File.Exists(path)) return null;

            string rawSql = (await File.ReadAllTextAsync(path))
                .Replace("{dashboard}", dashboard)
                .Replace("{dateRange}", dateRangeSnippet);
            
            return await con.QueryAsync<dynamic>(rawSql, p);
        }

        private string GetMonthsQuery() => @"
            SELECT TOP 13 T.yr_mnth, T.pername, ROW_NUMBER() OVER (ORDER BY T.yr_mnth) row_num
            FROM (
                SELECT DISTINCT 
                    CAST(YEAR(invoice_date) AS VARCHAR(4)) + '_' + RIGHT('0' + CAST(MONTH(invoice_date) AS VARCHAR(2)), 2) as yr_mnth,
                    LEFT(UPPER(DATENAME(month, invoice_date)), 3) + '-' + RIGHT(CAST(YEAR(invoice_date) AS VARCHAR(4)), 2) as pername
                FROM p21_view_invoice_hdr WHERE DATEDIFF(mm, invoice_date, GETDATE()) < 13
            ) T ORDER BY T.yr_mnth";
    }
}