using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneTime.Core.Data.Interfaces;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Data.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly OneTimeContext _context;

        public DashboardRepository(OneTimeContext context)
        {
            _context = context;
        }
        public async Task<List<ProjectStatModel>> GetTeamPerformanceAsync(int managerId, DateOnly startDate, DateOnly endDate)
        {
            var rawData = await _context.TimeEntries
                .Include(t => t.Project) 
                .Include(t => t.User)    
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .Where(t => t.User.ManagerId == managerId)
                .ToListAsync();

            var result = rawData
                .GroupBy(t => t.Project)
                .Select(g => new ProjectStatModel
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name, 
                    Status = g.Key.Status, 

                    TotalHours = g.Sum(t => t.Hours),

                    Members = g.GroupBy(t => t.User) 
                        .Select(ug => new ProjectMemberStat
                        {
                            UserId = ug.Key.UserId,
                            Name = ug.Key.Name,
                            Hours = ug.Sum(t => t.Hours)
                        })
                        .ToList() 
                })
                .ToList();

            return result;
        }
    }
}
