using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Models
{
    public class ForecastReportForXGenService
    {
        private readonly IConfiguration _config;
        public ForecastReportForXGenService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<object> GetDataAsync(string compId, Dictionary<string, string> filters)
        {
            var common = new Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);

            string supplierId = filters.GetValueOrDefault("supplierId", "ALL");
            string prefix = filters.GetValueOrDefault("prefix", "");
            string locationIdStr = filters.GetValueOrDefault("locationId", "0");
            bool isExport = filters.TryGetValue("isExport", out string? expVal) && expVal.Equals("true", StringComparison.OrdinalIgnoreCase);

            // 👉 Step 2: Handle Pagination vs Full Export Data
            int pageNumber = int.TryParse(filters.GetValueOrDefault("pageNumber", "1"), out int pn) ? pn : 1;
            
            // Agar export hai toh pageSize bahut bada kar do taaki saara data ek hi baar mein aa jaye, warna grid ke liye 100 rakho
            int pageSize = isExport ? 2000000 : (int.TryParse(filters.GetValueOrDefault("pageSize", "100"), out int ps) ? ps : 100);
            int offset = isExport ? 0 : (pageNumber - 1) * pageSize;

            var p = new DynamicParameters();
            
            int locationId = int.TryParse(locationIdStr, out int parsedLoc) ? parsedLoc : 0;
            p.Add("locationid", locationId);
            p.Add("Offset", offset);
            p.Add("PageSize", pageSize);

            string supplierQuery = "";
            string binQuery = "";

            if (!string.IsNullOrEmpty(supplierId) && !supplierId.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(supplierId, out int parsedSup))
                {
                    supplierQuery = " AND p21_view_inventory_supplier.supplier_id = @supplierId ";
                    p.Add("supplierId", parsedSup);
                }
            }

            if (supplierId.Equals("ALL", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(prefix))
            {
                binQuery = " AND primary_bin LIKE @prefix ";
                p.Add("prefix", $"{prefix}%");
            }

            // Period Logic (Last 6 Months calculation)
            DateTime current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            int startMonth = current.Month - 1;
            int cnt = 1;
            string curYearMonths = "";
            string curYear = current.AddMonths(-1).ToString("yyyy");
            int m = startMonth;
            curYearMonths = m.ToString();
            m--;

            while (cnt <= 6 && m >= 1)
            {
                curYearMonths = m.ToString() + "," + curYearMonths;
                m--;
                cnt++;
            }

            string prevYearMonths = "";
            string prevYear = "";
            if (cnt < 6)
            {
                m = 12;
                prevYearMonths = m.ToString();
                cnt++;
                prevYear = current.AddMonths(-cnt).ToString("yyyy");
                while (cnt < 6)
                {
                    m--;
                    prevYearMonths = m.ToString() + "," + prevYearMonths;
                    cnt++;
                }
            }

            string subqueryP = " ((p.period IN(" + curYearMonths + ") AND p.year_for_period = " + curYear + ") ";
            if (!string.IsNullOrEmpty(prevYear))
            {
                subqueryP += "or (p.period IN(" + prevYearMonths + ") AND p.year_for_period = " + prevYear + " ) ) ";
            }
            else
            {
                subqueryP += ")";
            }

            string sql = $@"
            SELECT 
                inv_mast.item_id,
                inv_mast.item_desc,
                p21_view_inventory_supplier.cost,
                p21_view_inv_loc.qty_on_hand,
                p21_view_inv_loc.order_quantity,
                p21_view_inv_loc.qty_allocated,
                inv_mast_ud.release_date,
                AVG(ISNULL(tbl.inv_period_usage, 0)) AS [AVG],
                (AVG(ISNULL(tbl.inv_period_usage, 0)) * 12) AS years_usage,
                p21_view_inv_loc.primary_bin
            FROM inv_mast WITH (NOLOCK)
            INNER JOIN (
                SELECT mast.inv_mast_uid,
                       mast.year_for_period,
                       mast.period,
                       ISNULL(p21_view_inv_period_usage.inv_period_usage, 0) AS inv_period_usage,
                       ISNULL(p21_view_inv_period_usage.location_id, @locationid) AS location_id,
                       mast.demand_period_uid
                FROM (
                    SELECT inv_mast.inv_mast_uid,
                           p.demand_period_uid,
                           p.year_for_period,
                           p.period
                    FROM p21_view_demand_period AS p
                    CROSS JOIN inv_mast
                    WHERE {subqueryP}
                ) AS mast
                LEFT OUTER JOIN p21_view_inv_period_usage WITH (NOLOCK) 
                    ON p21_view_inv_period_usage.inv_mast_uid = mast.inv_mast_uid
                   AND p21_view_inv_period_usage.demand_period_uid = mast.demand_period_uid
                   AND p21_view_inv_period_usage.location_id = @locationid
            ) tbl ON tbl.inv_mast_uid = inv_mast.inv_mast_uid
            JOIN p21_view_inv_loc WITH (NOLOCK) 
                ON p21_view_inv_loc.inv_mast_uid = tbl.inv_mast_uid 
               AND p21_view_inv_loc.location_id = tbl.location_id
            LEFT JOIN inv_mast_ud WITH (NOLOCK) 
                ON inv_mast_ud.inv_mast_uid = inv_mast.inv_mast_uid
            JOIN p21_view_inventory_supplier WITH (NOLOCK) 
                ON p21_view_inventory_supplier.inv_mast_uid = inv_mast.inv_mast_uid
            JOIN p21_view_supplier WITH (NOLOCK) 
                ON p21_view_supplier.supplier_id = p21_view_inventory_supplier.supplier_id
            JOIN dbo.inventory_supplier_x_loc 
                ON inventory_supplier_x_loc.inventory_supplier_uid = p21_view_inventory_supplier.inventory_supplier_uid 
               AND inventory_supplier_x_loc.location_id = p21_view_inv_loc.location_id
            WHERE tbl.location_id = @locationid
              {supplierQuery}
              AND p21_view_inventory_supplier.delete_flag = 'n'
              AND inv_mast.delete_flag = 'N'
              AND primary_supplier = 'Y'
              {binQuery}
            GROUP BY 
                inv_mast.item_id,
                inv_mast.item_desc,
                p21_view_inventory_supplier.cost,
                p21_view_inv_loc.qty_on_hand,
                p21_view_inv_loc.order_quantity,
                p21_view_inv_loc.qty_allocated,
                inv_mast_ud.release_date,
                p21_view_inv_loc.primary_bin,
                tbl.location_id
            ORDER BY inv_mast.item_id";

            using var con = new SqlConnection(conStr);
            var rawData = (await con.QueryAsync<dynamic>(sql, p, commandTimeout: 300)).ToList();

            var formattedList = rawData.Select(row => (IDictionary<string, object>)row)
                                       .Select(dict => new Dictionary<string, object>(dict))
                                       .ToList();

            return new { Data = formattedList, ExcelData = formattedList };
        }
    }
}