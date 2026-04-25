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
            // ✅ FIX: config pass karo constructor me
            ItemDetails model = new ItemDetails(_config);

            // ✅ FIX: sirf 2 params
            return await Task.Run(() => model.GetData(compId, itemIdList));
        }
    }
}