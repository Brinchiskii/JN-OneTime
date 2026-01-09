using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.ProjectsDto;
using OneTime.Core.Data.Interfaces;
using OneTime.Core.Models;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Interfaces;
using System;

namespace OneTime.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats/leader/{managerId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTeamStats(int managerId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var domainStats = await _dashboardService.GetTeamStatsAsync(managerId, startDate, endDate);

            var response = domainStats.Select(s => new ProjectPerformanceDto(
                s.ProjectId,
                s.ProjectName,
                s.Status,
                s.TotalHours, 
                s.Members.Select(m => new ProjectMemberDto(m.UserId, m.Name, m.Hours)).ToList()
            )).ToList();

            return Ok(response);
        }
        [HttpGet("stats/user/{userId}")]
        public async Task<IActionResult> GetUserStats(int userId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                List<ProjectStatModel> domainModels = await _dashboardService.GetUserStatsAsync(userId, startDate, endDate);

                var statsDtos = domainModels.Select(m => new UserStatsDto
                {
                    ProjectName = m.ProjectName,
                    Hours = m.TotalHours
                }).ToList();

                return Ok(statsDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Fejl: " + ex.Message);
            }
        }
    }
}
