namespace OneTime.Api.Models.UsersDto
{
	public record UserUpdateDto(string Name,string Email, string Password, int Role, int? ManagerId);
}
