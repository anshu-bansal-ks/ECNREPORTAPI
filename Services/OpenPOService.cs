 using ECNREPORTAPI.Models;

// namespace ECNREPORTAPI.Services
// {
//     public class OpenPOService
//     {
//         private readonly IConfiguration _config;
//         public OpenPOService(IConfiguration config) { _config = config; }

//         public async Task<List<OpenPO>> GetOpenPOAsync(string compId, string? supplierId, int page, int size)
//         {
//             return await new OpenPO().GetDataAsync(compId, supplierId, page, size, _config);
//         }

//         public async Task<List<OpenPO>> GetOpenPOAsync(string compId, string? supplierId, int page, int size)
//         {
//             // Model ka wahi method use karein jo pehle discuss kiya tha
//             return await new OpenPO().GetDataAsync(compId, supplierId, page, size, _config);
//         }
//     }
// }
namespace ECNREPORTAPI.Services
{
    public class OpenPOService
    {
        private readonly IConfiguration _config;
        public OpenPOService(IConfiguration config) { _config = config; }

        public async Task<List<Models.OpenPO>> GetOpenPOAsync(string compId, string? supplierId, int page, int size)
        {
            return await new Models.OpenPO().GetDataAsync(compId, supplierId, page, size, _config);
        }
    }
}