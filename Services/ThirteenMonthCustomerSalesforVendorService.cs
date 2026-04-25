using ECNREPORTAPI.Models;
using System.Data;
namespace ECNREPORTAPI.Services
{
    public class ThirteenMonthCustomerSalesforVendorService
    {
        private readonly IConfiguration _config;
        public ThirteenMonthCustomerSalesforVendorService(IConfiguration config) { _config = config; }

        public async Task<ThirteenMonthCustomerSalesforVendor> GetAsync(string compId, string repId, int supplierId)
        {
            return await new ThirteenMonthCustomerSalesforVendor(_config).GetDataAsync(compId, repId, supplierId);
        }

        public async Task<DataTable> GetDataTableAsync(string compId, string repId, int supplierId)
        {
            var model = new ThirteenMonthCustomerSalesforVendor(_config);
            var result = await model.GetDataAsync(compId, repId, supplierId);
            return model.GetDataTableFromList(result.Data);
        }
    }
}