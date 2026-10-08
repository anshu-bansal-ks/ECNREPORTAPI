using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class ItemDetailsService
    {
        private readonly IConfiguration _config;

        public ItemDetailsService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<ItemDetails>> GetItemDetailsAsync(string compId, string itemIdList)
        {
            ItemDetails model = new ItemDetails(_config);

            return await Task.Run(() => model.GetData(compId, itemIdList));
        }
        
        
    }
}