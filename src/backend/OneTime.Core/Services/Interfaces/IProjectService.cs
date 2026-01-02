using OneTime.Core.Models;
using OneTime.Core.Models.Enums;

namespace OneTime.Core.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAll();
    Task<Project?> GetById(int id);
    Task<Project> Create(string name, ProjectStatus status);
    Task<Project> Update(int id, string name, ProjectStatus status);
    Task<Project> Delete(int id);
}
