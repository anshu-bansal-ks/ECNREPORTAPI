using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Serialization;

namespace ECNREPORTAPI.Models
{
    public class ItemDetails
    {
        private readonly IConfiguration _config;
        public ItemDetails(IConfiguration config)
        {
            _config = config;
        }
        public string item_id { get; set; } = "";
        public string item_desc { get; set; } = "";
        public string price1 { get; set; } = "";
        public string upc { get; set; } = "";
        
        private string GetConnection(string compId)
        {
            Common common = new(_config);
            return common.GetDataBaseConnectionStringHardCoded(compId);
        }
        public List<ItemDetails> GetData(string compId, string itemIdList)
        {
            var list = new List<ItemDetails>();

             var conStr = GetConnection(compId);
            var dashboard = _config["DASHBOARD"]!;
            using var con = new SqlConnection(conStr);

            string sql = $@"SELECT tbl.value as item_id, item_desc, price1, UPC
                FROM {dashboard}.dbo.fn_CommaSeparatedStringToTable(@ItemIdList, 'N') tbl
                LEFT JOIN p21_view_inv_mast mast ON tbl.value = mast.item_id
                LEFT JOIN v_upc ON v_upc.inv_mast_uid = mast.inv_mast_uid";

            try
            {
                con.Open();
                sql = sql.Replace("{dashboard}", dashboard);
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ItemIdList", itemIdList);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ItemDetails(_config)
                    {
                        item_id = reader["item_id"]?.ToString() ?? "",
                        item_desc = reader["item_desc"]?.ToString() ?? "",
                        price1 = reader["price1"]?.ToString() ?? "",
                        upc = reader["UPC"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ItemDetails Error: " + ex.Message);
            }

            return list;
        }
    }
}