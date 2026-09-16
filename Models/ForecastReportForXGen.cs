using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace ECNREPORTAPI.Models
{
    public class ForecastReportForXGenModel
    {
        public string item_id { get; set; } = string.Empty;
        public string item_desc { get; set; } = string.Empty;
        public decimal cost { get; set; }
        public decimal qty_on_hand { get; set; }
        public decimal order_quantity { get; set; }
        public decimal qty_allocated { get; set; }
        public string release_date { get; set; } = string.Empty;
        public decimal AVG { get; set; }
        public decimal years_usage { get; set; }
        public string primary_bin { get; set; } = string.Empty;
    }

    public class ForecastReportForXGenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _dashboard;

        public ForecastReportForXGenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _dashboard = _configuration["DASHBOARD"] ?? "DashboardDB";
        }

        public async Task<List<ForecastReportForXGenModel>> GetDataAsync(string compId, string locationid, string supplierId, string prefix)
        {
            var list = new List<ForecastReportForXGenModel>();

            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "forecastreportforxgen.sql");
                if (!File.Exists(filePath)) return list;

                string query = await File.ReadAllTextAsync(filePath);
                query = query.Replace("{dashboard}", _dashboard);

                Common obj_gd = new Common(_configuration);
                string conStr = string.IsNullOrWhiteSpace(compId) 
                    ? obj_gd.ConStr 
                    : obj_gd.GetDataBaseConnectionStringHardCoded(compId);

                int commandTimeout = int.TryParse(_configuration["SqlCommandTimeOut"], out int timeout) ? timeout : 30;

                DateTime current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                int startMonth = current.Month - 1;

                int cnt = 1;
                string curYearMonths = "";
                string curYear;
                int m;
                string prevYearMonths = "";
                string prevYear = "";
                string subqueryP = "";

                m = startMonth;
                curYear = current.AddMonths(-1).ToString("yyyy");
                curYearMonths = m.ToString();
                m--;

                while (cnt <= 6 && m >= 1)
                {
                    curYearMonths = m.ToString() + "," + curYearMonths;
                    m--;
                    cnt++;
                }

                if (cnt < 6)
                {
                    m = 12;
                    prevYearMonths = m.ToString();
                    cnt++;
                    prevYear = current.AddMonths(-cnt).ToString("yyyy");
                    while (cnt < 6)
                    {
                        m--;
                        prevYearMonths = m.ToString() + "," + prevYearMonths;
                        cnt++;
                    }
                }

                subqueryP = " ((p.period IN(" + curYearMonths + ") AND p.year_for_period = " + curYear + ") ";
                if (!string.IsNullOrEmpty(prevYear))
                {
                    subqueryP += "or (p.period IN(" + prevYearMonths + ") AND p.year_for_period = " + prevYear + " ) ) ";
                }
                else
                {
                    subqueryP += ")";
                }

                query = query.Replace("{subqueryP}", subqueryP);

                using (var con = new SqlConnection(conStr))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@locationid", string.IsNullOrEmpty(locationid) ? "ALL" : locationid);
                    parameters.Add("@SupplierId", string.IsNullOrEmpty(supplierId) ? "ALL" : supplierId);
                    parameters.Add("@prefix", string.IsNullOrEmpty(prefix) ? "" : prefix);

                    var result = await con.QueryAsync<ForecastReportForXGenModel>(query, parameters, commandTimeout: commandTimeout);
                    list = result.AsList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occured (ForecastReportForXGen): " + ex.ToString());
            }

            return list;
        }
    }
}