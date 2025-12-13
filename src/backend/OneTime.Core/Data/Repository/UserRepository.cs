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
            
            return user;
        }
        
        public async Task<JNUser> GetByEmail(string email)
        {
            // Case-insensitive email lookup to align with common expectations and tests
            var normalized = email.ToLower();
            return await _context.JNUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
        }

        public async Task<JNUser> Create(JNUser user)
        {
			_context.JNUsers.Add(user);
			await _context.SaveChangesAsync();

			return user;
		}

        public async Task<JNUser> Update(JNUser user, string name,string email, UserRole role, int? managerId)
        {
            // Update user details
            user.Name = name;
            user.Email = email;
			user.Role = (int)role;
            user.ManagerId = managerId;

            //_context.JNUsers.Update(user); <-- dont think this is needed
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<JNUser> Delete(int id)
        {
            var user = await _context.JNUsers.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            _context.JNUsers.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> CheckManagersTeam(int managerId)
        {
            return await _context.JNUsers.AnyAsync(u => u.ManagerId == managerId);
        }
    }
}
