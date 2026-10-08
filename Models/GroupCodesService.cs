using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Services
{
    public class GroupCodesService
    {
        private readonly IConfiguration _config;
        private readonly string _dashboard;

        public GroupCodesService(IConfiguration config)
        {
            _config = config;
            _dashboard = _config["DASHBOARD"] ?? "";
        }

        public async Task<Dictionary<string, List<Dictionary<string, object>>>> GetDataAsync(string groupCode)
        {
            var common = new Models.Common(_config);
            string cmpListConfig = _config["CMPCODELIST"] ?? "";
            string[] cmplist = cmpListConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var groupedResult = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (string comp in cmplist)
            {
                string trimComp = comp.Trim();
                string conStr = common.GetDataBaseConnectionStringHardCoded(trimComp);
                
                try
                {
                    using var con = new SqlConnection(conStr);
                    await con.OpenAsync();

                    string sql = @"
                        SELECT DISTINCT t1.customer_id, 
                               t1.customer_name, 
                               ISNULL(t3.B1, 0) AS B1,  
                               ISNULL(t3.B2, 0) AS B2,  
                               ISNULL(t3.B3, 0) AS B3,  
                               ISNULL(t3.B4, 0) AS B4,  
                               ISNULL(t3.Tot, 0) AS tot 
                        FROM customer (nolock) t1  
                        LEFT OUTER JOIN DA_Aging t3 ON t3.customer_id = t1.customer_id 
                        INNER JOIN " + _dashboard + @".[dbo].[groupcodes] gc 
                            ON gc.customer_id = t1.customer_id 
                        WHERE gc.groupcode = @groupCode 
                          AND gc.company = @comp 
                          AND ISNULL(gc.delete_flag, 0) = 0
                        ORDER BY t1.customer_id";

                    var rows = (await con.QueryAsync<dynamic>(sql, new { groupCode, comp = trimComp })).ToList();
                    
                    var dictRows = rows.Select(x => (IDictionary<string, object>)x)
                                       .Select(d => d.ToDictionary(k => k.Key, v => v.Value))
                                       .ToList();

                    if (dictRows.Count > 0)
                    {
                        groupedResult[trimComp.ToUpper()] = dictRows;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching groupcodes for {trimComp}: {ex.Message}");
                }
            }

            return groupedResult;
        }
    }
}