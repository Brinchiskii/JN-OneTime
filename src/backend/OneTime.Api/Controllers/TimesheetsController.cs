using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models;
using OneTime.Api.Models.TimesheetsDto;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers
{
    /// <summary>
    /// Handles monthly review related operations through API endpoints.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TimesheetsController : ControllerBase
    {
        private readonly ITimesheetService _service;

        public TimesheetsController(ITimesheetService timesheetService)
        {
            _service = timesheetService;
        }

        /// <summary>
        /// Submits a monthly review for a user within a specified period.
        /// </summary>
        /// <param name="dto">The data used to submit the monthly review</param>
        /// <returns>
        /// Return 200 OK with the submitted monthly review in JSON format.
        /// Return 400 Bad Request if the input data is invalid or an error occurs.
        /// </returns>
        [HttpPost("submit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Submit([FromBody] SubmitTimesheetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var review = await _service.CreateTimesheet(dto.UserId, dto.PeriodStart, dto.PeriodEnd);

                var response = new TimesheetDto(
                    review.TimesheetId,
                    review.UserId,
                    review.PeriodStart,
                    review.PeriodEnd,
                    (TimesheetStatus)review.Status,
                    review.DecidedAt,
                    review.Comment
                );

                return Ok(response);
            } catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

		[HttpPost("update/{timesheetId:int}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> Update(int timesheetId, [FromBody] TimesheetDecisionDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var sheet = await _service.UpdateTimeSheet(timesheetId, dto.Status, dto.Comment, dto.LeaderId);

				var response = new TimesheetDto(
					sheet.TimesheetId,
					sheet.UserId,
					sheet.PeriodStart,
					sheet.PeriodEnd,
					(TimesheetStatus)sheet.Status,
					sheet.DecidedAt,
					sheet.Comment
				);

				return Ok(response);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("leader/{leaderId}/team/")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetTimeentriesForPendingTimesheet(int leaderId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
		{
			var entries = await _service.GetTimeentriesForPendingTimesheet(leaderId, startDate, endDate);

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
							}).ToList()
				);

			var response = new LeaderUsersProjectsResponseDto(usersDict);

			return Ok(response);
		}
	}
}
