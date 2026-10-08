using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class AccountSuppressedFromAgingService
    {
        private readonly IConfiguration _config;

        public AccountSuppressedFromAgingService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<AccountSuppressedFromAgingCollectionModel>> GetAsync(string compId)
        {
            var model = new AccountSuppressedFromAgingCollectionModel(_config);
            return model.GetData(compId);
        }
    }
}