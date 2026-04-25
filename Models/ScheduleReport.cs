//Modeles/SchrduleReport
using Microsoft.Data.SqlClient;

namespace ECNREPORTAPI.Models
{
    public class ScheduleReport
    {
        public string ReportName { get; set; } = "";
        public string recepients { get; set; } = "";
        public string cc_recepients { get; set; } = "";
        public string emailSubject { get; set; } = "";
        public string emailBody { get; set; } = "";
        public string deliveryFrequency { get; set; } = "";

        public DateTime DeliveryDateTime { get; set; }

        public string filter { get; set; } = "";
        public bool HtmlOutput { get; set; }

        public string Save(string compId, int userId, IConfiguration config)
        {
            try
            {
                var conStr = config.GetConnectionString("strCon_dashboard");

                using var con = new SqlConnection(conStr);

                string query = @"
                DECLARE @ReportId INT;

                SELECT @ReportId = ReportId 
                FROM Reports 
                WHERE report_name = @rptname;

                INSERT INTO scheduleReport
                (UserId, ReportId, ReportType, filter, recepients, cc_recepients,
                 emailSubject, emailBody, deliveryFrequency, deliveryDateTime, HtmlOutput)
                VALUES
                (@UserId, @ReportId, 1, @filter, @recepients, @cc_recepients,
                 @emailSubject, @emailBody, @deliveryFrequency, @deliveryDateTime, @HtmlOutput)
                ";

                using var cmd = new SqlCommand(query, con);

                cmd.Parameters.Add("@rptname", System.Data.SqlDbType.VarChar).Value = ReportName;
                cmd.Parameters.Add("@UserId", System.Data.SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@filter", System.Data.SqlDbType.VarChar).Value = filter ?? "";

                cmd.Parameters.Add("@recepients", System.Data.SqlDbType.VarChar).Value = recepients;
                cmd.Parameters.Add("@cc_recepients", System.Data.SqlDbType.VarChar).Value = cc_recepients ?? "";

                cmd.Parameters.Add("@emailSubject", System.Data.SqlDbType.VarChar).Value = emailSubject;
                cmd.Parameters.Add("@emailBody", System.Data.SqlDbType.VarChar).Value = emailBody ?? "";

                cmd.Parameters.Add("@deliveryFrequency", System.Data.SqlDbType.VarChar).Value = deliveryFrequency;

                cmd.Parameters.Add("@deliveryDateTime", System.Data.SqlDbType.DateTime)
                              .Value = DeliveryDateTime;

                cmd.Parameters.Add("@HtmlOutput", System.Data.SqlDbType.Bit)
                              .Value = HtmlOutput;

                con.Open();
                int count = cmd.ExecuteNonQuery();

                return count > 0 ? "Schedule Saved Successfully" : "Failed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}