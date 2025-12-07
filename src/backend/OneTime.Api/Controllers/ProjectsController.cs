using Microsoft.AspNetCore.Mvc;
using OneTime.Core.Services.Interfaces;
using OneTime.Core.Models;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProjectsController : ControllerBase
	{
		private readonly IProjectRepository _projectRepo;

		public ProjectsController(IProjectRepository projectRepo)
		{
			_projectRepo = projectRepo;
		}

		/// <summary>
		/// Henter alle projekter.
		/// </summary>
		/// <returns>
		/// 200 OK med en liste af projekter, hvis der findes nogen.  
		/// 204 No Content, hvis der ikke er nogen projekter.
		/// </returns>
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetAll()
		{
			var projects = await _projectRepo.GetAll();

			if (!projects.Any())
				return NoContent();

			return Ok(projects);
		}
	}
}
