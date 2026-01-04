using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.ProjectsDto;
using OneTime.Core.Data.Interfaces;
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
        public async Task<IActionResult> GetTeamProjectStats(int managerId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var domainStats = await _dashboardService.GetTeamProjectStatsAsync(managerId, startDate, endDate);

            var response = domainStats.Select(s => new ProjectPerformanceDto(
                s.ProjectId,
                s.ProjectName,
                s.Status,
                s.TotalHours, 
                s.Members.Select(m => new ProjectMemberDto(m.UserId, m.Name, m.Hours)).ToList()
            )).ToList();

            return Ok(response);
        }
    }
}
