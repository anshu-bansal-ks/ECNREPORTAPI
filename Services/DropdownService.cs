using System.Data;
using Microsoft.Data.SqlClient;
using ECNREPORTAPI.Dtos;
using ECNREPORTAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECNREPORTAPI.Services
{
    public class DropdownService
    {
        private readonly DashboardContext _ctx;
        private readonly Common _common;

        public DropdownService(DashboardContext ctx, Common common)
        {
            _ctx = ctx;
            _common = common;
        }

        // 1. Companies - WORKING
        public async Task<List<DropdownItemDto>> GetCompaniesAsync()
        {
            return await _ctx.DataSources
                .Select(d => new DropdownItemDto { Value = d.code, Label = d.dsource ?? d.code })
                .OrderBy(x => x.Label)
                .ToListAsync();
        }

        public async Task<string> GetDynamicConnectionString(string compCode)
        {
            var ds = await _ctx.DataSources
                .FirstOrDefaultAsync(x => x.code == compCode || x.dsource == compCode);

            if (ds == null)
                return string.Empty;

            return $"Data Source={ds.dbSource};" +
                   $"Initial Catalog={ds.dbName};" +
                   $"User ID={ds.dbUser};" +
                   $"Password={ds.dbPassword};" +
                   $"TrustServerCertificate=True;" +
                   $"Connection Timeout=30;";
        }

        private string GetConnStringByCompId(string compId)
        {
            var connStr = _common.GetDataBaseConnectionStringHardCoded(compId);
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return _common.ConStr;
            }
            return connStr;
        }

        public async Task<string> GetLocationsAsync(string compId)
        {
            var locationIds = new List<string>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return string.Empty;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT location_id 
                FROM dbo.p21_view_location WITH (NOLOCK)
                WHERE delete_flag = 'N'
                  AND location_name NOT LIKE '%RETU%'
                  AND location_name NOT LIKE '%DEF%'
                  AND location_name NOT LIKE '%TRANS%'
                  AND location_name NOT LIKE '%ASIA%'
                  AND location_name NOT LIKE '%DROP%';";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                locationIds.Add(reader.GetValue(0)?.ToString() ?? "");
            }

            return string.Join(",", locationIds.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public async Task<List<LocationDropdownDto>> GetLocationListAsync(string dsource, string locType)
        {
            var result = new List<LocationDropdownDto>();
            string connStr = _common.ConStr_Dashboard;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                DECLARE @code VARCHAR(15);
                SELECT @code = code FROM datasources WHERE dsource = @dsource;

                SELECT Company, State, location_id 
                FROM tbl_loc WITH (NOLOCK)
                WHERE Company = @code 
                  AND loc_type = @locType
                ORDER BY State;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@dsource", SqlDbType.VarChar, 50).Value = dsource ?? "";
            cmd.Parameters.Add("@locType", SqlDbType.VarChar, 50).Value = locType ?? "WAREHOUSE";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new LocationDropdownDto
                {
                    location_id = reader["location_id"]?.ToString() ?? "",
                    state = reader["State"]?.ToString() ?? ""
                });
            }

            return result;
        }

        public async Task<string> GetLocationIdAsync(string dsource, string locType, string state)
        {
            string connStr = _common.ConStr_Dashboard;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                DECLARE @code VARCHAR(15);
                SELECT @code = code FROM datasources WHERE dsource = @dsource;

                SELECT TOP 1 location_id 
                FROM tbl_loc WITH (NOLOCK)
                WHERE Company = @code 
                  AND loc_type = @locType 
                  AND State = @state;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@dsource", SqlDbType.VarChar, 50).Value = dsource ?? "";
            cmd.Parameters.Add("@locType", SqlDbType.VarChar, 50).Value = locType ?? "WAREHOUSE";
            cmd.Parameters.Add("@state", SqlDbType.VarChar, 50).Value = state ?? "";

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }

        public async Task<string> GetLocationByListAsync(string dsource, string locType)
        {
            var locList = await GetLocationListAsync(dsource, locType);
            var ids = locList.Select(l => l.location_id).Where(id => !string.IsNullOrWhiteSpace(id));
            return string.Join(",", ids);
        }

        public async Task<List<CustomerDto>> GetCustomersAsync(string compId)
        {
            var result = new List<CustomerDto>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT c.customer_id, c.customer_name, a.phys_city, a.phys_state
                FROM customer AS c WITH (NOLOCK)
                INNER JOIN address AS a WITH (NOLOCK) ON a.id = c.customer_id
                ORDER BY LTRIM(c.customer_name);";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new CustomerDto
                {
                    customer_id   = reader.GetValue(0)?.ToString() ?? "",
                    customer_name = reader.GetValue(1)?.ToString() ?? "",
                    phys_city     = reader.GetValue(2)?.ToString() ?? "",
                    phys_state    = reader.GetValue(3)?.ToString() ?? ""
                });
            }
            return result;
        }

        public async Task<List<CustomerDto>> GetCustomersByRepAsync(string compId, string repId)
        {
            var result = new List<CustomerDto>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT DISTINCT dr.customer_id, c.customer_name, a.phys_city, a.phys_state
                FROM da_rep dr WITH (NOLOCK)
                JOIN customer c WITH (NOLOCK) ON c.customer_id = dr.customer_id
                JOIN address a WITH (NOLOCK) ON a.id = c.customer_id
                WHERE dr.salesrep_id = @repId
                ORDER BY c.customer_name;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@repId", SqlDbType.NVarChar, 50).Value = repId ?? "";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new CustomerDto
                {
                    customer_id = reader.IsDBNull(0) ? "" : reader.GetString(0),
                    customer_name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    phys_city = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    phys_state = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }
            return result;
        }

        public async Task<List<VendorDto>> GetVendorsAsync(string compId)
        {
            var result = new List<VendorDto>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT DISTINCT vendor.vendor_id, vendor.vendor_name
                FROM p21_view_apinv_hdr WITH (NOLOCK)
                JOIN vendor WITH (NOLOCK) ON vendor.vendor_id = p21_view_apinv_hdr.vendor_id
                WHERE p21_view_apinv_hdr.paid_in_full = 'N'
                GROUP BY vendor.vendor_name, vendor.vendor_id
                ORDER BY vendor.vendor_name;";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new VendorDto
                {
                   vendor_id = reader.GetValue(0)?.ToString()?.Trim() ?? "",
                   vendor_name = reader.GetValue(1)?.ToString()?.Trim() ?? "",
                });
            }
            return result;
        }

        public async Task<List<SalesRepDto>> GetSalesRepsAsync(string compId)
        {
            var result = new List<SalesRepDto>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT DISTINCT 
                    oe_hdr_salesrep.salesrep_id,
                    LTRIM(RTRIM(contacts.first_name + ' ' + contacts.last_name)) AS rep_name,
                    LTRIM(RTRIM(contacts.last_name)) AS last_name
                FROM oe_hdr WITH (NOLOCK)
                JOIN oe_hdr_salesrep WITH (NOLOCK) ON oe_hdr.order_no = oe_hdr_salesrep.order_number
                JOIN contacts WITH (NOLOCK) ON oe_hdr_salesrep.salesrep_id = contacts.id
                WHERE DATEDIFF(MONTH, oe_hdr.order_date, GETDATE()) < 6
                AND contacts.first_name IS NOT NULL
                ORDER BY rep_name";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new SalesRepDto
                {
                    salesrep_id = reader.GetValue(0)?.ToString()?.Trim() ?? "",
                    rep_name    = reader.GetValue(1)?.ToString()?.Trim() ?? "",
                    last_name   = reader.GetValue(2)?.ToString()?.Trim() ?? ""
                });
            }
            return result;
        }

        public async Task<List<SupplierDto>> GetSuppliersAsync(string compId)
        {
            var result = new List<SupplierDto>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT DISTINCT 
                    s.supplier_id,
                    LTRIM(RTRIM(s.supplier_name)) AS supplier_name
                FROM p21_view_inv_mast im WITH (NOLOCK)
                JOIN p21_view_inventory_supplier invsup WITH (NOLOCK) 
                    ON invsup.inv_mast_uid = im.inv_mast_uid
                JOIN p21_view_supplier s WITH (NOLOCK) 
                    ON s.supplier_id = invsup.supplier_id
                WHERE invsup.delete_flag = 'N'
                AND im.delete_flag = 'N'
                ORDER BY supplier_name";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new SupplierDto
                {
                    supplier_id   = reader.GetValue(0)?.ToString()?.Trim() ?? "",
                    supplier_name = reader.GetValue(1)?.ToString()?.Trim() ?? ""
                });
            }
            return result;
        }

        public async Task<List<SupplierDto>> GetSuppliersOPAsync(string compId)
        {
            var result = new List<SupplierDto>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
               SELECT 
                poh.supplier_id,
                LTRIM(RTRIM(s.supplier_name)) AS supplier_name
                FROM dbo.p21_view_po_hdr poh
                JOIN dbo.p21_view_po_line pol ON pol.po_no = poh.po_no
                JOIN dbo.p21_view_supplier s ON s.supplier_id = poh.supplier_id
                WHERE pol.complete = 'N'
                AND pol.delete_flag = 'N'
                AND pol.cancel_flag = 'N'
                AND poh.po_type NOT IN('Q', 'X')
                GROUP BY poh.supplier_id, LTRIM(RTRIM(s.supplier_name))
                ORDER BY supplier_name;";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new SupplierDto
                {
                    supplier_id  = reader.GetValue(0)?.ToString()?.Trim() ?? "",
                    supplier_name  = reader.GetValue(1)?.ToString()?.Trim() ?? ""
                });
            }
            return result;
        }

        public async Task<List<ShowItemsEnhancement>> GetShowItemsAsync(string compId)
        {
            var result = new List<ShowItemsEnhancement>();
            string connStr = _common.ConStr_Dashboard;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT ShowId, ShowName
                FROM dbo.Shows
                WHERE CompId = @compId AND deleteflag = 'N'
                ORDER BY ShowName";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@compId", compId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ShowItemsEnhancement
                {
                    ShowId = reader["ShowId"]?.ToString() ?? "",
                    ShowName = reader["ShowName"]?.ToString() ?? ""
                });
            }
            return result;
        }

        public async Task<List<PromoItemsEnhancement>> GetPromoItemsAsync(string compId)
        {
            var result = new List<PromoItemsEnhancement>();
            string connStr = _common.ConStr_Dashboard;

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            const string sql = @"
                SELECT PromoId, PromoName
                FROM dbo.Promos
                WHERE CompId = @compId AND deleteflag = 'N'
                ORDER BY PromoName";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@compId", compId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new PromoItemsEnhancement
                {
                    PromoId = reader["PromoId"]?.ToString() ?? "",
                    PromoName = reader["PromoName"]?.ToString() ?? ""
                });
            }
            return result;
        }

        public async Task<List<LocationListSupplier>> GetLocationListSupplierAsync(string compId)
        {
            var result = new List<LocationListSupplier>();
            string connStr = GetConnStringByCompId(compId);
            if (string.IsNullOrWhiteSpace(connStr)) return result;

            string sqlsub = compId switch
            {
                "xg" => "('xg','xgen')",
                "adv" => "('adv','ad')",
                _ => $"('{compId}')"
            };

            await using var con = new SqlConnection(connStr);
            await con.OpenAsync();

            string sql = $@"
                SELECT location_id, location_name
                FROM dbo.p21_view_location
                WHERE company_id IN {sqlsub}
                AND delete_flag = 'N'
                ORDER BY location_name";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new LocationListSupplier
                {
                    location_id = reader["location_id"]?.ToString() ?? "",
                    location_name = reader["location_name"]?.ToString() ?? ""
                });
            }
            return result;
        }

        public List<DropdownItemDto> ToDropdownFromCustomers(IEnumerable<CustomerDto> customers)
        {
            return customers
                .Select(c => new DropdownItemDto { Value = c.customer_id, Label = c.customer_name })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromVendors(IEnumerable<VendorDto> vendors)
        {
            return vendors
                .Select(v => new DropdownItemDto { Value = v.vendor_id, Label = v.vendor_name })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromSalesReps(IEnumerable<SalesRepDto> reps)
        {
            return reps
                .Where(r => !string.IsNullOrWhiteSpace(r.rep_name))
                .Select(r => new DropdownItemDto 
                { 
                    Value = r.salesrep_id, 
                    Label = $"{r.rep_name} ({r.salesrep_id})" 
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromSuppliers(IEnumerable<SupplierDto> suppliers)
        {
            return suppliers
                .Where(s => !string.IsNullOrWhiteSpace(s.supplier_name))
                .Select(s => new DropdownItemDto 
                { 
                    Value = s.supplier_id, 
                    Label = $"{s.supplier_name} ({s.supplier_id})" 
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromLocations(IEnumerable<LocationDropdownDto> locations)
        {
            return locations
                .Select(l => new DropdownItemDto
                {
                    Value = l.location_id,
                    Label = $"{l.location_id} ({l.state})"
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromShows(IEnumerable<ShowItemsEnhancement> list)
        {
            return list
                .Select(x => new DropdownItemDto
                {
                    Value = x.ShowId,
                    Label = x.ShowName
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromPromos(IEnumerable<PromoItemsEnhancement> list)
        {
            return list
                .Select(x => new DropdownItemDto
                {
                    Value = x.PromoId,
                    Label = x.PromoName
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public List<DropdownItemDto> ToDropdownFromLocationSupplier(IEnumerable<LocationListSupplier> list)
        {
            return list
                .Select(x => new DropdownItemDto
                {
                    Value = x.location_id,
                    Label = $"{x.location_name} ({x.location_id})"
                })
                .OrderBy(x => x.Label)
                .ToList();
        }
    }
}