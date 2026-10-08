using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class ListOfSkusUpcsPricesCosts
    {
        private readonly IConfiguration _config;
        public ListOfSkusUpcsPricesCosts(IConfiguration config)
        {
            _config = config;
        }

        public string item_id { get; set; } = "";
        public string item_desc { get; set; } = "";
        public string price1 { get; set; } = "";
        public string upc { get; set; } = "";
        public string cost { get; set; } = "";

        public List<ListOfSkusUpcsPricesCosts> GetData(string Comp_id, string ItemIdList, string locationList)
        {
            var list = new List<ListOfSkusUpcsPricesCosts>();

            try
            {
                Common obj_gd = new(_config);
                string conStr = obj_gd.GetDataBaseConnectionStringHardCoded(Comp_id);
                string dashboard = _config["DASHBOARD"] ?? "dashboard";

                string strSQL = $@"
                    SELECT tbl.value as item_id, item_desc, price1, upc, cost 
                    FROM {dashboard}.dbo.fn_CommaSeparatedStringToTable(@ItemIdList, 'N') tbl 
                    LEFT JOIN p21_view_inv_mast im ON tbl.value = im.item_id 
                    LEFT OUTER JOIN v_upc (NOLOCK) ON v_upc.inv_mast_uid = im.inv_mast_uid
                    LEFT OUTER JOIN (
                        SELECT inv_mast_uid, MAX(standard_cost) cost 
                        FROM p21_view_inv_loc 
                        WHERE location_id IN (SELECT value FROM {dashboard}.dbo.fn_CommaSeparatedStringToTable(@LocationList, ',')) 
                        GROUP BY inv_mast_uid
                    ) AS costs ON im.inv_mast_uid = costs.inv_mast_uid";

                using var con = new SqlConnection(conStr);
                con.Open();
                using var cmd = new SqlCommand(strSQL, con);
                cmd.Parameters.AddWithValue("@LocationList", locationList ?? "");
                cmd.Parameters.AddWithValue("@ItemIdList", ItemIdList ?? "");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var val = new ListOfSkusUpcsPricesCosts(_config)
                    {
                        item_id = reader["item_id"]?.ToString() ?? "",
                        item_desc = reader["item_desc"]?.ToString() ?? "",
                        price1 = reader["price1"]?.ToString() ?? "",
                        upc = reader["upc"]?.ToString() ?? "",
                        cost = reader["cost"]?.ToString() ?? ""
                    };
                    list.Add(val);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ListOfSkusUpcsPricesCosts Error: " + ex.Message);
            }

            return list;
        }
    }
}