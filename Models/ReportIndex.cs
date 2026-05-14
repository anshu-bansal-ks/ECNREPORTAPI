using Microsoft.Data.SqlClient;

namespace ECNREPOTINGPORTAL.Models
{
    public class ReportIndex
    {
        public string ReportId { get; set; } = "";
        public string ReportName { get; set; } = "";
        public string? ReportSubtitles { get; set; }
        public string? Rank { get; set; }
        public string? Url { get; set; }
        public string Label { get; set; } = "";
        public string? Keyword { get; set; }
        public string? Description { get; set; }

        public List<ReportIndex> GetReports(int userId, string? keyword, IConfiguration config)
        {
            var list = new List<ReportIndex>();
            var conStr = config.GetConnectionString("strCon_dashboard");
            if (string.IsNullOrWhiteSpace(conStr)) return list;

            using var con = new SqlConnection(conStr);

            // SQL ko thoda "Smart" banaya hai
            string sql = $@"
                        WITH sub_tree AS (
                            -- 1. Pehle saari reports nikalo jo is user ke access mein hain (Bina filter ke)
                            SELECT rs.ReportId, rs.report_name, rs.ReportSubtitles, rs.rank, rs.url,
                                rs.AddedDate, DATEDIFF(day, rs.AddedDate, GETDATE()) AS rptDay,
                                rs.parent_report_id, rs.showPortalYN, rs.keyword, rs.description
                            FROM Reports rs
                            INNER JOIN Role_Report rr ON rr.ReportId = rs.ReportId
                            INNER JOIN User_Role ur ON ur.RoleId = rr.RoleId
                            WHERE ur.UserId = @user_id
                        )
                        SELECT DISTINCT *
                        FROM sub_tree
                        WHERE (
                        -- Case A: Dashboard Load (Keyword khali hai) -> Sirf Main Reports dikhao
                        ((@keyword IS NULL OR @keyword = '') AND parent_report_id IS NULL AND (showPortalYN = 1 OR showPortalYN IS NULL))
                        OR 
                        -- Case B: Drill-down (Keyword hai) -> Pure tree mein dhoondo, chahe parent koi bhi ho
                        (@keyword IS NOT NULL AND @keyword <> '' AND 
                            (report_name LIKE '%' + @keyword + '%' OR url LIKE '%' + @keyword + '%' OR keyword LIKE '%' + @keyword + '%')
                        )
                    )
                    ORDER BY report_name";                                                              

            try
            {
                con.Open();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@keyword", keyword ?? (object)DBNull.Value);
                
                using var reader = cmd.ExecuteReader();
                int showNewDays = int.Parse(config["ShowNewLabelDays"] ?? "3");

                while (reader.Read())
                {
                    int rptDay = reader["rptDay"] != DBNull.Value ? Convert.ToInt32(reader["rptDay"]) : -1;

                    list.Add(new ReportIndex
                    {
                        ReportId = reader["reportId"]?.ToString() ?? "",
                        ReportName = reader["report_name"]?.ToString() ?? "",
                        ReportSubtitles = reader["ReportSubtitles"]?.ToString(),
                        Rank = reader["rank"]?.ToString(),
                        Url = reader["url"]?.ToString(),
                        Label = (rptDay >= 0 && rptDay <= showNewDays) ? "New" : "",
                        Keyword = reader["keyword"]?.ToString(),
                        Description = reader["description"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReportIndex Error: " + ex.Message);
            }
            return list;
        }
    }
}
