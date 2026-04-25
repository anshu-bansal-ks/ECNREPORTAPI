// Controlllers/DropdownControlller
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Dtos;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DropdownController : ControllerBase
    {
        private readonly DropdownService _svc;

        public DropdownController(DropdownService svc)
        {
            _svc = svc;
        }

        [HttpGet("companies")]
        [AllowAnonymous]  
        public async Task<IActionResult> Companies()
        {
            var companies = await _svc.GetCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("dbstring/{compCode}")]
        [Authorize]
        public async Task<IActionResult> DbString(string compCode) 
            => Ok(await _svc.GetDynamicConnectionString(compCode));

        // CUSTOMERS - Normal search
        [HttpGet("customers")]
        [Authorize]
        public async Task<IActionResult> Customers([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId is required");

            var all = await _svc.GetCustomersAsync(compId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var filtered = all
                    .Where(t => t.customer_name.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || t.customer_id.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.customer_name)
                    .Take(20)
                    .ToList();
                return Ok(filtered);
            }
            return Ok(all);
        }

        // CUSTOMERS VALIDATE
        [HttpGet("customers/validate")]
        [Authorize]
        public async Task<IActionResult> ValidateCustomer([FromQuery] string compId, [FromQuery] string custId)
        {
            if (string.IsNullOrWhiteSpace(compId) || string.IsNullOrWhiteSpace(custId))
                return BadRequest("compId and custId are required");

            var all = await _svc.GetCustomersAsync(compId);
            var matched = all
                .Where(t => string.Equals(t.customer_id, custId, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            return Ok(matched);
        }

        // CUSTOMERS BY REP
        [HttpGet("customers/byrep")]
        [Authorize]
        public async Task<IActionResult> CustomersByRep([FromQuery] string compId, [FromQuery] string repId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId) || string.IsNullOrWhiteSpace(repId))
                return BadRequest("compId and repId are required");

            var all = await _svc.GetCustomersByRepAsync(compId, repId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var filtered = all
                    .Where(t => t.customer_name.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || t.customer_id.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.customer_name)
                    .Take(20)
                    .ToList();
                return Ok(filtered);
            }

            return Ok(all);
        }

        // CUSTOMERS BY REP VALIDATE
        [HttpGet("customers/byrep/validate")]
        [Authorize]
        public async Task<IActionResult> ValidateCustomerByRep([FromQuery] string compId, [FromQuery] string repId, [FromQuery] string custId)
        {
            if (string.IsNullOrWhiteSpace(compId) || string.IsNullOrWhiteSpace(repId) || string.IsNullOrWhiteSpace(custId))
                return BadRequest("compId, repId, and custId are required");

            var all = await _svc.GetCustomersByRepAsync(compId, repId);
            var matched = all
                .Where(t => string.Equals(t.customer_id, custId, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            return Ok(matched);
        }

        // VENDORS - SEARCH + DROPDOWN
        [HttpGet("vendors")]
        [AllowAnonymous]  
        public async Task<IActionResult> Vendors([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId is required");

            var all = await _svc.GetVendorsAsync(compId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var filtered = all
                    .Where(t => t.vendor_name.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || t.vendor_id.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.vendor_name)
                    .Take(10)
                    .ToList();
                return Ok(filtered);
            }

            return Ok(all);
        }

        [HttpGet("vendors/dropdown")]
        [AllowAnonymous]  
        public async Task<IActionResult> VendorsDropdown([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId is required");

            var all = await _svc.GetVendorsAsync(compId);
            var dd = _svc.ToDropdownFromVendors(all);
            return Ok(dd);
        }
       
        [HttpGet("salesreps")]
        [AllowAnonymous]  
        public async Task<IActionResult> SalesReps([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var reps = await _svc.GetSalesRepsAsync(compId);  // ← _svc (not _service)

            if (!string.IsNullOrWhiteSpace(search))
            {
                reps = reps.Where(r => 
                    r.rep_name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.salesrep_id.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var dropdown = _svc.ToDropdownFromSalesReps(reps);
            return Ok(dropdown);
        }
        [HttpGet("suppliers")]
        [AllowAnonymous]
        public async Task<IActionResult> Suppliers([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var suppliers = await _svc.GetSuppliersAsync(compId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                suppliers = suppliers
                    .Where(s => 
                        s.supplier_name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        s.supplier_id.Contains(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .Take(20)
                    .OrderBy(s => s.supplier_name)
                    .ToList();
            }

            return Ok(suppliers); 
        }

        [HttpGet("suppliers/op")]
        [AllowAnonymous]
        public async Task<IActionResult> SuppliersOP([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var suppliers = await _svc.GetSuppliersOPAsync(compId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                suppliers = suppliers
                    .Where(s =>
                        s.supplier_name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        s.supplier_id.Contains(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .Take(20)
                    .OrderBy(s => s.supplier_name)
                    .ToList();
            }

            return Ok(suppliers);
        }
    
        [HttpGet("locations")]
        [AllowAnonymous]  
        public async Task<IActionResult> Locations([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var locations = await _svc.GetLocationsAsync(compId, "WAREHOUSE");

            var dropdown = _svc.ToDropdownFromLocations(locations);

            return Ok(dropdown);
        }
    
        [HttpGet("shows")]
        [AllowAnonymous]  
        public async Task<IActionResult> Shows([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetShowItemsAsync(compId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                data = data.Where(x => 
                    x.ShowName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.ShowId.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var dropdown = _svc.ToDropdownFromShows(data);
            return Ok(dropdown);
        }

        [HttpGet("promos")]
        [AllowAnonymous]  
        public async Task<IActionResult> Promos([FromQuery] string compId, [FromQuery] string? search)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetPromoItemsAsync(compId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                data = data.Where(x => 
                    x.PromoName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.PromoId.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var dropdown = _svc.ToDropdownFromPromos(data);
            return Ok(dropdown);
        }

        [HttpGet("location-supplier")]
        [AllowAnonymous]
        public async Task<IActionResult> LocationSupplier([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetLocationListSupplierAsync(compId);
            var dropdown = _svc.ToDropdownFromLocationSupplier(data);

            return Ok(dropdown);
        }

    }
}