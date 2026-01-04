using OneTime.Core.Data.Interfaces;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<List<ProjectStatModel>> GetTeamProjectStatsAsync(int managerId, DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Slutdato skal være efter startdato");
            }

            return await _dashboardRepository.GetTeamPerformanceAsync(managerId, startDate, endDate);
        }
    }
}
