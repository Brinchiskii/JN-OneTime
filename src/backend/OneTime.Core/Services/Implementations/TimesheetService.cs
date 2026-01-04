using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Implementations;

public class TimesheetService : ITimesheetService
{
	private readonly ITimesheetRepository _timesheetRepository;
	private readonly IAuditLogService _auditLogService;

	public TimesheetService(ITimesheetRepository timesheetRepository,IAuditLogService auditLogService)
	{
		_timesheetRepository = timesheetRepository;
		_auditLogService = auditLogService;
	}

	public async Task<Timesheet> CreateTimesheet(int userId, DateOnly periodStart, DateOnly periodEnd)
    {
        // Validate basic input
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException("User ID must be greater than zero.");
        }
        if (periodStart > periodEnd)
        {
            throw new ArgumentOutOfRangeException("Start date must be before or equal to end date.");
        }

        // Business rules moved from repository into service
        var exists = await _timesheetRepository.ExistsForPeriod(userId, periodStart, periodEnd);
        if (exists)
        {
            throw new InvalidOperationException("Timesheet already exists for the specified user and period.");
        }

        var timesheet = new Timesheet
        {
            UserId = userId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = (int)TimesheetStatus.Draft,
            DecidedByUserId = null,
            DecidedAt = null,
            Comment = null
        };

        await _timesheetRepository.Add(timesheet);

		// logging timesheet creation
		await _auditLogService.Log(
			actorUserId: userId,
			action: "TimesheetCreated",
			entityType: "Timesheet",
			entityId: timesheet.TimesheetId,
			details: $"Timesheet created for period {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}");

		return timesheet;
    }

    public async Task<Timesheet> UpdateTimeSheet(int timesheetId, int status, string? comment, int leaderId = 0)
    {
        if (timesheetId <= 0)
            throw new ArgumentOutOfRangeException("Timesheet ID must be greater than zero.");

        // Fetch sheet
        var sheet = await _timesheetRepository.GetById(timesheetId);
        if (sheet is null)
            throw new InvalidOperationException("Timesheet not found.");

		var oldStatus = (TimesheetStatus)sheet.Status;

		// Validate and map status
		TimesheetStatus newStatus = status switch
        {
            0 => TimesheetStatus.Pending,
            1 => TimesheetStatus.Approved,
            2 => TimesheetStatus.Rejected,
            3 => TimesheetStatus.Draft,
            _ => throw new ArgumentOutOfRangeException("Invalid timesheet status value.")
        };

        sheet.Status = (int)newStatus;

        sheet.DecidedByUserId = leaderId != 0 ? leaderId : sheet.UserId;

        sheet.DecidedAt = DateTime.Now;
        sheet.Comment = comment;

        await _timesheetRepository.Update(sheet);

		// logging timesheet update
		await _auditLogService.Log(
		   actorUserId: leaderId == 0 ? null : leaderId,
		   action: "TimesheetStatusChanged",
		   entityType: "Timesheet",
		   entityId: sheet.TimesheetId,
		   details: $"Status changed from {oldStatus} to {newStatus}. Comment: {comment}");

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
    public async Task<Timesheet?> GetTimesheetByUserAndDate(int userId, DateOnly startDate, DateOnly endDate)
    {
        if(userId <= 0)
        {
            throw new ArgumentOutOfRangeException("User ID must be greater than zero.");
        }
        if(startDate > endDate)
        {
            throw new ArgumentOutOfRangeException("Start date must be before or equal to end date.");
        }
        return await _timesheetRepository.GetTimesheetByUserAndDate(userId, startDate, endDate);
    }
}