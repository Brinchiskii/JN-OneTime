using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Core.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<JNUser> Create(string name, string email, string password, string salt, UserRole role, int? managerId);
        Task Delete(int id);
        Task<JNUser> Create(JNUser user);
        Task<JNUser> Delete(int id);
        Task<IEnumerable<JNUser>> GetAll();
		Task<JNUser?> GetByEmail(string email);
		Task<JNUser> GetById(int id);
        Task<JNUser> Update(int id, string name, string email, UserRole role, int? managerId);
        Task<JNUser> GetById(int id);
        Task<JNUser> GetByEmail(string email);
        Task<JNUser> Update(JNUser user, string name, string email, UserRole role, int? managerId);
        Task<bool> CheckManagersTeam(int managerId);
    }
}