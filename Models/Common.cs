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

        public static PeriodDate getPeriod(string t_period)
        {
            string from_date = "", till_date = "";
            PeriodDate PD = new PeriodDate();

            if (!string.IsNullOrWhiteSpace(t_period) && !t_period.Equals("Time Period"))
            {
                // Purana format: "day-0-Today" ya direct "Today"
                string period = t_period;
                if (t_period.Contains("-"))
                {
                    string[] strArr = t_period.Split('-');
                    if (strArr.Length >= 3) period = strArr[2];
                }

                DateTime curdate = DateTime.Now;
                var firstDayOfMonth = new DateTime(curdate.Year, curdate.Month, 1);
                var firstDayOfYear = new DateTime(curdate.Year, 1, 1);

                // Normalize string for safe matching
                string key = period.ToUpper().Replace(" ", "").Replace("(", "").Replace(")", "");

                if (key == "TODAY")
                {
                    from_date = curdate.ToString("yyyy-MM-dd");
                    till_date = curdate.ToString("yyyy-MM-dd");
                }
                else if (key == "YESTERDAY")
                {
                    from_date = curdate.AddDays(-1).ToString("yyyy-MM-dd");
                    till_date = curdate.AddDays(-1).ToString("yyyy-MM-dd");
                }
                else if (key == "THISWEEK")
                {
                    // Aapka purana Sunday-based logic
                    DayOfWeek currentDay = curdate.DayOfWeek;
                    int daysTillCurrentDay = currentDay - DayOfWeek.Sunday;
                    DateTime currentWeekStartDate = curdate.AddDays(-daysTillCurrentDay);
                    from_date = currentWeekStartDate.ToString("yyyy-MM-dd");
                    till_date = curdate.ToString("yyyy-MM-dd");
                }
                else if (key == "WTDWEEKTODATE" || key == "WTD")
                {
                    int diffMon = (int)curdate.DayOfWeek - (int)DayOfWeek.Monday;
                    if (curdate.DayOfWeek == DayOfWeek.Sunday) diffMon = 6;
                    DateTime wtdStart = curdate.AddDays(-diffMon);
                    from_date = wtdStart.ToString("yyyy-MM-dd");
                    till_date = curdate.ToString("yyyy-MM-dd");
                }
                else if (key == "LASTWEEKSUNTOSAT" || key == "LASTWEEK")
                {
                    // 🔥 Correct Last Week (Sun-Sat) logic
                    int dOW = (int)curdate.DayOfWeek; 
                    int daysToSat = dOW + 1; 
                    DateTime lastSat = curdate.AddDays(-daysToSat);
                    DateTime lastSun = lastSat.AddDays(-6);
                    from_date = lastSun.ToString("yyyy-MM-dd");
                    till_date = lastSat.ToString("yyyy-MM-dd");
                }
                else if (key == "MONTHTODATE")
                {
                    from_date = firstDayOfMonth.ToString("yyyy-MM-dd");
                    till_date = curdate.ToString("yyyy-MM-dd");
                }
                else if (key == "LASTMONTH")
                {
                    from_date = firstDayOfMonth.AddMonths(-1).ToString("yyyy-MM-dd");
                    till_date = firstDayOfMonth.AddDays(-1).ToString("yyyy-MM-dd");
                }
                else if (key == "LASTTHREEMONTH")
                {
                    DateTime startOfLast3Months = curdate.AddMonths(-3);
                    startOfLast3Months = new DateTime(startOfLast3Months.Year, startOfLast3Months.Month, 1);
                    DateTime endOfLastMonth = new DateTime(curdate.Year, curdate.Month, 1).AddDays(-1);
                    from_date = startOfLast3Months.ToString("yyyy-MM-dd");
                    till_date = endOfLastMonth.ToString("yyyy-MM-dd");
                }
                else if (key == "QTRTODATE")
                {
                    DateTime startOfQuarter = (curdate.Month <= 3) ? new DateTime(curdate.Year, 1, 1) :
                                            (curdate.Month <= 6) ? new DateTime(curdate.Year, 4, 1) :
                                            (curdate.Month <= 9) ? new DateTime(curdate.Year, 7, 1) : 
                                                                    new DateTime(curdate.Year, 10, 1);
                    from_date = startOfQuarter.ToString("yyyy-MM-dd");
                    till_date = curdate.ToString("yyyy-MM-dd");
                }
                else if (key == "LASTQTR")
                {
                    DateTime startPrevQtr, endPrevQtr;
                    if (curdate.Month <= 3) { startPrevQtr = new DateTime(curdate.Year - 1, 10, 1); endPrevQtr = new DateTime(curdate.Year - 1, 12, 31); }
                    else if (curdate.Month <= 6) { startPrevQtr = new DateTime(curdate.Year, 1, 1); endPrevQtr = new DateTime(curdate.Year, 3, 31); }
                    else if (curdate.Month <= 9) { startPrevQtr = new DateTime(curdate.Year, 4, 1); endPrevQtr = new DateTime(curdate.Year, 6, 30); }
                    else { startPrevQtr = new DateTime(curdate.Year, 7, 1); endPrevQtr = new DateTime(curdate.Year, 9, 30); }
                    from_date = startPrevQtr.ToString("yyyy-MM-dd");
                    till_date = endPrevQtr.ToString("yyyy-MM-dd");
                }
                else if (key == "LASTTWELVEMONTH")
                {
                    DateTime startOfLast12Months = curdate.AddYears(-1).AddDays(-curdate.Day + 1);
                    DateTime endOfLastMonth = new DateTime(curdate.Year, curdate.Month, 1).AddDays(-1);
                    from_date = startOfLast12Months.ToString("yyyy-MM-dd");
                    till_date = endOfLastMonth.ToString("yyyy-MM-dd");
                }
                else if (key == "YEARTODATE")
                {
                    from_date = firstDayOfYear.ToString("yyyy-MM-dd");
                    till_date = curdate.ToString("yyyy-MM-dd");
                }
                else if (key == "LASTYEAR")
                {
                    from_date = firstDayOfYear.AddYears(-1).ToString("yyyy-MM-dd");
                    till_date = firstDayOfYear.AddDays(-1).ToString("yyyy-MM-dd");
                }
                else
                {
                    // Default Fallback
                    from_date = "1900-01-01";
                    till_date = "2099-12-31";
                }

                PD.from_date = from_date;
                PD.till_date = till_date;
            }
            return PD;
        }
        
       
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