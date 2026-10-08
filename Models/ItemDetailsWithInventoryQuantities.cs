using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class ItemDetailsWithInventoryQuantities
    {
        private readonly IConfiguration _config;
        public ItemDetailsWithInventoryQuantities(IConfiguration config)
        {
            _config = config;
        }

        public string item_id { get; set; } = "";
        public string item_desc { get; set; } = "";
        public string UPC { get; set; } = "";
        public string release_date { get; set; } = "";
        public string price1 { get; set; } = "";
        public string PA_Qty { get; set; } = "";
        public string NJ_QTY { get; set; } = "";
        public string FL_Qty { get; set; } = "";
        public string CA_Qty { get; set; } = "";
        public string LV_Qty { get; set; } = "";
        public string Tot_Qty { get; set; } = "";

        private string GetConnection(string compId)
        {
            Common common = new(_config);
            return common.GetDataBaseConnectionStringHardCoded(compId);
        }

        public List<ItemDetailsWithInventoryQuantities> GetData(string compId, string itemIdList)
        {
            var list = new List<ItemDetailsWithInventoryQuantities>();
            var conStr = GetConnection(compId);
            var dashboard = _config["DASHBOARD"] ?? "dashboard";

            string subSql = compId.Trim().ToLower() switch
            {
                "xg" => "PA_QTY",
                "adv" => "NJ_QTY, FL_QTY, CA_QTY, LV_QTY",
                _ => "NJ_QTY, FL_QTY, CA_QTY"
            };

            string sql = $@"
                SELECT im.item_id,
                       im.item_desc,
                       UPC,
                       ud.release_date,
                       price1,
                       {subSql},
                       Tot_Qty
                FROM {dashboard}.dbo.fn_CommaSeparatedStringToTable(@ItemIdList, 'N') tbl
                LEFT JOIN inv_mast im (NOLOCK) ON tbl.value = im.item_id
                LEFT JOIN v_upc (NOLOCK) ON v_upc.inv_mast_uid = v_upc.inv_mast_uid AND v_upc.item_id = im.item_id
                LEFT OUTER JOIN inv_mast_ud ud ON ud.inv_mast_uid = im.inv_mast_uid 
                INNER JOIN V_QTY ON V_QTY.UID = im.inv_mast_uid AND v_qty.Item = im.item_id";

            try
            {
                using var con = new SqlConnection(conStr);
                con.Open();
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ItemIdList", itemIdList);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var item = new ItemDetailsWithInventoryQuantities(_config)
                    {
                        item_id = reader["item_id"]?.ToString() ?? "",
                        item_desc = reader["item_desc"]?.ToString() ?? "",
                        UPC = reader["UPC"]?.ToString() ?? "",
                        price1 = reader["price1"]?.ToString() ?? "",
                        Tot_Qty = reader["Tot_Qty"]?.ToString() ?? ""
                    };

                    string rdate = reader["release_date"]?.ToString()?.ToUpper() ?? "";
                    if (string.IsNullOrEmpty(rdate) || rdate == "NULL")
                    {
                        item.release_date = "";
                    }
                    else if (DateTime.TryParse(rdate, out DateTime parsedDt))
                    {
                        item.release_date = parsedDt.ToString("MM/dd/yyyy");
                    }

                    string compLower = compId.Trim().ToLower();
                    if (compLower == "xg")
                    {
                        item.PA_Qty = reader["PA_QTY"]?.ToString() ?? "";
                    }
                    else
                    {
                        item.NJ_QTY = reader["NJ_QTY"]?.ToString() ?? "";
                        item.FL_Qty = reader["FL_QTY"]?.ToString() ?? "";
                        item.CA_Qty = reader["CA_QTY"]?.ToString() ?? "";
                        if (compLower == "adv")
                        {
                            item.LV_Qty = reader["LV_QTY"]?.ToString() ?? "";
                        }
                    }

                    list.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ItemDetailsWithInventoryQuantities Error: " + ex.Message);
            }

            return list;
        }
    }
}