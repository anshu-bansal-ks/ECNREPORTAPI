using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class AdsOrderLookupService
    {
        private readonly IConfiguration _config;
        public AdsOrderLookupService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<object> GetDataAsync(string compId, Dictionary<string, string> filters)
        {
            var common = new Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "adsorderlookup.sql");
            if (!File.Exists(filePath)) return new { Data = new List<object>(), ExcelData = new List<object>() };

            string sql = await File.ReadAllTextAsync(filePath);

            string pono = filters.GetValueOrDefault("pono", "").Trim();
            if (pono.StartsWith("0"))
            {
                pono = pono.Substring(1);
            }

            var p = new DynamicParameters();
            p.Add("pono", $"{pono}%");

            using var con = new SqlConnection(conStr);
            var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 300)).ToList();

            var formattedList = rawData.Select(row => (IDictionary<string, object>)row)
                                       .Select(dict => new Dictionary<string, object>(dict))
                                       .ToList();

            return new { Data = formattedList, ExcelData = formattedList };
        }
    }
}