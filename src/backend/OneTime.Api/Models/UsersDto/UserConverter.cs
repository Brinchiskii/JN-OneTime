using OneTime.Core.Models;

namespace OneTime.Api.Models.UsersDto
{
	public static class UserConverter
	{
		public static UserDto ToDto(JNUser user)
			=> new UserDto(
				user.UserId,
				user.Name,
				user.Email,
				(int)user.Role, 
				user.ManagerId
			);
	}
}
