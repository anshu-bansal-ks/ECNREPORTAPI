using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class CustomerTotalsService
    {
        private readonly IConfiguration _config;

        public CustomerTotalsService(IConfiguration config)
        {
            _config = config;
        }

        public Task<List<CustomerTotals>> GetCustomerTotalsAsync(string compId)
        {
            CustomerTotals model = new CustomerTotals();
            var data = model.GetData(compId, _config);

            return Task.FromResult(data);
        }
    }
}
