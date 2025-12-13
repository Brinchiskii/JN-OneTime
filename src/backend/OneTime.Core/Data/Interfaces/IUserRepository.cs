using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Core.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<JNUser> Create(JNUser user);
        Task<JNUser> Delete(int id);
        Task<IEnumerable<JNUser>> GetAll();
        Task<JNUser> GetById(int id);
        Task<JNUser> GetByEmail(string email);
        Task<JNUser> Update(JNUser user, string name, string email, UserRole role, int? managerId);
        Task<bool> CheckManagersTeam(int managerId);
    }
}