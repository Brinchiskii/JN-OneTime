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