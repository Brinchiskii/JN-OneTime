using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAll();
        Task<Project> GetById(int id);
    }
}