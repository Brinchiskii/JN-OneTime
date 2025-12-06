using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.TimeEntries;
using OneTime.Api.Models.TimeEntriesDto;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using System.Globalization;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TimeEntriesController : ControllerBase
	{
		private readonly ITimeEntryRepository _timeEntryRepository;
		private readonly IProjectRepository _projectRepository;

		public TimeEntriesController(ITimeEntryRepository timeEntryRepository, IProjectRepository projectRepository)
		{
			_timeEntryRepository = timeEntryRepository;
			_projectRepository = projectRepository;
		}

		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> CreateTimeEntry([FromBody] TimeEntryCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var project = await _projectRepository.GetById(dto.ProjectId);
				if (project == null)
					return BadRequest("Project not found");

				if (dto.Hours <= 0 || dto.Hours > 24)
					return BadRequest("Hours must be greater than zero and less than 24");

				var entity = TimeEntryConverter.ToEntity(dto);

				entity.Status = (int)TimeEntryStatus.Pending;
				entity.Date = entity.Date == default ? DateOnly.FromDateTime(DateTime.Now) : entity.Date;

				var created = await _timeEntryRepository.Add(entity);

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
			var entries = await _timeEntryRepository.GetByUserWithDetails(userId);

			if (!entries.Any())
				return NoContent();

			var response = entries.Select(TimeEntryConverter.ToDetailsDto).ToList();

			return Ok(response);
		}

		[HttpGet("leader/{leaderId}/team")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetLeaderPeriodProjectsOverview(
			int leaderId,
			[FromQuery] DateOnly startDate,
			[FromQuery] DateOnly endDate)
		{
			var entries = await _timeEntryRepository
				.GetLeaderWithDetailsForPeriod(leaderId, startDate, endDate);

			if (!entries.Any())
				return NoContent();

			var usersDict = entries
				.GroupBy(e => e.User!.Name)
				.ToDictionary(
					userGroup => userGroup.Key,
					userGroup =>
						userGroup
							.GroupBy(e => new
							{
								e.ProjectId,
								ProjectName = e.Project!.Name,
								ProjectStatus = (int)e.Project.Status
							})
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
									new ProjectSimpleDto(
										projectGroup.Key.ProjectId,
										projectGroup.Key.ProjectName,
										projectGroup.Key.ProjectStatus),
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
