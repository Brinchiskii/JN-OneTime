using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.TimeSheetDto;
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
        private readonly ITimesheetRepository _service;

        public TimesheetsController(ITimesheetRepository monthlyReviewService)
        {
            _service = monthlyReviewService;
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
                var review = await _service.SubmitMonthlyReviewAsync(dto.UserId, dto.PeriodStart, dto.PeriodEnd);

                var response = new TimesheetDto(
                    review.TimesheetId,
                    review.UserId,
                    review.PeriodStart,
                    review.PeriodEnd,
                    review.Status,
                    review.DecidedAt,
                    review.Comment
                );

                return Ok(response);
            } catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

		[HttpPost("decide")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> Decide([FromBody] TimesheetDecisionDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var sheet = await _service.UpdateTimeSheet(dto.TimesheetId, dto.LeaderId, dto.Status, dto.Comment);

				var response = new TimesheetDto(
					sheet.TimesheetId,
					sheet.UserId,
					sheet.PeriodStart,
					sheet.PeriodEnd,
					sheet.Status,
					sheet.DecidedAt,
					sheet.Comment
				);

				return Ok(response);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
