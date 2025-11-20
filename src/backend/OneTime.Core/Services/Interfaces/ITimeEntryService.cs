using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    public interface ITimeEntryService
    {
        Task<TimeEntry> CreateTimeEntry(TimeEntry entry);
        Task<IEnumerable<Project>> GetAvailableProjects();
    }
}