namespace OneTime.Api.Models.UsersDto
{
	public record UserDto(int UserId, string Name, string Email, string password, int Role,int? ManagerId);
}
