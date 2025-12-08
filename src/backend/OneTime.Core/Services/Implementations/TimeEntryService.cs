using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Services.Implementations;

public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IProjectRepository _projectRepository;

    public TimeEntryService(ITimeEntryRepository timeEntryRepository, IProjectRepository projectRepository)
    {
        _timeEntryRepository = timeEntryRepository;
        _projectRepository = projectRepository;
    }
    
    public async Task<TimeEntry> CreateTimeEntry(TimeEntry timeEntry)
    {
        // Validate time entry
        if (timeEntry == null)
        {
            throw new ArgumentNullException("Time entry cannot be null");
        } 
        
        // Validate if project exists
        var project = await _projectRepository.GetById(timeEntry.ProjectId);
        if (project == null)
        {
            throw new ArgumentNullException("Project not found");
        }
        
        // Validate if hours are within 0 to 24
        if (timeEntry.Hours <= 0 || timeEntry.Hours > 24)
        {
            throw new ArgumentOutOfRangeException("Hours must be greater than zero and less than 24");
        }
        
        return await _timeEntryRepository.Add(timeEntry);
        
    }

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByUserWithDetails(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId must be greater than zero");
        }

        var entries = _timeEntryRepository.GetByUserWithDetails(userId);

        if (!entries.Result.Any())
        {
            return Enumerable.Empty<TimeEntry>();
        }
        
        return entries.Result;
    }
}