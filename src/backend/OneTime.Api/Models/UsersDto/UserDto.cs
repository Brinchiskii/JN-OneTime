namespace OneTime.Api.Models.UsersDto
{
	public record UserDto(int UserId, string Name, string Email, int Role,int? ManagerId);
}
