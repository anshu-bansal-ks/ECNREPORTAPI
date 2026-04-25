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
            if (string.IsNullOrWhiteSpace(conStr))
                return list;

            using var con = new SqlConnection(conStr);

            string subQuery = "";
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                subQuery = " AND (rs.keyword LIKE '%'+ @keyword +'%' OR rs.description LIKE '%'+ @keyword +'%') ";
            }

            string sql = $@"
WITH sub_tree(reportId, report_name, ReportSubtitles, rank, url, addedDate, rptDay, parent_report_id, showPortalYN, keyword, description)
AS(
    SELECT rs.ReportId, rs.report_name,
           rs.ReportSubtitles,
           rs.rank, rs.url,
           rs.AddedDate, DATEDIFF(day, rs.AddedDate, GETDATE()) AS rptDay,
           rs.parent_report_id, rs.showPortalYN, rs.keyword, rs.description
    FROM Reports rs
    INNER JOIN Role_Report rr ON rr.ReportId = rs.ReportId
    INNER JOIN User_Role ur ON ur.RoleId = rr.RoleId
    WHERE parent_report_id IS NULL
    AND ur.UserId = @user_id
    {subQuery}

    UNION ALL

    SELECT rs.ReportId, rs.report_name,
           rs.ReportSubtitles,
           rs.rank, rs.url,
           rs.AddedDate, DATEDIFF(day, rs.AddedDate, GETDATE()) AS rptDay,
           rs.parent_report_id, rs.showPortalYN, rs.keyword, rs.description
    FROM Reports rs
    INNER JOIN sub_tree st ON rs.parent_report_id = st.ReportId
    WHERE rs.parent_report_id IS NOT NULL
    {subQuery}
)
SELECT DISTINCT *
FROM sub_tree
WHERE (showPortalYN = 1 OR showPortalYN IS NULL)
ORDER BY report_name";

            try
            {
                con.Open();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@user_id", userId);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    cmd.Parameters.AddWithValue("@keyword", keyword);
                }

                using var reader = cmd.ExecuteReader();

                int showNewDays = int.Parse(config["ShowNewLabelDays"] ?? "3");

                while (reader.Read())
                {
                    int rptDay = reader["rptDay"] != DBNull.Value
                                ? Convert.ToInt32(reader["rptDay"])
                                : -1;

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
