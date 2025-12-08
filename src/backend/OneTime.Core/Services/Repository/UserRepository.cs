using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly OneTimeContext _context;

        public UserRepository(OneTimeContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JNUser>> GetAll()
        {
            return await _context.JNUsers
                .Include(u => u.Manager)
                //.Include(u => u.TeamMembers)
                .ToListAsync();
        }

        public async Task<JNUser> GetById(int id)
        {
            var user = await _context.JNUsers
                .Include(u => u.Manager)
                //.Include(u => u.TeamMembers)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            return user;
        }

        public async Task<JNUser> Create(string name, string email, string password ,UserRole role, int? managerId)
        {
			if (await _context.JNUsers.AnyAsync(u => u.Email == email))
				throw new InvalidOperationException("Email is already in use.");

			ValidateRoleAndManager(role, managerId);

			var user = new JNUser
			{
				Name = name,
				Email = email,
				Role = (int)role,
				ManagerId = managerId,
				// midlertidig løsning uden password hashing... vi hasher når vi implementerer login
				PasswordHash = password,
				PasswordSalt = string.Empty
			};

			_context.JNUsers.Add(user);
			await _context.SaveChangesAsync();

			return user;
		}
        public async Task<JNUser> Update(int id, string name,string email, UserRole role, int? managerId)
        {
            var user = await _context.JNUsers.FindAsync(id);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Check email
            var other = await _context.JNUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (other != null && other.UserId != id)
                throw new InvalidOperationException("Email already used by another user.");

            ValidateRoleAndManager(role, managerId);

            user.Name = name;
            user.Email = email;
			user.Role = (int)role;
            user.ManagerId = managerId;

            _context.JNUsers.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }
        public async Task Delete(int id)
        {
            var user = await _context.JNUsers.FindAsync(id);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            // mnager må ikke slettes hvis han har medarbejdere
            if ((int)user.Role == (int)UserRole.Manager)
            {
                bool hasTeam = await _context.JNUsers.AnyAsync(u => u.ManagerId == id);
                if (hasTeam)
                    throw new InvalidOperationException("Cannot delete manager with employees assigned.");
            }

            _context.JNUsers.Remove(user);
            await _context.SaveChangesAsync();
        }
        private static void ValidateRoleAndManager(UserRole role, int? managerId)
        {
            switch (role)
            {
                case UserRole.Admin:
                case UserRole.Manager:
                    if (managerId.HasValue)
                        throw new InvalidOperationException("Admins/managers cannot have a manager.");
                    break;

                case UserRole.Employee:
                    if (!managerId.HasValue)
                        throw new InvalidOperationException("Employees must have a manager.");
                    break;
            }
        }
    }
}
