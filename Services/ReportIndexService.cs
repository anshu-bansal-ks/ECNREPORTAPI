
using ECNREPOTINGPORTAL.Models;

namespace ECNREPORTAPI.Services
{
    public class ReportIndexService
    {
        private readonly IConfiguration _config;

        public ReportIndexService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<ReportIndex>> GetUserReportsAsync(int userId, string? keyword = "")
        {
            var model = new ReportIndex();
            var result = model.GetReports(userId, keyword, _config);

            // Ab async hai aur await bhi use kar sakte hain (future-proof)
            return await Task.FromResult(result);
        }
    }
}