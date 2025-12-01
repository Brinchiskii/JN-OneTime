using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using OneTime.Core.Services.Repository;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OneTime.Api.Controllers
{
    /// <summary>
    /// Handles time entry related operations through API endpoints.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ITimeEntryService _service;

        /// <summary>
        /// Initializes a new instance of the TimeEntriesController class using the specified time entry service.
        /// </summary>
        /// <param name="timeEntryService">The time entry service.</param>
        public TimeEntriesController(ITimeEntryService timeEntryService)
        {
			_service = timeEntryService;
		}

        /// <summary>
        /// Retrieves a list of all available projects.
        /// </summary>
        /// <returns>
		/// Returns 200 OK with JSON array of projects if any exist.
		/// Return 204 No Content if no projects are found.
		/// </returns>
        [HttpGet("projects")]
        [ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetAvailableProjects()
		{
			var projects = await _service.GetAvailableProjects();

			return !projects.Any() ? NoContent() : Ok(projects);
		}

        /// <summary>
        /// Creates a new time entry based on the provided data.
        /// </summary>
        /// <param name="dto">The data used to create the time entry.</param>
        /// <returns>
		/// Return 200 OK with the created time entry in JSON format.
		/// Return 400 Bad Request if the input data is invalid or an error occurs.
		/// </returns>
        [HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> CreateTimeEntry([FromBody] TimeEntryCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			try
			{
				var entity = TimeEntryConverter.ToEntity(dto);

				var created = await _service.CreateTimeEntry(entity);

				var response = TimeEntryConverter.ToDto(created);

				return Ok(response);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("user/{userId}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetTimeEntriesForUser(int userId)
		{
			var entries = await _service.GetTimeEntriesForUser(userId);

			if (!entries.Any())
				return NoContent();

			var response = entries.Select(TimeEntryConverter.ToDetailsDto).ToList();

			return Ok(response);
		}

		[HttpGet("leader/{leaderId}/team")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetLeaderPeriodProjectsOverview(int leaderId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
		{
			var entries = await _service.GetLeaderPeriodEntries(leaderId, startDate, endDate);

			if (!entries.Any())
				return NoContent();

			var usersDict = entries
				.GroupBy(e => e.User!.Name) 
				.ToDictionary(
					userGroup => userGroup.Key,
					userGroup =>
						userGroup
							.GroupBy(e => new { e.ProjectId, ProjectName = e.Project!.Name, ProjectStatus = (int)e.Project.Status })
							.Select(projectGroup =>
							{
								var hoursByDate = new Dictionary<string, decimal>();

								foreach (var entry in projectGroup)
								{
									var key = entry.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

									if (!hoursByDate.ContainsKey(key))
										hoursByDate[key] = 0;

									hoursByDate[key] += entry.Hours;
								}

								return new ProjectHoursByDateDto(
									new ProjectSimpleDto(projectGroup.Key.ProjectId, projectGroup.Key.ProjectName, projectGroup.Key.ProjectStatus),
									hoursByDate
								);
							})
							.ToList()
				);

			var response = new LeaderUsersProjectsResponseDto(usersDict);

			return Ok(response);
		}
	}
}
