using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OneTime.Core.Services.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly IUserRepository _users;
		private readonly IPasswordHasher _hasher;
		private readonly IConfiguration _config;

		public AuthService(IUserRepository users, IPasswordHasher hasher, IConfiguration config)
		{
			_users = users;
			_hasher = hasher;
			_config = config;
		}

		public async Task<JNUser> Register(string name, string email, string password ,UserRole role, int? managerId)
		{
			var existing = await _users.GetByEmail(email);
			if (existing != null)
				throw new InvalidOperationException("Email is already in use.");

			if (role == UserRole.Employee && !managerId.HasValue)
				throw new InvalidOperationException("Employees must have a manager.");
			if (role != UserRole.Employee && managerId.HasValue)
				throw new InvalidOperationException("Only employees can have a manager.");

			var (hash, generatedSalt) = _hasher.HashPassword(password);

			return await _users.Create(name, email, hash, generatedSalt, role, managerId);
		}

		public async Task<(JNUser User, string Token)> Login(string email, string password)
		{
			var user = await _users.GetByEmail(email);
			if (user == null)
				throw new InvalidOperationException("Invalid credentials.");

			var ok = _hasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
			if (!ok)
				throw new InvalidOperationException("Invalid credentials.");

			var token = GenerateJwt(user);
			return (user, token);
		}

		private string GenerateJwt(JNUser user)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
			new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
			new Claim(ClaimTypes.Name, user.Name),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Role, ((UserRole)user.Role).ToString())
        };

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
