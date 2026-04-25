using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public sealed class Common(IConfiguration config)
    {
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));

        public string ConStr => 
            _config.GetConnectionString("strCon") 
            ?? throw new InvalidOperationException("Missing connection string: strCon");

        public string ConStr_Dashboard => 
            _config.GetConnectionString("strCon_dashboard") 
            ?? throw new InvalidOperationException("Missing connection string: strCon_dashboard");

        /// <summary>
        /// Returns company-specific connection string by replacing database prefix
        /// </summary>
        public string GetDataBaseConnectionStringHardCoded(string? compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return ConStr;

            return compId.Trim().ToLowerInvariant() switch
            {
                "abc"     => ConStr.Replace("ccecn", "ccabc",     StringComparison.OrdinalIgnoreCase),
                "adv"     => ConStr.Replace("ccecn", "ccad",      StringComparison.OrdinalIgnoreCase),
                "baci"    => ConStr.Replace("ccecn", "ccbaci",    StringComparison.OrdinalIgnoreCase),
                "ecn"     => ConStr.Replace("ccecn", "ccecn",     StringComparison.OrdinalIgnoreCase),
                "ivd"     => ConStr.Replace("ccecn", "ccivd",     StringComparison.OrdinalIgnoreCase),
                "ovo"     => ConStr.Replace("ccecn", "ccovo",     StringComparison.OrdinalIgnoreCase),
                "ple" or "pp" => ConStr.Replace("ccecn", "ccpleasure", StringComparison.OrdinalIgnoreCase),
                "ppm"     => ConStr.Replace("ccecn", "ccppm",     StringComparison.OrdinalIgnoreCase),
                "pure"    => ConStr.Replace("ccecn", "ccppmold",  StringComparison.OrdinalIgnoreCase),
                "vv"      => ConStr.Replace("ccecn", "ccvv",      StringComparison.OrdinalIgnoreCase),
                "xg"      => ConStr.Replace("ccecn", "ccxg",      StringComparison.OrdinalIgnoreCase),
                _         => ConStr
            };
        }


 // =========================================================
        // ================= PERIOD SECTION (LEGACY SAFE) ==========
        // =========================================================

        public class PeriodList
        {
            public string Period { get; set; } = "";
            public string PeriodStartDate { get; set; } = "";
            public string PeriodEndDate { get; set; } = "";
        }

        /// <summary>
        /// Last 12 months list (MMM-yyyy)
        /// </summary>
        public List<PeriodList> gePeriodList()
        {
            List<PeriodList> list = new();

            try
            {
                DateTime current = new(DateTime.Now.Year, DateTime.Now.Month, 1);

                for (int n = 1; n <= 12; n++)
                {
                    list.Add(new PeriodList
                    {
                        Period = current.AddMonths(-n).ToString("MMM-yyyy"),
                        PeriodStartDate = current.AddMonths(-n).ToString("yyyy-MM-dd"),
                        PeriodEndDate = current.AddMonths(-n + 1).AddDays(-1).ToString("yyyy-MM-dd")
                    });
                }
            }
            catch { }

            return list;
        }

        public struct PeriodDate
        {
            public string from_date { get; set; }
            public string till_date { get; set; }
        }

        /// <summary>
        /// OLD getPeriod logic – untouched
        /// </summary>
        public static PeriodDate getPeriod(string? t_period)
        {
            string from_date = "", till_date = "";
            PeriodDate PD = new();

            if (!string.IsNullOrWhiteSpace(t_period) && !t_period.Equals("Time Period"))
            {
                string[] arr = t_period.Split('-');
                string period = arr.Length >= 3 ? arr[2] : t_period;

                DateTime cur = DateTime.Now;
                DateTime firstMonth = new(cur.Year, cur.Month, 1);
                DateTime firstYear = new(cur.Year, 1, 1);

                switch (period.ToUpper())
                {
                    case "TODAY":
                        from_date = cur.ToShortDateString();
                        till_date = cur.ToShortDateString();
                        break;

                    case "YESTERDAY":
                        from_date = cur.AddDays(-1).ToShortDateString();
                        till_date = cur.AddDays(-1).ToShortDateString();
                        break;

                    case "THIS WEEK":
                        int diff = (int)cur.DayOfWeek;
                        from_date = cur.AddDays(-diff).ToShortDateString();
                        till_date = cur.ToShortDateString();
                        break;

                    case "MONTH TO DATE":
                        from_date = firstMonth.ToShortDateString();
                        till_date = cur.ToShortDateString();
                        break;

                    case "LAST MONTH":
                        from_date = firstMonth.AddMonths(-1).ToShortDateString();
                        till_date = firstMonth.AddDays(-1).ToShortDateString();
                        break;

                    case "YEAR TO DATE":
                        from_date = firstYear.ToShortDateString();
                        till_date = cur.ToShortDateString();
                        break;

                    case "LAST YEAR":
                        from_date = firstYear.AddYears(-1).ToShortDateString();
                        till_date = firstYear.AddDays(-1).ToShortDateString();
                        break;
                }

                PD.from_date = from_date;
                PD.till_date = till_date;
            }

            return PD;
        }

        /// <summary>
        /// Authenticate user against Active Directory (Windows only)
        /// </summary>
        public bool AuthenticateUserAD(string username, string password, out string errMsg)
        {
            errMsg = string.Empty;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                errMsg = "Active Directory authentication is only supported on Windows.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errMsg = "Username or password cannot be empty.";
                return false;
            }

            string domain = _config["ActiveDirectory:Domain"] ?? "IVD1";
            string path   = _config["ActiveDirectory:Path"]   ?? "LDAP://DCDP01.ivd1.local";
            string fullUser = $@"{domain}\{username}";

            try
            {
                using var entry = new DirectoryEntry(path, fullUser, password, AuthenticationTypes.Secure);
                _ = entry.NativeObject; // Forces authentication
                return true;
            }
            catch (COMException) // Most common: bad credentials
            {
                errMsg = "Invalid username or password.";
                return false;
            }
            catch (Exception ex)
            {
                errMsg = "Authentication failed: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Get user from unified portal database
        /// </summary>
        public users_unifiedportal? GetUserFromDB(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            const string sql = """
                SELECT TOP 1 
                    UserId, Username, Name, Email, IsActive, IsAdmin 
                FROM users_unifiedportal 
                WHERE LOWER(Username) = @username
                """;

            using var con = new SqlConnection(ConStr_Dashboard);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = username.ToLowerInvariant();

            con.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new users_unifiedportal(
                UserId:   reader.GetInt32(reader.GetOrdinal("UserId")),
                Username: reader.GetString(reader.GetOrdinal("Username")),
                Name:     reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name")),
                Email:    reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                IsActive: reader.GetBoolean(reader.GetOrdinal("IsActive")),
                IsAdmin:  reader.GetBoolean(reader.GetOrdinal("IsAdmin"))
            );
        }

        /// <summary>
        /// Log successful login
        /// </summary>
        public void LogLogin(int userId, string ip)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(ip))
                return;

            const string sql = """
                INSERT INTO UPortalLoginUserHistory (UserId, LoginDateTime, LoginUserIP)
                VALUES (@userId, GETDATE(), @ip)
                """;

            try
            {
                using var con = new SqlConnection(ConStr_Dashboard);
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@ip", SqlDbType.NVarChar, 45).Value = ip.Trim();

                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // In real app: use ILogger
                System.Diagnostics.Debug.WriteLine($"[LogLogin] Failed: {ex.Message}");
            }
        }
    }

    // Modern, clean DTO — perfect for this use case
    public record users_unifiedportal(
        int UserId,
        string Username,
        string Name,
        string? Email,
        bool IsActive,
        bool IsAdmin
    );

    
}