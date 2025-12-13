using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Core.Services.Interfaces;

public interface IUserService
{
    public Task<IEnumerable<JNUser>> GetAllUsers();
    public Task<JNUser> GetUserById(int id);
    public Task<JNUser> Create(JNUser user);
    public Task<JNUser> Update(int id, JNUser user);
    public Task<JNUser> Delete(int id);
    // Overloads to support API-layer DTO mapping without leaking business logic to controllers
    public Task<JNUser> Create(string name, string email, UserRole role, int? managerId);
    public Task<JNUser> Update(int id, string name, string email, UserRole role, int? managerId);
}