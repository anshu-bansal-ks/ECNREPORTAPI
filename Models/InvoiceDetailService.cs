using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECNREPORTAPI.Services
{
    public class InvoiceDetailService
    {
        private readonly IConfiguration _config;

        public InvoiceDetailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<Dictionary<string, object>>> GetDataAsync(string compId, string invoiceNum)
        {
            var common = new Models.Common(_config);
            string conStr = common.GetDataBaseConnectionStringHardCoded(compId);

            using var con = new SqlConnection(conStr);
            await con.OpenAsync();

            string sql = @"
                SELECT invoice_hdr.invoice_date,
                       invoice_hdr.invoice_no,
                       invoice_hdr.customer_id,
                       invoice_hdr.bill2_name,
                       invoice_hdr.po_no,
                       invoice_hdr.total_amount,
                       invoice_line.item_id,
                       invoice_line.item_desc,
                       invoice_line.unit_price,
                       invoice_line.qty_shipped,
                       invoice_line.extended_price,
                       invoice_line.line_no,
                       address.name,
                       oe_pick_ticket.tracking_no
                FROM invoice_hdr WITH (nolock) 
                JOIN invoice_line WITH (nolock) ON invoice_hdr.invoice_no = invoice_line.invoice_no 
                JOIN oe_pick_ticket ON oe_pick_ticket.invoice_no = invoice_hdr.invoice_no 
                JOIN address ON oe_pick_ticket.carrier_id = address.id
                WHERE invoice_hdr.invoice_no = @invoiceNum
                ORDER BY invoice_line.line_no";

            var rows = (await con.QueryAsync<dynamic>(sql, new { invoiceNum })).ToList();
            return rows.Select(x => (IDictionary<string, object>)x)
                       .Select(d => d.ToDictionary(k => k.Key, v => v.Value))
                       .ToList();
        }
    }
}