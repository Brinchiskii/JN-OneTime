using OneTime.Core.Models.Enums;

namespace OneTime.Api.Models.AuthDto
{
	public record RegisterDto(string Name, string Email, string Password, UserRole Role, int? ManagerId);
}
