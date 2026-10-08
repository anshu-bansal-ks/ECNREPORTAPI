using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class ArcallnotesService
    {
        private readonly IConfiguration _config;
        public ArcallnotesService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<object> GetDataAsync(string compId, Dictionary<string, string> filters)
        {
            var common = new Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);

            string fromDate = filters.GetValueOrDefault("fromdate", "");
            string tillDate = filters.GetValueOrDefault("tilldate", "");
            string tPeriod = filters.GetValueOrDefault("timeperiod", "");

            if (!string.IsNullOrWhiteSpace(tPeriod) && tPeriod != "Time Period")
            {
                var pd = Common.getPeriod(tPeriod);
                fromDate = pd.from_date;
                tillDate = pd.till_date;
            }

            string subquery = "";
            var p = new DynamicParameters();

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(tillDate))
            {
                subquery = "WHERE customer_call.date_last_modified BETWEEN @fromDate AND @tillDate";
                p.Add("fromDate", $"{fromDate} 00:00:00");
                p.Add("tillDate", $"{tillDate} 23:59:59");
            }

            string detailSql = $@"
                SELECT customer_call.customer_id,
                       customer.customer_name,
                       customer_call.date_last_modified,
                       customer_call.last_maintained_by,
                       customer_call.notes
                FROM customer_call WITH (NOLOCK)
                JOIN customer WITH (NOLOCK) ON customer_call.customer_id = customer.customer_id
                {subquery}
                ORDER BY customer.customer_name, customer_call.date_last_modified";

            string summarySql = $@"
                SELECT last_maintained_by,
                       COUNT(customer_call.date_last_modified) AS [COUNT]
                FROM customer_call WITH (NOLOCK)
                {subquery}
                GROUP BY last_maintained_by";

            using var con = new SqlConnection(conStr);
            using var multi = await con.QueryMultipleAsync($"{detailSql}; {summarySql}", p, commandTimeout: 300);

            var detailData = (await multi.ReadAsync<dynamic>())
                .Select(row => (IDictionary<string, object>)row)
                .Select(d => new Dictionary<string, object>(d))
                .ToList();

            var summaryData = (await multi.ReadAsync<dynamic>())
                .Select(row => (IDictionary<string, object>)row)
                .Select(d => new Dictionary<string, object>(d))
                .ToList();

            return new { 
                Data = detailData, 
                SummaryData = summaryData, 
                ExcelData = detailData 
            };
        }
    }
}