using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.ProjectsDto;
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
		/// Replaces all time entries for the specified timesheet with the provided collection of new entries.
		/// </summary>
		/// <remarks>All existing time entries for the specified timesheet are removed and replaced with the provided
		/// entries. The request body must contain a valid list of time entry objects. This operation is atomic; either all
		/// entries are replaced or none are if an error occurs.</remarks>
		/// <param name="timesheetId">The unique identifier of the timesheet for which time entries will be replaced.</param>
		/// <param name="entries">A list of time entry data transfer objects representing the new time entries to be saved. Cannot be null.</param>
		/// <returns>An IActionResult indicating the result of the operation. Returns 200 OK if the entries are saved successfully;
		/// otherwise, returns 400 Bad Request if the input is invalid or an error occurs.</returns>
		[HttpPost("bulk/{timesheetId}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> SaveTimeEntries(int timesheetId, [FromBody] List<TimeEntryCreateDto> entries)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			try
			{
                var entities = new List<TimeEntry>();

                foreach (var dto in entries)
                {
                    var entity = TimeEntryConverter.ToEntity(dto);
                    entity.TimesheetId = timesheetId;

                    entities.Add(entity);
                }
                await _timeEntryService.ReplaceTimeEntries(timesheetId, entities);

				return Ok();
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

		/// <summary>
		/// Henter alle brugerens timeentries med angivende timesheetid 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="timesheetId"></param>
		/// <returns></returns>
		[HttpGet("user/{userId}/timesheet/{timesheetId}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetTimeEntriesForTimesheet(int userId, int timesheetId)
		{
			var entries = await _timeEntryService.GetTimeEntriesForTimesheet(userId, timesheetId);
			var response = entries.Select(TimeEntryConverter.ToDetailsDto).ToList();
            return Ok(response);
		}
    }
}
