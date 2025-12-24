using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models;
using OneTime.Api.Models.UsersDto;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
    	private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}


		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetAll()
		{
			var users = await _userService.GetAllUsers();

			if (!users.Any())
				return NoContent();

			var response = users.Select(UserConverter.ToDto).ToList();

			return Ok(response);
		}

		[HttpGet("{id:int}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> GetById(int id)
		{
			try
			{
				var user = await _userService.GetUserById(id);
				var response = UserConverter.ToDto(user);
				return Ok(response);
			}
			catch (InvalidOperationException)
			{
				return NotFound("User not found.");
			}
		}

		[HttpPost]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				if (!Enum.IsDefined(typeof(UserRole), dto.Role))
					return BadRequest("Invalid role value.");

				var roleEnum = (UserRole)dto.Role;

				 var user = await _userService.Create(
                     dto.Name,
                     dto.Email,
                     dto.Password,
                     roleEnum,
                     dto.ManagerId
                 );

				var response = UserConverter.ToDto(user);

				return CreatedAtAction(nameof(GetById),
					new { id = response.UserId },
					response);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPut("{id:int}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				if (!Enum.IsDefined(typeof(UserRole), dto.Role))
					return BadRequest("Invalid role value.");

				var roleEnum = (UserRole)dto.Role;

				var user = await _userService.Update(
                    id,
                    dto.Name,
                    dto.Email,
                    roleEnum,
                    dto.ManagerId
                );

				var response = UserConverter.ToDto(user);

				return Ok(response);
			}
			catch (InvalidOperationException ex)
			{
				if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
					return NotFound(ex.Message);

				return BadRequest(ex.Message);
			}
		}

		[HttpDelete("{id:int}")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _userService.Delete(id);

				return Ok(new
				{
					Message = $"User with ID {id} was successfully deleted."
				});
			}
			catch (InvalidOperationException ex)
			{
				if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
					return NotFound(ex.Message);

				return BadRequest(ex.Message);
			}
		}
	}
}
