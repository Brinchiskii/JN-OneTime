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
		private readonly ITimeEntryService _timeEntryService;

		public TimeEntriesController(ITimeEntryService timeEntryService)
		{
			_timeEntryService = timeEntryService;
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

				entity.Date = entity.Date == default ? DateOnly.FromDateTime(DateTime.Now) : entity.Date;

				var created = await _timeEntryService.CreateTimeEntry(entity);

				var response = TimeEntryConverter.ToDto(created);
				return Ok(response);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Henter time entries for en given bruger med projekt- og bruger-detaljer.
		/// </summary>
		[HttpGet("user/{userId}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetTimeEntriesForUser(int userId)
		{
			var entries = await _timeEntryService.GetTimeEntriesByUserWithDetails(userId);

			if (!entries.Any())
				return NoContent();

			var response = entries.Select(TimeEntryConverter.ToDetailsDto).ToList();

			return Ok(response);
		}
	}
}
