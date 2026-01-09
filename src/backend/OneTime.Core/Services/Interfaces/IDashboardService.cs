using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<ProjectStatModel>> GetTeamStatsAsync(int managerId, DateOnly startDate, DateOnly endDate);
        Task<List<ProjectStatModel>> GetUserStatsAsync(int userId, DateOnly startDate, DateOnly endDate);
    }
}
