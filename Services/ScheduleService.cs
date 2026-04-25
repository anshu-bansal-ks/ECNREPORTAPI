//Services/ScheduleServices
using ECNREPORTAPI.Models;

namespace ECNREPORTAPI.Services
{
    public class ScheduleService
    {
        private readonly IConfiguration _config;

        public ScheduleService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> SaveScheduleAsync(ScheduleReport model, string compId, int userId)
        {
            return model.Save(compId, userId, _config);
        }
    }
}