using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<ProjectStatModel>> GetTeamProjectStatsAsync(int managerId, DateOnly startDate, DateOnly endDate);
    }
}
