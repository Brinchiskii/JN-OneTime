using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces;

public interface ITimeEntryService
{
    Task<TimeEntry> CreateTimeEntry(TimeEntry timeEntry);
    Task<IEnumerable<TimeEntry>> GetTimeEntriesByUserWithDetails(int userId);
}