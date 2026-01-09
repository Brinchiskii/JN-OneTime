using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Data.Interfaces
{
    public interface IDashboardRepository
    {
        Task<List<ProjectStatModel>> GetTeamPerformanceAsync(int managerId, DateOnly startDate, DateOnly endDate);
        Task<List<ProjectStatModel>> GetUserStatsAsync(int userId, DateOnly startDate, DateOnly endDate);
    }
}
