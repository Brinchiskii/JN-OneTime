using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.TimeEntriesDto;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TimeEntriesController : ControllerBase
	{
		private readonly ITimeEntryRepository _timeEntryRepo;
		private readonly IProjectRepository _projectRepo;

		public TimeEntriesController(ITimeEntryRepository timeEntryRepo, IProjectRepository projectRepo)
		{
			_timeEntryRepo = timeEntryRepo;
			_projectRepo = projectRepo;
		}


		/// <summary>
		/// Opretter en ny time entry.
		/// </summary>
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

				var project = await _projectRepo.GetById(entity.ProjectId);
				if (project == null)
					return BadRequest("Projekt not found");

				if (entity.Hours <= 0 || entity.Hours > 24)
					return BadRequest("Hours must be greater than zero and less than 24");

				entity.Status = (int)TimeEntryStatus.Pending;
				entity.Date = entity.Date == default ? DateOnly.FromDateTime(DateTime.Now) : entity.Date;

				var created = await _timeEntryRepo.Add(entity);

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
			var entries = await _timeEntryRepo.GetByUserWithDetails(userId);

			if (!entries.Any())
				return NoContent();

			var response = entries.Select(TimeEntryConverter.ToDetailsDto).ToList();

			return Ok(response);
		}
	}
}
