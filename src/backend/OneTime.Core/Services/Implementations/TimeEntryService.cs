using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Implementations;

public class TimeEntryService : ITimeEntryService
{
	private readonly ITimeEntryRepository _timeEntryRepository;
	private readonly IProjectRepository _projectRepository;
	private readonly IAuditLogService _auditLogService;

	public TimeEntryService(ITimeEntryRepository timeEntryRepository,IProjectRepository projectRepository,IAuditLogService auditLogService)
	{
		_timeEntryRepository = timeEntryRepository;
		_projectRepository = projectRepository;
		_auditLogService = auditLogService;
	}

	public async Task<TimeEntry> CreateTimeEntry(TimeEntry timeEntry)
	{
		if (timeEntry == null)
		{
			throw new ArgumentNullException(nameof(timeEntry), "Time entry cannot be null");
		}

		var project = await _projectRepository.GetById(timeEntry.ProjectId);
		if (project == null)
		{
			throw new ArgumentNullException("Project not found");
		}

		if (timeEntry.Hours <= 0 || timeEntry.Hours > 24)
		{
			throw new ArgumentOutOfRangeException("Hours must be greater than zero and less than 24");
		}
		  
		var created = await _timeEntryRepository.Add(timeEntry);

		await _auditLogService.Log(
			actorUserId: created.UserId,
			action: "TimeEntryCreated",
			entityType: "TimeEntry",
			entityId: created.TimeEntryId,
			details: $"ProjectId={created.ProjectId}, Date={created.Date:yyyy-MM-dd}, Hours={created.Hours}");

		return created;
	}

    public async Task ReplaceTimeEntries(int timesheetId, List<TimeEntry> newEntries)
    {
	    if (timesheetId <= 0)
	    {
		    throw new ArgumentException("TimesheetId must be greater than zero");
	    }
	    
        await _timeEntryRepository.DeleteEntriesByTimesheetId(timesheetId);
        await _timeEntryRepository.AddTimeEntries(newEntries);
        await _timeEntryRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByUserWithDetails(int userId)
	{
		if (userId <= 0)
		{
			throw new ArgumentException("UserId must be greater than zero");
		}

		var entries = await _timeEntryRepository.GetByUserWithDetails(userId);

		if (!entries.Any())
		{
			return Enumerable.Empty<TimeEntry>();
		}

		return entries;
	}

	public async Task<IEnumerable<TimeEntry>> GetTimeEntriesForTimesheet(int userId, int timesheetId)
	{
		if (userId <= 0) 
			throw new ArgumentException("UserId must be greater than zero.");
		if (timesheetId <= 0) 
			throw new ArgumentException("TimeSheetId must be greater than zero.");

		var entries = await _timeEntryRepository.GetWeeklyTimeEntriesByUser(userId, timesheetId);

		return entries;

	}
}