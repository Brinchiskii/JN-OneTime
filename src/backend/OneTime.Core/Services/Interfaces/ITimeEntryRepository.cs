using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry> Add(TimeEntry entry);
    }
}