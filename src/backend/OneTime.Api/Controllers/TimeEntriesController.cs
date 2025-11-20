using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using OneTime.Core.Services.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OneTime.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ITimeEntryService _service;
        public TimeEntriesController(ITimeEntryService timeEntryService)
        {
			_service = timeEntryService;
		}

        [HttpGet("projects")]
        [ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetAvailableProjects()
		{
			var projects = await _service.GetAvailableProjects();

			return !projects.Any() ? NoContent() : Ok(projects);
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

	}
}
