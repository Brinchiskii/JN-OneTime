using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.AuthDto;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthsController : ControllerBase
{
	private readonly IAuthService _auth;

	public AuthsController(IAuthService auth)
	{
		_auth = auth;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterDto dto)
	{
		try
		{
			var user = await _auth.Register(dto.Name, dto.Email, dto.Password, dto.Role, dto.ManagerId);
			return Ok(new { user.UserId, user.Name, user.Email, user.Role, user.ManagerId });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginDto dto)
	{
		try
		{
			var (user, token) = await _auth.Login(dto.Email, dto.Password);

			return Ok(new AuthResponseDto(
				token,
				user.UserId,
				user.Name,
				user.Email,
				user.Role
			));
		}
		catch (InvalidOperationException)
		{
			return Unauthorized("Invalid credentials.");
		}
	}
}