using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ECNREPORTAPI.Models
{
    public class IncludeProfitfive
    {
        public string customer_id { get; set; } = string.Empty;
        public string customer_name { get; set; } = string.Empty;
        public string rep { get; set; } = string.Empty;
        public string terms_desc { get; set; } = string.Empty;
        public string last_sales_date { get; set; } = string.Empty;
        public string year5 { get; set; } = "0";
        public string year5_GP_PCT { get; set; } = "0";
        public string year4 { get; set; } = "0";
        public string year4_GP_PCT { get; set; } = "0";
        public string year3 { get; set; } = "0";
        public string year3_GP_PCT { get; set; } = "0";
        public string year2 { get; set; } = "0";
        public string year2_GP_PCT { get; set; } = "0";
        public string year1 { get; set; } = "0";
        public string year1_GP_PCT { get; set; } = "0";
        public string year0 { get; set; } = "0";
        public string year0_GP_PCT { get; set; } = "0";
        public string sf_account_id { get; set; } = string.Empty;
    }

    public class FiveYearSalesIncludeProfitResponse
    {
        public List<IncludeProfitfive> c1 { get; set; } = new List<IncludeProfitfive>();
        public int curryear { get; set; }
    }

    public class FiveYearSalesIncludeProfit
    {
        private readonly IConfiguration _configuration;
        private readonly string _dashboard;

        public FiveYearSalesIncludeProfit(IConfiguration configuration)
        {
            _configuration = configuration;
            _dashboard = _configuration["DASHBOARD"] ?? "DashboardDB";
        }

        public async Task<FiveYearSalesIncludeProfitResponse> GetDataAsync(string Comp_id, string rep_id)
        {
            var list = new FiveYearSalesIncludeProfitResponse
            {
                curryear = DateTime.Today.Year
            };

            int[] year = new int[6];
            year[0] = list.curryear;
            year[1] = year[0] - 1;
            year[2] = year[0] - 2;
            year[3] = year[0] - 3;
            year[4] = year[0] - 4;
            year[5] = year[0] - 5;

            string[] year_c = new string[6];
            for (int i = 0; i < 6; i++)
            {
                year_c[i] = year[i] + "_c";
            }

            bool hasRepFilter = !string.IsNullOrEmpty(rep_id) && !rep_id.Equals("ALL", StringComparison.OrdinalIgnoreCase);
            string subQueryrep = hasRepFilter ? "WHERE rep.salesrep_id = @rep_id" : "";

            Common obj_gd = new Common(_configuration);
            string conStr = string.IsNullOrWhiteSpace(Comp_id)
                ? obj_gd.ConStr
                : obj_gd.GetDataBaseConnectionStringHardCoded(Comp_id);

            int commandTimeout = int.TryParse(_configuration["SqlCommandTimeOut"], out int timeout) ? timeout : 30;

            string query = $@"
                SELECT customer_id
                  , customer_name
                  , rep
                  , terms_desc
                  , LastSl as last_sales_date
                  , SUM(ISNULL([{year[5]}], 0)) AS year5
                  , CASE WHEN SUM(ISNULL([{year[5]}], 0)) = 0 THEN 0 ELSE ((SUM(ISNULL([{year[5]}], 0)) - SUM(ISNULL([{year_c[5]}], 0))) / SUM(ISNULL([{year[5]}], 0))) * 100 END AS [year5_GP_PCT]
                  , SUM(ISNULL([{year[4]}], 0)) AS year4
                  , CASE WHEN SUM(ISNULL([{year[4]}], 0)) = 0 THEN 0 ELSE ((SUM(ISNULL([{year[4]}], 0)) - SUM(ISNULL([{year_c[4]}], 0))) / SUM(ISNULL([{year[4]}], 0))) * 100 END AS [year4_GP_PCT]
                  , SUM(ISNULL([{year[3]}], 0)) AS year3
                  , CASE WHEN SUM(ISNULL([{year[3]}], 0)) = 0 THEN 0 ELSE ((SUM(ISNULL([{year[3]}], 0)) - SUM(ISNULL([{year_c[3]}], 0))) / SUM(ISNULL([{year[3]}], 0))) * 100 END AS [year3_GP_PCT]
                  , SUM(ISNULL([{year[2]}], 0)) AS year2
                  , CASE WHEN SUM(ISNULL([{year[2]}], 0)) = 0 THEN 0 ELSE ((SUM(ISNULL([{year[2]}], 0)) - SUM(ISNULL([{year_c[2]}], 0))) / SUM(ISNULL([{year[2]}], 0))) * 100 END AS [year2_GP_PCT]
                  , SUM(ISNULL([{year[1]}], 0)) AS year1
                  , CASE WHEN SUM(ISNULL([{year[1]}], 0)) = 0 THEN 0 ELSE ((SUM(ISNULL([{year[1]}], 0)) - SUM(ISNULL([{year_c[1]}], 0))) / SUM(ISNULL([{year[1]}], 0))) * 100 END AS [year1_GP_PCT]
                  , SUM(ISNULL([{year[0]}], 0)) AS year0
                  , CASE WHEN SUM(ISNULL([{year[0]}], 0)) = 0 THEN 0 ELSE ((SUM(ISNULL([{year[0]}], 0)) - SUM(ISNULL([{year_c[0]}], 0))) / SUM(ISNULL([{year[0]}], 0))) * 100 END AS [year0_GP_PCT]
                  , sf_account_id
                  FROM (
                      SELECT c.customer_id
                      , c.customer_name
                      , t.terms_desc
                      , rep
                      , c.delete_flag
                      , sls.yr
                      , CONVERT(varchar, sls.yr) + '_c' as yr2
                      , sls.Tot
                      , sls.COGS
                      , cs.LastSl
                      , sf.sf_account_id
                      FROM da_cust_sls_history sls
                      JOIN p21_view_customer c ON c.customer_id = sls.customer_id
                      JOIN DA_Rep rep ON rep.customer_id = c.customer_id
                      JOIN p21_view_terms t ON t.terms_id = c.terms_id
                      LEFT OUTER JOIN dbo.DA_Cust_Stats_Static cs ON cs.customer_id = c.customer_id
                      LEFT JOIN {_dashboard}.dbo.p21_customer_sf_account_map sf (nolock) ON sf.customer_id = sls.customer_id
                      {subQueryrep}
                  ) AS saleshist
                PIVOT( SUM(Tot)
                  FOR yr IN([{year[5]}], [{year[4]}], [{year[3]}], [{year[2]}], [{year[1]}], [{year[0]}])
                ) AS pvt
                PIVOT( SUM(COGS)
                  FOR yr2 IN([{year_c[5]}], [{year_c[4]}], [{year_c[3]}], [{year_c[2]}], [{year_c[1]}], [{year_c[0]}])
                ) AS pvt2
                GROUP BY customer_id
                  , customer_name
                  , rep
                  , terms_desc
                  , LastSl
                  , sf_account_id
                ORDER BY customer_name";

            try
            {
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandTimeout = commandTimeout;
                        if (hasRepFilter)
                        {
                            cmd.Parameters.AddWithValue("@rep_id", rep_id);
                        }

                        await con.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var val = new IncludeProfitfive
                                {
                                    customer_id = reader["customer_id"]?.ToString() ?? string.Empty,
                                    customer_name = reader["customer_name"]?.ToString() ?? string.Empty,
                                    rep = reader["rep"]?.ToString() ?? string.Empty,
                                    terms_desc = reader["terms_desc"]?.ToString() ?? string.Empty,
                                    last_sales_date = reader["last_sales_date"]?.ToString() ?? string.Empty,
                                    year5 = reader["year5"]?.ToString() ?? "0",
                                    year5_GP_PCT = reader["year5_GP_PCT"]?.ToString() ?? "0",
                                    year4 = reader["year4"]?.ToString() ?? "0",
                                    year4_GP_PCT = reader["year4_GP_PCT"]?.ToString() ?? "0",
                                    year3 = reader["year3"]?.ToString() ?? "0",
                                    year3_GP_PCT = reader["year3_GP_PCT"]?.ToString() ?? "0",
                                    year2 = reader["year2"]?.ToString() ?? "0",
                                    year2_GP_PCT = reader["year2_GP_PCT"]?.ToString() ?? "0",
                                    year1 = reader["year1"]?.ToString() ?? "0",
                                    year1_GP_PCT = reader["year1_GP_PCT"]?.ToString() ?? "0",
                                    year0 = reader["year0"]?.ToString() ?? "0",
                                    year0_GP_PCT = reader["year0_GP_PCT"]?.ToString() ?? "0",
                                    sf_account_id = reader["sf_account_id"]?.ToString() ?? string.Empty
                                };
                                list.c1.Add(val);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occured (FiveYearSalesIncludeProfit): " + ex.ToString());
            }

            return list;
        }
    }
}