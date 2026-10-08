
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

        public async Task<List<ReportIndex>> GetUserReportsAsync(int userId, string? keyword = null)
        {
            var model = new ReportIndex();
            var result = model.GetReports(userId, keyword, _config);

            return await Task.FromResult(result);
        }
    }
}