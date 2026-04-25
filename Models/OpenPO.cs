using Microsoft.Data.SqlClient;

namespace ECNREPORTAPI.Models
{
    public class OpenPO
    {
        public string location_id { get; set; } = "";
    public string location_name { get; set; } = "";
    public string order_date { get; set; } = "";
    public string date_due { get; set; } = "";
    public string po_no { get; set; } = "";
    public string external_po_no { get; set; } = "";
    public string supplier_id { get; set; } = "";
    public string supplier_name { get; set; } = "";
    public string item_id { get; set; } = "";
    public string supplier_part_no { get; set; } = "";
    public string item_desc { get; set; } = "";
    public string qty_ordered { get; set; } = "0";
    public string qty_received { get; set; } = "0";
    public string qty_remaining { get; set; } = "0";

        // ✅ DATA API
        public async Task<List<OpenPO>> GetDataAsync(string compId, string? supplierId, int pageNumber, int pageSize, IConfiguration config)
        {
            var list = new List<OpenPO>();
            var conStr = new Common(config).GetDataBaseConnectionStringHardCoded(compId);

            if (string.IsNullOrWhiteSpace(conStr))
                return list;

            await using var con = new SqlConnection(conStr);

            string supplierFilter = "";
            bool isAll = string.IsNullOrWhiteSpace(supplierId) || supplierId == "ALL";

            if (!isAll)
                supplierFilter = " AND poh.supplier_id = @SupplierId";

            string sql = $@"
                SELECT 
                    poh.location_id,
                    l.location_name,
                    poh.order_date,
                    pol.date_due,
                    poh.po_no,
                    ISNULL(poh.external_po_no, '') AS external_po_no,
                    s.supplier_id,
                    s.supplier_name,
                    im.item_id,
                    ins.supplier_part_no,
                    im.item_desc,
                    pol.qty_ordered,
                    pol.qty_received,
                    pol.qty_ordered - pol.qty_received AS qty_remaining
                FROM p21_view_po_hdr AS poh
                INNER JOIN p21_view_po_line AS pol ON pol.po_no = poh.po_no
                INNER JOIN p21_view_supplier AS s ON s.supplier_id = poh.supplier_id
                INNER JOIN p21_view_inventory_supplier AS ins 
                    ON ins.supplier_id = s.supplier_id AND ins.inv_mast_uid = pol.inv_mast_uid
                INNER JOIN p21_view_inv_mast AS im ON im.inv_mast_uid = pol.inv_mast_uid
                INNER JOIN p21_view_location AS l ON l.location_id = poh.location_id
                WHERE pol.complete = 'N'
                AND pol.delete_flag = 'N'
                AND pol.cancel_flag = 'N'
                AND poh.po_type NOT IN ('Q','X')
                {supplierFilter}
                ORDER BY poh.order_date
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;";

            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            if (!isAll)
                cmd.Parameters.AddWithValue("@SupplierId", supplierId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new OpenPO
                {
                    location_id = reader["location_id"]?.ToString() ?? "",
                    location_name = reader["location_name"]?.ToString() ?? "",
                    order_date = reader["order_date"] ?.ToString() ?? "",
                    date_due = reader["date_due"]?.ToString() ?? "",
                    po_no = reader["po_no"]?.ToString() ?? "",
                    external_po_no = reader["external_po_no"]?.ToString() ?? "",
                    supplier_id = reader["supplier_id"]?.ToString() ?? "",
                    supplier_name = reader["supplier_name"]?.ToString() ?? "",
                    item_id = reader["item_id"]?.ToString() ?? "",
                    supplier_part_no = reader["supplier_part_no"]?.ToString() ?? "",
                    item_desc = reader["item_desc"]?.ToString() ?? "",
                    qty_ordered = reader["qty_ordered"]?.ToString() ?? "0",
                    qty_received = reader["qty_received"]?.ToString() ?? "0",
                    qty_remaining = reader["qty_remaining"]?.ToString() ?? "0"
                });
            }

            return list;
        }

    
    }
}