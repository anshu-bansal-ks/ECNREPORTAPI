using Microsoft.Data.SqlClient;

namespace ECNREPORTAPI.Models
{
    public class CustomerTotals
    {
        public string CustomerId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public decimal ShowTotal { get; set; } 

        public List<CustomerTotals> GetData(string compId, IConfiguration config)
        {
            var list = new List<CustomerTotals>();

            Common _common = new(config);
            var conStr = _common.GetDataBaseConnectionStringHardCoded(compId);

            if (string.IsNullOrWhiteSpace(conStr))
                return list;

            // 🔥 Core me "DASHBOARD" yahan se milega
            var dashboard = config["DASHBOARD"]!;

            using var con = new SqlConnection(conStr);

            string sql = @"  IF OBJECT_ID('tempdb..#vstable') IS NOT NULL" + System.Environment.NewLine +
                                    "  DROP TABLE #vstable;" + System.Environment.NewLine +
                                    "  SELECT invoice_no" + System.Environment.NewLine +
                                    "  , invoice_date" + System.Environment.NewLine +
                                    "  , po_no" + System.Environment.NewLine +
                                    "  , customer_id" + System.Environment.NewLine +
                                    "  , customer_name" + System.Environment.NewLine +
                                    "  , supplier_id" + System.Environment.NewLine +
                                    "  , supplier_name" + System.Environment.NewLine +
                                    "  , amt" + System.Environment.NewLine +
                                    "  INTO #vstable" + System.Environment.NewLine +
                                    "  FROM" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  SELECT ih.invoice_no" + System.Environment.NewLine +
                                    "  , ih.invoice_date" + System.Environment.NewLine +
                                    "  , ih.po_no" + System.Environment.NewLine +
                                    "  , ih.customer_id" + System.Environment.NewLine +
                                    "  , c.customer_name" + System.Environment.NewLine +
                                    "  , il.supplier_id" + System.Environment.NewLine +
                                    "  , s.supplier_name" + System.Environment.NewLine +
                                    "  , SUM(il.extended_price) amt" + System.Environment.NewLine +
                                    "  FROM ccecn.dbo.p21_view_invoice_hdr ih" + System.Environment.NewLine +
                                    "  JOIN ccecn.dbo.p21_view_customer c ON c.customer_id = ih.customer_id" + System.Environment.NewLine +
                                    "  JOIN ccecn.dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no" + System.Environment.NewLine +
                                    "  JOIN ccecn.dbo.p21_view_supplier s ON s.supplier_id = il.supplier_id" + System.Environment.NewLine +
                                    "  WHERE ih.invoice_date > '8/3/21 00:00:00'" + System.Environment.NewLine +
                                    "  AND (ih.po_no LIKE 'ECN21%')" + System.Environment.NewLine +
                                    "  AND il.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  -- AND [week] = 1" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  )" + System.Environment.NewLine +
                                    "  GROUP BY ih.invoice_no" + System.Environment.NewLine +
                                    "  , ih.invoice_date" + System.Environment.NewLine +
                                    "  , ih.po_no" + System.Environment.NewLine +
                                    "  , ih.customer_id" + System.Environment.NewLine +
                                    "  , c.customer_name" + System.Environment.NewLine +
                                    "  , il.supplier_id" + System.Environment.NewLine +
                                    "  , s.supplier_name" + System.Environment.NewLine +
                                    "  ) vsresults;" + System.Environment.NewLine +
                                    "  SELECT customer_id" + System.Environment.NewLine +
                                    "  , customer_name" + System.Environment.NewLine +
                                    "  , SUM(amt) showtot" + System.Environment.NewLine +
                                    "  FROM #vstable" + System.Environment.NewLine +
                                    "  WHERE (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-BO%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W1%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 1" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W2%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 2" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W3%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 3" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W4%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 4" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W5%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 5" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W6%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 6" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W7%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 7" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  OR" + System.Environment.NewLine +
                                    "  (" + System.Environment.NewLine +
                                    "  po_no LIKE 'ECN21-W8%'" + System.Environment.NewLine +
                                    "  AND #vstable.supplier_id IN (" + System.Environment.NewLine +
                                    "  SELECT DISTINCT" + System.Environment.NewLine +
                                    "  supplier_id" + System.Environment.NewLine +
                                    "  FROM {dashboard}.dbo.[vshowdisc]" + System.Environment.NewLine +
                                    "  WHERE [year] = 2021" + System.Environment.NewLine +
                                    "  AND [week] = 8" + System.Environment.NewLine +
                                    "  AND delete_flag = 0" + System.Environment.NewLine +
                                    "  ))" + System.Environment.NewLine +
                                    "  GROUP BY customer_id" + System.Environment.NewLine +
                                    "  , customer_name" + System.Environment.NewLine +
                                    "  ORDER BY customer_name";

            // 📌 Replace placeholder with appsettings.json value
            sql = sql.Replace("{dashboard}", dashboard);

            try
            {
                con.Open();

                using var cmd = new SqlCommand(sql, con);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new CustomerTotals
                    {
                        CustomerId = reader["customer_id"]?.ToString() ?? "",
                        CustomerName = reader["customer_name"]?.ToString() ?? "",
                        ShowTotal = reader["showtot"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["showtot"]): 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CustomerTotals Error: " + ex.Message);
            }

            return list;
        }
    }
}
