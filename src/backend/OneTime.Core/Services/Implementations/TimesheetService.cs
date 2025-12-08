using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Implementations;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepository;

    public TimesheetService(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task<Timesheet> CreateTimesheet(int userId, DateOnly periodStart, DateOnly periodEnd)
    {
        // Validate basic input
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than zero.");
        }
        if (periodStart > periodEnd)
        {
            throw new ArgumentOutOfRangeException(nameof(periodStart), "Start date must be before or equal to end date.");
        }

        // Business rules moved from repository into service
        var exists = await _timesheetRepository.ExistsForPeriod(userId, periodStart, periodEnd);
        if (exists)
        {
            throw new InvalidOperationException("Timesheet already exists for the specified user and period.");
        }

        var hasEntries = await _timesheetRepository.HasTimeEntriesInPeriod(userId, periodStart, periodEnd);
        if (!hasEntries)
        {
            throw new InvalidOperationException("There are no registered entries for this period.");
        }

        var timesheet = new Timesheet
        {
            UserId = userId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = (int)TimesheetStatus.Pending,
            DecidedByUserId = null,
            DecidedAt = null,
            Comment = null
        };

        await _timesheetRepository.Add(timesheet);
        
        return timesheet;
    }

    public async Task<Timesheet> UpdateTimeSheet(int timesheetId, int status, string? comment, int leaderId = 0)
    {
        if (timesheetId <= 0)
            throw new ArgumentOutOfRangeException(nameof(timesheetId), "Timesheet ID must be greater than zero.");

        // Fetch sheet
        var sheet = await _timesheetRepository.GetById(timesheetId);
        if (sheet is null)
            throw new InvalidOperationException("Timesheet not found.");

        // Validate and map status
        TimesheetStatus newStatus = status switch
        {
            0 => TimesheetStatus.Pending,
            1 => TimesheetStatus.Approved,
            2 => TimesheetStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), "Invalid timesheet status value.")
        };

        sheet.Status = (int)newStatus;

        if (leaderId != 0)
        {
            sheet.DecidedByUserId = leaderId;
        }

        sheet.DecidedAt = DateTime.Now;
        sheet.Comment = comment;

        await _timesheetRepository.Update(sheet);

        return sheet;
    }
    
    public async Task<IEnumerable<TimeEntry>> GetTimeentriesForPendingTimesheet(int leaderId, DateOnly start, DateOnly end)
    {
        if (leaderId <= 0)
        {
            throw new ArgumentOutOfRangeException("Leader ID must be greater than zero.");
        }

        if (start > end)
        {
            throw new ArgumentOutOfRangeException("Start date must be before or equal to end date.");
        }

        var entries = await _timesheetRepository.GetTimeentriesForPendingTimesheet(leaderId, start, end);
        return entries ?? Array.Empty<TimeEntry>();
    }
}