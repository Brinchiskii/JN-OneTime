using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Interfaces
{
	public interface IPasswordHasher
	{
		(string Hash, string Salt) HashPassword(string password);
		bool VerifyPassword(string password, string hashBase64, string saltBase64);
	}
}
