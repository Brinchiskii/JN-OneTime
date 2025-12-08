using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces;

public interface ITimesheetService
{
    Task<Timesheet> CreateTimesheet(int userId, DateOnly periodStart, DateOnly periodEnd);
    Task<Timesheet> UpdateTimeSheet(int timesheetId, int status, string? comment, int leaderId = 0);
    Task<IEnumerable<TimeEntry>> GetTimeentriesForPendingTimesheet(int leaderId, DateOnly start, DateOnly end);
}