using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class AfterHoursUsersReport
    {
        public string Loginname { get; set; } = "";
        public string NtUsername { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string DbId { get; set; } = "";
        public string DataDate { get; set; } = "";
        public string DateLoggedIn { get; set; } = "";
        public string ClientNetAddress { get; set; } = "";
        public string Hostname { get; set; } = "";

        /// <summary>
        /// Fetch after hours users report from dashboard database
        /// </summary>
        public static async Task<List<AfterHoursUsersReport>> GetDataAsync(
            IConfiguration config,
            string? fromDate = null,
            string? tillDate = null,
            string? timePeriod = null)
        {
            var list = new List<AfterHoursUsersReport>();

            // Get dashboard connection string
            string conStr = config.GetConnectionString("strCon_dashboard")
                ?? throw new InvalidOperationException("Missing connection string: strCon_dashboard");

            // Time period logic
            if (!string.IsNullOrWhiteSpace(timePeriod) && timePeriod != "Time Period")
            {
                var pd = Common.getPeriod(timePeriod);
                fromDate = pd.from_date;
                tillDate = pd.till_date;
            }

            string sql = @"
                SELECT 
                    [Loginname],
                   ,[nt_username]
                   ,[database_name]
                   ,[dbid]
                   ,[datadate]
                   ,[dateloggedin]
                   ,[client_net_address]
                   ,[hostname]
                FROM dbo.dailyuserloggedindetails WITH (NOLOCK)
                WHERE hostname NOT IN ('ECNP21SCHED')
            ";

            if (!string.IsNullOrWhiteSpace(fromDate) && !string.IsNullOrWhiteSpace(tillDate))
            {
                sql += " AND datadate BETWEEN @fromDate AND @tillDate ";
            }

            sql += " ORDER BY Loginname, datadate DESC";

            try
            {
                await using var con = new SqlConnection(conStr);
                await using var cmd = new SqlCommand(sql, con);

                if (!string.IsNullOrWhiteSpace(fromDate) && !string.IsNullOrWhiteSpace(tillDate))
                {
                    cmd.Parameters.Add("@fromDate", SqlDbType.DateTime)
                        .Value = DateTime.Parse(fromDate + " 00:00:00");

                    cmd.Parameters.Add("@tillDate", SqlDbType.DateTime)
                        .Value = DateTime.Parse(tillDate + " 23:59:59");
                }

                await con.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new AfterHoursUsersReport
                    {
                        Loginname        = reader["Loginname"]?.ToString() ?? "",
                        NtUsername       = reader["nt_username"]?.ToString() ?? "",
                        DatabaseName     = reader["database_name"]?.ToString() ?? "",
                        DbId             = reader["dbid"]?.ToString() ?? "",
                        DataDate         = reader["datadate"]?.ToString() ?? "",
                        DateLoggedIn     = reader["dateloggedin"]?.ToString() ?? "",
                        ClientNetAddress = reader["client_net_address"]?.ToString() ?? "",
                        Hostname         = reader["hostname"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AfterHoursUsersReport Error: " + ex.Message);
            }

            return list;
        }
    }
}
