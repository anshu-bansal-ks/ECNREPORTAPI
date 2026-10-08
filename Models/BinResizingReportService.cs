using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class BinResizingReportService
    {
        private readonly IConfiguration _config;
        public BinResizingReportService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<object> GetDataAsync(string compId, Dictionary<string, string> filters)
        {
            var common = new Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "binresizingreport.sql");
            if (!File.Exists(filePath)) return new { Data = new List<object>(), ExcelData = new List<object>() };

            string sql = await File.ReadAllTextAsync(filePath);

            string supplierId = filters.GetValueOrDefault("supplierId", "ALL");
            string bin = filters.GetValueOrDefault("bin", "");
            string discOnly = filters.GetValueOrDefault("checkedStatus", "false");
            string locationId = filters.GetValueOrDefault("locationId", "0");
            string scat = filters.GetValueOrDefault("scat", "ALL");

            string supplierQuery = "";
            string binQuery = "";
            string discQuery = "";

            var p = new DynamicParameters();
            p.Add("locationid", locationId);

           if (!string.IsNullOrEmpty(supplierId) && !supplierId.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(supplierId, out int parsedSupplierId))
                {
                    supplierQuery = " AND p21_view_inventory_supplier.supplier_id = @supplierId ";
                    p.Add("supplierId", parsedSupplierId); 
                }
            }
            else if (supplierId.Equals("ALL", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(bin))
            {
                binQuery = " AND p21_view_inv_loc.primary_bin LIKE @bin ";
                p.Add("bin", $"{bin}%");
            }

            if (discOnly.ToLower() == "true")
            {
                discQuery = " AND inv_mast.item_desc LIKE '%(DISC)%' ";
            }

            sql = sql.Replace("{supplierQuery}", supplierQuery)
                     .Replace("{binQuery}", binQuery)
                     .Replace("{discQuery}", discQuery);

            using var con = new SqlConnection(conStr);
            var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 300)).ToList();
            var formattedList = new List<Dictionary<string, object>>();
            string sifyConnStr = common.strecnsalsify;
            using var sifyCon = new SqlConnection(sifyConnStr);
            string sifyQuery = @"SELECT item_sku, 
                CASE WHEN LEN(structure_group) > 0 AND CHARINDEX(' > ', structure_group) > 0 
                THEN RIGHT(structure_group, LEN(structure_group) - (CHARINDEX(' > ', structure_group) + 2)) ELSE '' END AS scat 
                FROM salsify_itemData WHERE ISNULL(item_erpuid, 0) > 0 AND ISNULL(item_webid, 0) > 0";
            
            var salsifyItems = (await sifyCon.QueryAsync<dynamic>(sifyQuery)).ToDictionary(x => (string)x.item_sku, x => (string)x.scat);

            foreach (var row in rawData)
            {
                var dict = (IDictionary<string, object>)row;
                string itemId = dict["item_id"]?.ToString() ?? "";
                string itemScat = salsifyItems.ContainsKey(itemId) ? salsifyItems[itemId] : "";
                if (!compId.Equals("ECN", StringComparison.OrdinalIgnoreCase))
                    {
                        if (dict.ContainsKey("scat"))
                        {
                            dict.Remove("scat");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(scat) && !scat.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(itemScat) || !itemScat.Equals(scat, StringComparison.OrdinalIgnoreCase))
                            {
                                continue; 
                            }
                        }
                        dict["scat"] = itemScat;
                    }
            
                formattedList.Add(new Dictionary<string, object>(dict));
            }

            return new { Data = formattedList, ExcelData = formattedList };
        }
    }
}