using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

namespace ECNREPORTAPI.Models
{
    public class CreditHoldsWithReleaseService
    {
        private readonly IConfiguration _config;
        private readonly string _dashboard;

        public CreditHoldsWithReleaseService(IConfiguration config)
        {
            _config = config;
            _dashboard = _config["DASHBOARD"] ?? "";
        }
        public async Task<Dictionary<string, List<Dictionary<string, object>>>> GetDataAsync()
        {
            var result = new Dictionary<string, List<Dictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);
            var common = new Common(_config);

            string companyList = _config["CMPCODELIST"] ?? "ADV,ECN,IVD,XG";
            string[] companies = companyList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            const string sql = @"
                        SELECT 
                            oe_hdr.customer_id,
                            customer.customer_name,
                            terms.terms_desc,
                            a.terms_desc AS orderHeaderTerms,
                            credit_limit,
                            credit_limit_used,
                            customer.credit_status,
                            oe_hdr.order_no,
                            oe_hdr.order_date,
                            SUM((oe_line.qty_ordered - oe_line.qty_invoiced - oe_line.qty_canceled) / oe_line.pricing_unit_size * oe_line.unit_price) AS order_total,
                            oe_hdr.validation_status,
                            oe_hdr.job_name,
                            oe_hdr.po_no,
                            first_name + ' ' + last_name AS salesrep,
                            DATEDIFF(MINUTE, oe_hdr.date_created, GETDATE()) AS Time_In_Q,
                            ISNULL(da_aging.b1, 0) AS b1,
                            ISNULL(da_aging.b2, 0) AS b2,
                            ISNULL(da_aging.b3, 0) AS b3,
                            ISNULL(da_aging.b4, 0) AS b4,
                            ISNULL(da_aging.tot, 0) AS tot
                        FROM oe_hdr WITH (NOLOCK)
                        JOIN oe_line WITH (NOLOCK) 
                            ON oe_line.order_no = oe_hdr.order_no
                        AND oe_line.complete = 'N'
                        AND oe_line.parent_oe_line_uid = 0
                        JOIN customer WITH (NOLOCK) 
                            ON customer.customer_id = oe_hdr.customer_id
                        AND customer.company_id = oe_hdr.company_id
                        JOIN contacts WITH (NOLOCK) ON customer.salesrep_id = contacts.id
                        JOIN terms WITH (NOLOCK) ON customer.terms_id = terms.terms_id
                        JOIN terms a WITH (NOLOCK) ON oe_hdr.terms = a.terms_id
                        LEFT OUTER JOIN da_aging ON da_aging.customer_id = oe_hdr.customer_id
                        WHERE (oe_hdr.validation_status = 'Hold' OR oe_hdr.validation_status = 'COD')
                        AND oe_hdr.completed = 'N'
                        AND oe_hdr.approved = 'Y'
                        AND oe_hdr.delete_flag = 'N'
                        AND oe_hdr.projected_order = 'N'
                        GROUP BY 
                            oe_hdr.order_no, oe_hdr.order_date, oe_hdr.date_created,
                            oe_hdr.date_last_modified, oe_hdr.last_maintained_by,
                            oe_hdr.validation_status, oe_hdr.oe_hdr_uid, oe_hdr.customer_id,
                            oe_hdr.approved, oe_hdr.taker, oe_hdr.source_location_id,
                            oe_hdr.location_id, oe_hdr.company_id, oe_hdr.terms,
                            customer.customer_name, oe_hdr.job_name, oe_hdr.job_price_hdr_uid,
                            oe_hdr.po_no, first_name + ' ' + last_name, terms.terms_desc,
                            credit_limit, credit_limit_used, customer.credit_status,
                            a.terms_desc, da_aging.b1, da_aging.b2, da_aging.b3, da_aging.b4, da_aging.tot
                        ORDER BY customer.customer_name, oe_hdr.order_no";

            foreach (var comp in companies)
            {
                try
                {
                    string conStr = common.GetDataBaseConnectionStringHardCoded(comp);
                    if (string.IsNullOrWhiteSpace(conStr)) continue;

                    string p21Url = "";
                    using (var dashCon = new SqlConnection(common.ConStr))
                    {
                        p21Url = await dashCon.QueryFirstOrDefaultAsync<string>(
                            $"SELECT p21ApiURL FROM {_dashboard}.dbo.datasources WHERE dsource = @comp OR code = @comp",
                            new { comp }) ?? "";
                    }

                    using var con = new SqlConnection(conStr);
                    var rows = (await con.QueryAsync(sql, commandTimeout: 300)).ToList();

                    var list = new List<Dictionary<string, object>>();
                    foreach (var row in rows)
                    {
                        var d = (IDictionary<string, object>)row;
                        list.Add(new Dictionary<string, object>
                        {
                            ["comp_id"]          = comp,
                            ["customer_id"]      = d["customer_id"]?.ToString() ?? "",
                            ["customer_name"]    = d["customer_name"]?.ToString() ?? "",
                            ["terms_desc"]       = d["terms_desc"]?.ToString() ?? "",
                            ["orderHeaderTerms"] = d["orderHeaderTerms"]?.ToString() ?? "",
                            ["credit_limit"]     = d["credit_limit"] ?? 0,
                            ["credit_limit_used"]= d["credit_limit_used"] ?? 0,
                            ["credit_status"]    = d["credit_status"]?.ToString() ?? "",
                            ["order_no"]         = d["order_no"]?.ToString() ?? "",
                            ["order_date"]       = d["order_date"],
                            ["order_total"]      = d["order_total"] ?? 0,
                            ["validation_status"]= d["validation_status"]?.ToString() ?? "",
                            ["job_name"]         = d["job_name"]?.ToString() ?? "",
                            ["po_no"]            = d["po_no"]?.ToString() ?? "",
                            ["salesrep"]         = d["salesrep"]?.ToString() ?? "",
                            ["Time_In_Q"]        = d["Time_In_Q"] ?? 0,
                            ["b1"]               = d["b1"] ?? 0,
                            ["b2"]               = d["b2"] ?? 0,
                            ["b3"]               = d["b3"] ?? 0,
                            ["b4"]               = d["b4"] ?? 0,
                            ["tot"]              = d["tot"] ?? 0,
                            ["url"]              = p21Url
                        });
                    }

                    if (list.Count > 0)
                        result[comp.ToUpper()] = list;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CreditHoldsWithRelease [{comp}]: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<string> ValidateAndReleaseAsync(string itemsList)
        {
            if (string.IsNullOrWhiteSpace(itemsList))
                return "No orders selected.";

            var sb = new StringBuilder();
            var p21 = new P21ApiHelper(_config);
            string lastComp = "";
            P21TokenInfo? token = null;

            foreach (var item in itemsList.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split('#');
                if (parts.Length < 3) continue;

                string compId     = parts[0].Trim();
                string customerId = parts[1].Trim();
                string orderNo    = parts[2].Trim();

                try
                {
                    if (token == null || !compId.Equals(lastComp, StringComparison.OrdinalIgnoreCase))
                    {
                        token = await p21.GetTokenAsync(compId);
                        lastComp = compId;

                        if (token == null || string.IsNullOrEmpty(token.Token))
                        {
                            sb.AppendLine($"{compId}: API URL / credentials not set");
                            continue;
                        }
                    }

                    sb.AppendLine(await p21.ValidateOrderAsync(compId, token, customerId, orderNo));
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"{compId}: Order # {orderNo} Error – {ex.Message}");
                }
            }

            return sb.ToString().Trim();
        }
    }
}