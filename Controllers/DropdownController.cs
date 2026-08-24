// Controlllers/DropdownControlller
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECNREPORTAPI.Services;
using ECNREPORTAPI.Dtos;
using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DropdownController : ControllerBase
    {
        private readonly DropdownService _svc;
        private readonly IConfiguration _config;

        public DropdownController(DropdownService svc, IConfiguration config)
        {
            _svc = svc;
            _config = config;
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
        public async Task<IActionResult> Locations([FromQuery] string compId,[FromQuery] string? locType = "WAREHOUSE")
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var locations = await _svc.GetLocationListAsync(compId, locType ?? "WAREHOUSE");

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

        [HttpGet("periods")]
        public IActionResult GetPeriods()
        {
            var commonSvc = new Common(_config); 
            var periods = commonSvc.gePeriodList();
            
            var dropdownData = periods.Select(p => new {
                periodName = p.Period,        
                startDate = p.PeriodStartDate,  
                endDate = p.PeriodEndDate       
            }).ToList();

            return Ok(dropdownData);
        }

        [HttpGet("salsify/mcat")]
        [AllowAnonymous]
        public async Task<IActionResult> SalsifyMcat()
        {
            var data = await _svc.GetSalsifyMcatAsync();
            return Ok(data);
        }

        [HttpGet("salsify/scat")]
        [AllowAnonymous]
        public async Task<IActionResult> SalsifyScat([FromQuery] string? mcat)
        {
            var data = await _svc.GetSalsifyScatAsync(mcat);
            return Ok(data);
        }

        [HttpGet("itemcategories")]
        [AllowAnonymous]
        public async Task<IActionResult> ItemCategories([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetLnkItemCategoriesAsync(compId);

            var dropdown = _svc.ToDropdownFromItemCategories(data);

            return Ok(dropdown);
        }

        [HttpGet("pricepages")]
        [AllowAnonymous]
        public async Task<IActionResult> PricePages(
            [FromQuery] string compId,
            [FromQuery] string supplierId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            if (string.IsNullOrWhiteSpace(supplierId))
                return BadRequest("supplierId required");

            var data = await _svc.GetPricePageListAsync(compId, supplierId);

            var dropdown = _svc.ToDropdownFromPricePages(data);

            return Ok(dropdown);
        }

        [HttpGet("terms")]
        [AllowAnonymous]
        public async Task<IActionResult> Terms([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetTermsListAsync(compId);

            var dropdown = _svc.ToDropdownFromTerms(data);

            return Ok(dropdown);
        }

        [HttpGet("classnumbers")]
        [AllowAnonymous]
        public async Task<IActionResult> ClassNumbers([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetClassNumbersAsync(compId);

            var dropdown = _svc.ToDropdownFromClassNumbers(data);

            return Ok(dropdown);
        }

        [HttpGet("classid")]
        [AllowAnonymous]
        public async Task<IActionResult> ClassId(
            [FromQuery] string compId,
            [FromQuery] string classNumber)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            if (string.IsNullOrWhiteSpace(classNumber))
                return BadRequest("classNumber required");

            var data = await _svc.GetClassIdAsync(compId, classNumber);

            var dropdown = _svc.ToDropdownFromClassId(data);

            return Ok(dropdown);
        }

        [HttpGet("purchaseclass")]
        [AllowAnonymous]
        public async Task<IActionResult> PurchaseClass([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetPurchaseClassAsync(compId);

            var dropdown = _svc.ToDropdownFromPurchaseClass(data);

            return Ok(dropdown);
        }
        [HttpGet("productgroup")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductGroup([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetProductGroupAsync(compId);

            var dropdown = _svc.ToDropdownFromProductGroup(data);

            return Ok(dropdown);
        }

        [HttpGet("roles")]
        [AllowAnonymous]
        public async Task<IActionResult> Roles([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetRolesAsync(compId);

            var dropdown = _svc.ToDropdownFromRoles(data);

            return Ok(dropdown);
        }

        [HttpGet("buyer")]
        [AllowAnonymous]
        public async Task<IActionResult> Buyer([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetBuyerAsync(compId);

            var dropdown = _svc.ToDropdownFromBuyer(data);

            return Ok(dropdown);
        }

        [HttpGet("pricelibrary")]
        [AllowAnonymous]
        public async Task<IActionResult> PriceLibrary([FromQuery] string compId)
        {
            if (string.IsNullOrWhiteSpace(compId))
                return BadRequest("compId required");

            var data = await _svc.GetPriceLibraryAsync(compId);

            var dropdown = _svc.ToDropdownFromPriceLibrary(data);

            return Ok(dropdown);
        }

        [HttpGet("rolesrepots")]
        [AllowAnonymous]
        public async Task<IActionResult> RolesReports()
        {
            // if (string.IsNullOrWhiteSpace))
            //     return BadRequest("compId required");

            var data = await _svc.GetRolesReportsAsync();

            var dropdown = _svc.ToDropdownFromRolesReports(data);

            return Ok(dropdown);
        }

    }
}