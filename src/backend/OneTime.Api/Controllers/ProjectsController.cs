using Microsoft.AspNetCore.Mvc;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProjectsController : ControllerBase
	{
		private readonly IProjectRepository _projectRepository;

		public ProjectsController(IProjectRepository projectRepository)
		{
			_projectRepository = projectRepository;
		}


		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetAll()
		{
			var projects = await _projectRepository.GetAll();

			if (!projects.Any())
				return NoContent();

			return Ok(projects);
		}
	}
}
