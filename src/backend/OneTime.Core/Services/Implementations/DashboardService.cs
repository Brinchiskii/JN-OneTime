using OneTime.Core.Data.Interfaces;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using OneTime.Core.Services.Repository;
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

        public async Task<List<ProjectStatModel>> GetTeamStatsAsync(int managerId, DateOnly startDate, DateOnly endDate)
        {

            if (startDate > endDate)
            {
                throw new ArgumentException("End date have to be after start date.");
            }

            return await _dashboardRepository.GetTeamPerformanceAsync(managerId, startDate, endDate);
        }
        public async Task<List<ProjectStatModel>> GetUserStatsAsync(int userId, DateOnly startDate, DateOnly endDate)
        {
            if (userId <= 0) throw new ArgumentException("Ugyldigt ID");

            return await _dashboardRepository.GetUserStatsAsync(userId, startDate, endDate);
        }
    }
}
