namespace OneTime.Api.Models.UsersDto
{
	public record UserCreateDto(string Name,string Email,string Password, int Role, int? ManagerId);
}
