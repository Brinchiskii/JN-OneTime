using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Core.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<JNUser> Create(string name, string email, string password,UserRole role, int? managerId);
        Task Delete(int id);
        Task<IEnumerable<JNUser>> GetAll();
        Task<JNUser> GetById(int id);
        Task<JNUser> Update(int id, string name, string email, UserRole role, int? managerId);
    }
}