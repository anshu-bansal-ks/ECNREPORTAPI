using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class Suppliers
    {
        public string supplier_id { get; set; } = "";
        public string supplier_name { get; set; } = "";
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

    public class Monthsupp
    {
        public string id { get; set; } = "";
        public string month { get; set; } = "";
    }

    public class ThirteenMonthCustomerSalesforVendor
    {
        private readonly IConfiguration _config;
        public ThirteenMonthCustomerSalesforVendor(IConfiguration config) { _config = config; }

        public List<Suppliers> Data { get; set; } = new();
        public List<Monthsupp> Months { get; set; } = new();

        public async Task<ThirteenMonthCustomerSalesforVendor> GetDataAsync(string compId, string repId, int supplierId)
        {
            var result = new ThirteenMonthCustomerSalesforVendor(_config);
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
                result.Months.Add(new Monthsupp { id = "mon" + dr["row_num"], month = dr["pername"].ToString() ?? "" });
            }
            subquery1 = subquery1.TrimEnd(',');
            subquery2 = subquery2.TrimEnd(',');

            // 2. Final SQL with Pivot
            string repFilter = repId != "ALL" ? " AND DA_Rep.salesrep_id=@rep_id " : "";
            string sql = $@"
                SELECT supplier_id, supplier_name, customer_id, customer_name, rep, {subquery1}
                FROM (
                    SELECT DA_Rep.rep, CAST(YEAR(invoice_date) AS VARCHAR(4)) + '_' + RIGHT('0' + CAST(MONTH(invoice_date) AS VARCHAR(2)),2) yr_mnth,
                           ih.customer_id, c.customer_name, il.supplier_id, s.supplier_name, ISNULL(il.extended_price,0) total_amount
                    FROM p21_view_invoice_hdr ih
                    JOIN p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
                    JOIN DA_Rep ON DA_Rep.customer_id = ih.customer_id
                    JOIN p21_view_customer c ON c.customer_id = ih.customer_id
                    JOIN p21_view_supplier s ON s.supplier_id = il.supplier_id
                    WHERE DATEDIFF(mm, invoice_date, GETDATE()) < 13 {repFilter} AND il.supplier_id=@supplierid
                ) src
                PIVOT (SUM(total_amount) FOR yr_mnth IN ({subquery2})) pvt
                ORDER BY supplier_name";

            using var cmd = new SqlCommand(sql, con);
            if (repId != "ALL") cmd.Parameters.AddWithValue("@rep_id", repId);
            cmd.Parameters.AddWithValue("@supplierid", supplierId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var s = new Suppliers {
                    supplier_id = reader["supplier_id"]?.ToString() ?? "",
                    supplier_name = reader["supplier_name"]?.ToString() ?? "",
                    customer_id = reader["customer_id"]?.ToString() ?? "",
                    customer_name = reader["customer_name"]?.ToString() ?? "",
                    rep = reader["rep"]?.ToString() ?? ""
                };
                decimal total = 0;
                for (int i = 1; i <= result.Months.Count; i++)
                {
                    var valStr = reader[$"mon{i}"]?.ToString() ?? "0";
                    typeof(Suppliers).GetProperty($"mon{i}")?.SetValue(s, valStr);
                    if (decimal.TryParse(valStr, out decimal d)) total += d;
                }
                s.Total = total;
                result.Data.Add(s);
            }
            return result;
        }

        public DataTable GetDataTableFromList(List<Suppliers> list)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("supplier_id"); dt.Columns.Add("supplier_name");
            dt.Columns.Add("customer_id"); dt.Columns.Add("customer_name");
            dt.Columns.Add("rep");
            for (int i = 1; i <= 13; i++) dt.Columns.Add($"mon{i}", typeof(decimal));
            dt.Columns.Add("Total", typeof(decimal));

            foreach (var item in list) {
                DataRow dr = dt.NewRow();
                dr["supplier_id"] = item.supplier_id; dr["supplier_name"] = item.supplier_name;
                dr["customer_id"] = item.customer_id; dr["customer_name"] = item.customer_name;
                dr["rep"] = item.rep;
                for (int i = 1; i <= 13; i++) {
                    var val = typeof(Suppliers).GetProperty($"mon{i}")?.GetValue(item)?.ToString() ?? "0";
                    dr[$"mon{i}"] = decimal.Parse(val);
                }
                dr["Total"] = item.Total;
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private string GetMonthsQuery() => @"
            SELECT TOP 13 T.yr_mnth, t.pername, ROW_NUMBER() OVER (ORDER BY yr_mnth) row_num
            FROM (
                SELECT DISTINCT CAST(YEAR(invoice_date) AS VARCHAR(4)) + '_' + RIGHT('0' + CAST(MONTH(invoice_date) AS VARCHAR(2)),2) yr_mnth,
                       LEFT(UPPER(DATENAME(month,invoice_date)),3) + '-' + RIGHT(CAST(YEAR(invoice_date) AS VARCHAR(4)),2) pername
                FROM p21_view_invoice_hdr WHERE DATEDIFF(mm, invoice_date, GETDATE()) < 13
            ) T ORDER BY yr_mnth";
    }
}