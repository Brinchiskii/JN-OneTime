using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.ProjectsDto;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
     public class ProjectsController : ControllerBase
     {
         private readonly IProjectService _projectService;

         public ProjectsController(IProjectService projectService)
         {
             _projectService = projectService;
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
              var projects = await _projectService.GetAll();

              if (!projects.Any())
                  return NoContent();

              return Ok(projects);
          }
          
                [HttpPost]
		        [ProducesResponseType(201)]
		        [ProducesResponseType(400)]
          public async Task<IActionResult> Create([FromBody] ProjectCreateDto dto)
          {
              if (!ModelState.IsValid)
                  return BadRequest(ModelState);

              try
              {
                  var created = await _projectService.Create(dto.Name, (ProjectStatus)dto.Status);
                  return CreatedAtAction(nameof(GetById), new { id = created.ProjectId }, created);
              }
              catch (InvalidOperationException ex)
              {
                  return BadRequest(ex.Message);
              }
          }

          [HttpDelete("{id:int}")]
          [ProducesResponseType(204)]
          [ProducesResponseType(400)]
          [ProducesResponseType(404)]
          public async Task<IActionResult> Delete(int id)
          {
              try
              {
                  await _projectService.Delete(id);
                  return Ok(new
                  {
                      Message = $"Project with ID {id} was successfully deleted."
                  });
              }
              catch (InvalidOperationException ex)
              {
                  if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                      return NotFound(ex.Message);

                  return BadRequest(ex.Message);
              }
          }

          [HttpPut("{id:int}")]
          [ProducesResponseType(200)]
          [ProducesResponseType(400)]
          [ProducesResponseType(404)]
          public async Task<IActionResult> Update(int id, [FromBody] ProjectUpdateDto dto)
          {
              if (!ModelState.IsValid) return BadRequest(ModelState);

              try
              {
                  var updated = await _projectService.Update(id, dto.Name, (ProjectStatus)dto.Status);
                  return Ok(updated);
              }
              catch (InvalidOperationException ex)
              {
                  if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                      return NotFound(ex.Message);
                  return BadRequest(ex.Message);
              }
          }

          [HttpGet("{id}")]
          [ProducesResponseType(200)]
          [ProducesResponseType(404)]
          public async Task<IActionResult> GetById(int id)
          {
              try
              {
                  var project = await _projectService.GetById(id);
                  if (project == null)
                      return NotFound();
                  return Ok(project);
              }
              catch (InvalidOperationException ex)
              {
                  if (ex.Message.Contains("Project not found.", StringComparison.OrdinalIgnoreCase))
                      return NotFound(ex.Message);
                  return BadRequest(ex.Message);
              }
          } 
     }
}
