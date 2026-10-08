using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class AccountSuppressedFromAgingCollectionModel
    {
        private readonly IConfiguration _config;

        public AccountSuppressedFromAgingCollectionModel(IConfiguration config)
        {
            _config = config;
        }

        public string customer_id { get; set; } = "";
        public string customer_name { get; set; } = "";
        public string salesrep_id { get; set; } = "";
        public string Rep { get; set; } = "";
        public string B1 { get; set; } = "";
        public string B2 { get; set; } = "";
        public string B3 { get; set; } = "";
        public string B4 { get; set; } = "";
        public string Total_Due { get; set; } = "";

        public List<AccountSuppressedFromAgingCollectionModel> GetData(string compId)
        {
            var list = new List<AccountSuppressedFromAgingCollectionModel>();

            try
            {
                Common obj_gd = new(_config);
                string conStr = obj_gd.GetDataBaseConnectionStringHardCoded(compId);
                
                string subquery = "";
                if (compId.Equals("ECN", StringComparison.OrdinalIgnoreCase))
                {
                    subquery = " AND customer.salesrep_id in (2734, 1636, 5148, 5184, 5293, 5294, 5305, 5325, 6031, 7127, 7128, 7153)";
                }
                else if (compId.Equals("IVD", StringComparison.OrdinalIgnoreCase))
                {
                    subquery = " AND customer.salesrep_id in (1672,5313,4016,4027,4364,5312,1010,4029,4028)";
                }

                string strSQL = $@"
                    SELECT customer.customer_id
                    , customer.customer_name
                    , customer.salesrep_id
                    , contacts.first_name + ' ' + contacts.last_name + ' - ' + customer.salesrep_id AS Rep
                    , SUM(CASE WHEN datediff(dd,invoice_hdr.invoice_date, getdate())<= 30 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END) as B1
                    , SUM(CASE WHEN datediff(dd,invoice_hdr.invoice_date, getdate())>30 AND datediff(dd,invoice_hdr.invoice_date, getdate()) <=60 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END) as B2
                    , SUM(CASE WHEN datediff(dd,invoice_hdr.invoice_date, getdate())>60 AND datediff(dd,invoice_hdr.invoice_date, getdate()) <=90 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END) as B3
                    , SUM(CASE WHEN datediff(dd,invoice_hdr.invoice_date, getdate())>90 THEN p21_invoice_amt_remaining_view.amt_remaining_frominv ELSE 0 END) as B4
                    , SUM(p21_invoice_amt_remaining_view.amt_remaining_frominv) as Total_Due
                    FROM customer with (nolock) 
                    join contacts with (nolock) on customer.salesrep_id = contacts.id 
                    join invoice_hdr with (nolock) on customer.customer_id = invoice_hdr.customer_id 
                    join p21_invoice_amt_remaining_view with (nolock) on p21_invoice_amt_remaining_view.invoice_no = invoice_hdr.invoice_no
                    Where invoice_hdr.paid_in_full_flag = 'N'
                    {subquery}
                    GROUP BY customer.customer_id, customer.customer_name, contacts.first_name, contacts.last_name, customer.salesrep_id
                    Order by contacts.last_name, customer_name";

                using var con = new SqlConnection(conStr);
                con.Open();
                using var cmd = new SqlCommand(strSQL, con);
                cmd.CommandTimeout = 300;

                using var adap = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adap.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    var val = new AccountSuppressedFromAgingCollectionModel(_config)
                    {
                        customer_id = dr["customer_id"]?.ToString() ?? "",
                        customer_name = dr["customer_name"]?.ToString() ?? "",
                        salesrep_id = dr["salesrep_id"]?.ToString() ?? "",
                        Rep = dr["Rep"]?.ToString() ?? "",
                        B1 = dr["B1"]?.ToString() ?? "",
                        B2 = dr["B2"]?.ToString() ?? "",
                        B3 = dr["B3"]?.ToString() ?? "",
                        B4 = dr["B4"]?.ToString() ?? "",
                        Total_Due = dr["Total_Due"]?.ToString() ?? ""
                    };
                    list.Add(val);

                    var val1 = new AccountSuppressedFromAgingCollectionModel(_config)
                    {
                        customer_id = "",
                        customer_name = "",
                        salesrep_id = "",
                        Rep = "",
                        B1 = !string.IsNullOrEmpty(val.B1) && Convert.ToDouble(val.Total_Due) != 0 ? (Convert.ToDouble(val.B1) / Convert.ToDouble(val.Total_Due)).ToString() : "",
                        B2 = !string.IsNullOrEmpty(val.B2) && Convert.ToDouble(val.Total_Due) != 0 ? (Convert.ToDouble(val.B2) / Convert.ToDouble(val.Total_Due)).ToString() : "",
                        B3 = !string.IsNullOrEmpty(val.B3) && Convert.ToDouble(val.Total_Due) != 0 ? (Convert.ToDouble(val.B3) / Convert.ToDouble(val.Total_Due)).ToString() : "",
                        B4 = !string.IsNullOrEmpty(val.B4) && Convert.ToDouble(val.Total_Due) != 0 ? (Convert.ToDouble(val.B4) / Convert.ToDouble(val.Total_Due)).ToString() : "",
                        Total_Due = ""
                    };
                    list.Add(val1);
                }
                if (dt.Rows.Count > 0)
                {
                    var valtot = new AccountSuppressedFromAgingCollectionModel(_config)
                    {
                        customer_id = "",
                        customer_name = "",
                        salesrep_id = "",
                        Rep = "Total:",
                        B1 = Convert.ToDouble(dt.Compute("SUM(B1)", string.Empty)).ToString(),
                        B2 = Convert.ToDouble(dt.Compute("SUM(B2)", string.Empty)).ToString(),
                        B3 = Convert.ToDouble(dt.Compute("SUM(B3)", string.Empty)).ToString(),
                        B4 = Convert.ToDouble(dt.Compute("SUM(B4)", string.Empty)).ToString(),
                        Total_Due = Convert.ToDouble(dt.Compute("SUM(Total_Due)", string.Empty)).ToString()
                    };
                    list.Add(valtot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return list;
        }
    }
}