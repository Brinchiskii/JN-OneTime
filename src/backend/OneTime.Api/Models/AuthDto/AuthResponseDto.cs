namespace OneTime.Api.Models.AuthDto
{
	public record AuthResponseDto(string Token, int UserId, string Name, string Email, int Role);
}
