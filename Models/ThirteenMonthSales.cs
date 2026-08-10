using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class SalesCustomer
    {
        public string customer_id { get; set; } = "";
        public string customer_name { get; set; } = "";
        public string rep { get; set; } = "";
        public string mon1 { get; set; } = "0";
        public string mon2 { get; set; } = "0";
        public string mon3 { get; set; } = "0";
        public string mon4 { get; set; } = "0";
        public string mon5 { get; set; } = "0";
        public string mon6 { get; set; } = "0";
        public string mon7 { get; set; } = "0";
        public string mon8 { get; set; } = "0";
        public string mon9 { get; set; } = "0";
        public string mon10 { get; set; } = "0";
        public string mon11 { get; set; } = "0";
        public string mon12 { get; set; } = "0";
        public string mon13 { get; set; } = "0";
        public decimal Total { get; set; }
    }

    public class SalesMonthsupp
    {
        public string id { get; set; } = "";
        public string month { get; set; } = "";
    }

    public class ThirteenMonthSales
    {
        private readonly IConfiguration _config;
        public ThirteenMonthSales(IConfiguration config) { _config = config; }

        public List<SalesCustomer> Data { get; set; } = new();
        public List<SalesMonthsupp> Months { get; set; } = new();

        // Compatibility aliases for ReportEngine if needed
        public List<SalesCustomer> c1 => Data;
        public List<SalesMonthsupp> mon1 => Months;

        public async Task<ThirteenMonthSales> GetDataAsync(string compId, string repId)
        {
            var result = new ThirteenMonthSales(_config);
            Common common = new(_config);
            var conStr = common.GetDataBaseConnectionStringHardCoded(compId);
            if (string.IsNullOrWhiteSpace(conStr)) return result;

            await using var con = new SqlConnection(conStr);
            await con.OpenAsync();

            // 1. Get Dynamic Months
            string monthSql = GetMonthsQuery();
            using var cmdMonth = new SqlCommand(monthSql, con);
            using var readerMonth = await cmdMonth.ExecuteReaderAsync();
            DataTable dt1 = new DataTable();
            dt1.Load(readerMonth);

            string subquery1 = "";
            string subquery2 = "";
            foreach (DataRow dr in dt1.Rows)
            {
                subquery1 += $" ISNULL([{dr["yr_mnth"]}],0) as [mon{dr["row_num"]}],";
                subquery2 += $" [{dr["yr_mnth"]}],";
                result.Months.Add(new SalesMonthsupp { id = "mon" + dr["row_num"], month = dr["pername"].ToString() ?? "" });
            }
            subquery1 = subquery1.TrimEnd(',');
            subquery2 = subquery2.TrimEnd(',');

            // 2. Final SQL with Pivot
            string repFilter = repId != "ALL" ? " AND da_rep.salesrep_id=@rep_id " : "";
            string sql = $@"
                SELECT customer_id, customer_name, rep, {subquery1}
                FROM (
                    SELECT da_rep.salesrep_id,
                           CAST(YEAR(invoice_date) AS VARCHAR(4)) + '_' + CASE WHEN LEN(MONTH(invoice_date)) = 1 THEN '0' + CAST(MONTH(invoice_date) AS VARCHAR(2)) ELSE CAST(MONTH(invoice_date) AS VARCHAR(2)) END yr_mnth,
                           invoice_hdr.customer_id, customer.customer_name, da_rep.rep,
                           ISNULL((total_amount - freight), 0) total_amount
                    FROM invoice_hdr(nolock)
                    JOIN da_rep(nolock) ON da_rep.customer_id = invoice_hdr.customer_id
                    JOIN customer(nolock) ON invoice_hdr.customer_id = customer.customer_id
                    JOIN contacts(nolock) ON contacts.id = DA_Rep.salesrep_id
                    WHERE DATEDIFF(mm, invoice_date, GETDATE()) < 13 {repFilter}
                ) SalesSummary
                PIVOT (SUM(total_amount) FOR [yr_mnth] IN ({subquery2})) AS PivotTable
                ORDER BY PivotTable.rep, customer_name, customer_id DESC";

            using var cmd = new SqlCommand(sql, con);
            if (repId != "ALL") cmd.Parameters.AddWithValue("@rep_id", repId);
cmd.CommandTimeout = 240;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var s = new SalesCustomer {
                    customer_id = reader["customer_id"]?.ToString() ?? "",
                    customer_name = reader["customer_name"]?.ToString() ?? "",
                    rep = reader["rep"]?.ToString() ?? ""
                };
                decimal total = 0;
                for (int i = 1; i <= result.Months.Count; i++)
                {
                    var valStr = reader[$"mon{i}"]?.ToString() ?? "0";
                    typeof(SalesCustomer).GetProperty($"mon{i}")?.SetValue(s, valStr);
                    if (decimal.TryParse(valStr, out decimal d)) total += d;
                }
                s.Total = total;
                result.Data.Add(s);
            }
            return result;
        }

        private string GetMonthsQuery() => @"
            SELECT TOP 13 T.yr_mnth, t.pername, ROW_NUMBER() OVER (ORDER BY yr_mnth) row_num
            FROM (
                SELECT DISTINCT CAST(YEAR(invoice_date) AS VARCHAR(4)) + '_' + CASE WHEN LEN(MONTH(invoice_date)) = 1 THEN '0' + CAST(MONTH(invoice_date) AS VARCHAR(2)) ELSE CAST(MONTH(invoice_date) AS VARCHAR(2)) END yr_mnth,
                       LEFT(UPPER(DATENAME(month, invoice_date)), 3) + '-' + CAST(RIGHT(YEAR(invoice_date), 2) as varchar(10)) pername
                FROM invoice_hdr(nolock) WHERE DATEDIFF(mm, invoice_date, GETDATE()) < 13
            ) T ORDER BY yr_mnth";
    }
}