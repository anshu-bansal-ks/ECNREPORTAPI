using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ECNREPORTAPI.Models
{
    public class Backorders
    {
        private readonly IConfiguration _config;

        public Backorders(IConfiguration config)
        {
            _config = config;
        }

        public async Task<object> GetDataAsync(string compId, string repId, string checkedStatus)
        {
            var common = new Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);
            string compLower = compId.Trim().ToLower();

            string subQueryRep = (string.IsNullOrWhiteSpace(repId) || repId.Trim().ToUpper() == "ALL")
                ? ""
                : "oe_hdr_salesrep.salesrep_id = @repid AND ";

            string subQueryStock = (checkedStatus?.ToLower() == "true")
                ? ""
                : " AND inv_loc.qty_on_hand - inv_loc.qty_allocated > 0 ";

            string extraSelects = "";
            string extraJoins = "";
            string locationJoinCondition = "inv_loc.location_id = oe_hdr.source_location_id";

            if (compLower == "ecn")
            {
                locationJoinCondition = "inv_loc.location_id = oe_line.source_loc_id";
                extraSelects = @", '' AS ship2_id, '' AS [source], '' AS qty_allocated, '' AS on_order, '' AS disposition, '' AS rep,
                                 inv_loc.order_quantity, inv_loc.qty_in_transit, vq.nj_qty, vq.fl_qty, vq.ca_qty, '' AS pa_qty, '' AS LV_Qty";
                extraJoins = @"JOIN da_rep (NOLOCK) ON da_rep.customer_id = oe_hdr.customer_id";
            }
            else if (compLower == "ivd")
            {
                extraSelects = @", oe_hdr.address_id AS ship2_id, address.phys_state AS [source], inv_loc.qty_allocated, 
                                 (inv_loc.order_quantity + inv_loc.qty_in_transit) AS on_order, '' AS disposition, '' AS rep,
                                 '' AS order_quantity, '' AS qty_in_transit, vq.nj_qty, '' AS fl_qty, vq.ca_qty, '' AS pa_qty, '' AS LV_Qty";
                extraJoins = @"JOIN address (NOLOCK) ON address.id = oe_hdr.source_location_id";
            }
            else if (compLower == "adv")
            {
                extraSelects = @", oe_hdr.address_id AS ship2_id, address.phys_state AS [source], inv_loc.qty_allocated,
                                 (inv_loc.order_quantity + inv_loc.qty_in_transit) AS on_order, '' AS disposition, '' AS rep,
                                 '' AS order_quantity, '' AS qty_in_transit, vq.nj_qty, vq.fl_qty, vq.ca_qty, '' AS pa_qty, vq.LV_Qty";
                extraJoins = @"JOIN address (NOLOCK) ON address.id = oe_hdr.source_location_id";
            }
            else if (compLower == "xg")
            {
                locationJoinCondition = "inv_loc.location_id = oe_hdr.location_id";
                extraSelects = @", oe_hdr.address_id AS ship2_id, address.phys_state AS [source], inv_loc.qty_allocated,
                                 '' AS on_order, '' AS disposition, '' AS rep,
                                 inv_loc.order_quantity, inv_loc.qty_in_transit, '' AS nj_qty, '' AS fl_qty, '' AS ca_qty, vq.pa_qty, '' AS LV_Qty";
                extraJoins = @"JOIN da_rep (NOLOCK) ON da_rep.customer_id = oe_hdr.customer_id
                               JOIN address (NOLOCK) ON address.id = oe_hdr.source_location_id";
            }
            else
            {
                locationJoinCondition = "inv_loc.location_id = oe_hdr.location_id";
                extraSelects = @", oe_hdr.address_id AS ship2_id, address.phys_state AS [source], inv_loc.qty_allocated,
                                 (inv_loc.order_quantity + inv_loc.qty_in_transit) AS on_order, oe_line.disposition, c.first_name + ' ' + c.last_name AS rep,
                                 inv_loc.order_quantity, inv_loc.qty_in_transit, '' AS nj_qty, '' AS fl_qty, vq.ca_qty, '' AS pa_qty, '' AS LV_Qty";
                extraJoins = @"JOIN da_rep (NOLOCK) ON da_rep.customer_id = oe_hdr.customer_id
                               JOIN address (NOLOCK) ON address.id = oe_hdr.source_location_id";
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "backorders.sql");
            string sql = await File.ReadAllTextAsync(filePath);

            sql = sql.Replace("{extraSelects}", extraSelects)
                     .Replace("{extraJoins}", extraJoins)
                     .Replace("{subQueryRep}", subQueryRep)
                     .Replace("{locationJoinCondition}", locationJoinCondition)
                     .Replace("{subQueryStock}", subQueryStock);

            using var con = new SqlConnection(conStr);
            var p = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(repId) && repId.Trim().ToUpper() != "ALL")
            {
                p.Add("repid", repId);
            }

            var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 180)).ToList();
            return new { Data = rawData, ExcelData = rawData };
        }

        public async Task<List<object>> GetSalesRepsAsync(string compId)
        {
            var common = new Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);

            string sql = @"
                SELECT DISTINCT oe_hdr_salesrep.salesrep_id,
                                contacts.first_name + ' ' + contacts.last_name AS rep
                FROM oe_hdr WITH (NOLOCK)
                JOIN oe_hdr_salesrep (NOLOCK) ON oe_hdr.order_no = oe_hdr_salesrep.order_number
                JOIN oe_line (NOLOCK) ON oe_hdr.order_no = oe_line.order_no
                JOIN inv_mast WITH (NOLOCK) ON inv_mast.inv_mast_uid = oe_line.inv_mast_uid
                JOIN contacts (NOLOCK) ON oe_hdr_salesrep.salesrep_id = contacts.id
                JOIN inv_loc WITH (NOLOCK) ON inv_loc.inv_mast_uid = inv_mast.inv_mast_uid
                LEFT OUTER JOIN V_QTY AS vq (NOLOCK) ON vq.uid = inv_mast.inv_mast_uid
                WHERE oe_hdr.delete_flag = 'N'
                  AND oe_line.delete_flag = 'N'
                  AND oe_line.disposition IN ('B')
                  AND inv_loc.location_id = oe_hdr.location_id
                  AND oe_hdr.rma_flag = 'N'
                  AND oe_hdr.projected_order = 'N'
                  AND oe_hdr.cancel_flag = 'N'
                ORDER BY rep";

            using var con = new SqlConnection(conStr);
            var result = await con.QueryAsync<dynamic>(sql, commandTimeout: 180);
            return result.ToList();
        }
    }
}