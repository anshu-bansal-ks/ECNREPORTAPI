using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class ItemDetailsWithInventoryQuantitiesService
    {
        private readonly IConfiguration _config;

        public ItemDetailsWithInventoryQuantitiesService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<ItemDetailsWithInventoryQuantities>> GetItemDetailsAsync(string compId, string itemIdList)
        {
            var model = new ItemDetailsWithInventoryQuantities(_config);
            return await Task.Run(() => model.GetData(compId, itemIdList));
        }
    }
}