using ECNREPORTAPI.Models;
using Microsoft.Extensions.Configuration;

namespace ECNREPORTAPI.Services
{
    public class AfterHoursUsersReportService
    {
        private readonly IConfiguration _config;

        public AfterHoursUsersReportService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<List<AfterHoursUsersReport>> GetReportAsync(
            string? fromDate = null,
            string? tillDate = null,
            string? timePeriod = null)
        {
            // Call static async method from model
            var data = await AfterHoursUsersReport.GetDataAsync(_config, fromDate, tillDate, timePeriod);
            return data;
        }
    }
}
