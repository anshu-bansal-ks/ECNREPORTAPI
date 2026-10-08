using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class ListOfSkusUpcsPricesCostsService
    {
        private readonly IConfiguration _config;
        private readonly DropdownService _dropdownService;

        public ListOfSkusUpcsPricesCostsService(IConfiguration config, DropdownService dropdownService)
        {
            _config = config;
            _dropdownService = dropdownService;
        }

        public async Task<List<ListOfSkusUpcsPricesCosts>> GetAsync(string compId, string itemIdList)
        {
            string locationList = await _dropdownService.GetLocationByListAsync(compId, "WAREHOUSE");

            var model = new ListOfSkusUpcsPricesCosts(_config);
            return model.GetData(compId, itemIdList, locationList);
        }
    }
}