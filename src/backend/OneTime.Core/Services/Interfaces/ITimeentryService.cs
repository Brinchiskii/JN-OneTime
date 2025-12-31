using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces;

public interface ITimeEntryService
{
    Task<TimeEntry> CreateTimeEntry(TimeEntry timeEntry);
    Task ReplaceTimeEntries(int timesheetId, List<TimeEntry> newEntries);

    Task<IEnumerable<TimeEntry>> GetTimeEntriesByUserWithDetails(int userId);
    Task<IEnumerable<TimeEntry>> GetTimeEntriesForTimesheet(int userId, int timesheetId);
}