using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Interfaces
{
	public interface IAuthService
	{
		Task<JNUser> Register(string name, string email, string password, UserRole role, int? managerId);
		Task<(JNUser User, string Token)> Login(string email, string password);
	}
}
