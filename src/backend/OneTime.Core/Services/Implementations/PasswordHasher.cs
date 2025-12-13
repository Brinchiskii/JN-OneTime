using OneTime.Core.Services.Interfaces;
using System;
using System.Security.Cryptography;

namespace OneTime.Core.Services.Implementations
{
	public class PasswordHasher : IPasswordHasher
	{
		private const int SaltSize = 16;       
		private const int KeySize = 32;        
		private const int Iterations = 100_000; 

		private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

		public (string Hash, string Salt) HashPassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("Password cannot be empty", nameof(password));

			var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

			var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
				password: password,
				salt: saltBytes,
				iterations: Iterations,
				hashAlgorithm: Algorithm,
				outputLength: KeySize
			);

			return (Hash: Convert.ToBase64String(hashBytes), Salt: Convert.ToBase64String(saltBytes));
		}

		public bool VerifyPassword(string password, string hashBase64, string saltBase64)
		{
			if (string.IsNullOrWhiteSpace(password) ||
				string.IsNullOrWhiteSpace(hashBase64) ||
				string.IsNullOrWhiteSpace(saltBase64))
			{
				return false;
			}

			byte[] saltBytes, expectedHash;

			try
			{
				saltBytes = Convert.FromBase64String(saltBase64);
				expectedHash = Convert.FromBase64String(hashBase64);
			}
			catch (FormatException)
			{
				return false;
			}

			if (saltBytes.Length != SaltSize || expectedHash.Length != KeySize)
				return false;

			var computedHash = Rfc2898DeriveBytes.Pbkdf2(
				password: password,
				salt: saltBytes,
				iterations: Iterations,
				hashAlgorithm: Algorithm,
				outputLength: expectedHash.Length
			);

			return CryptographicOperations.FixedTimeEquals(expectedHash, computedHash);
		}
	}
}
