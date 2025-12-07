using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    /// <summary>
    /// Handles creation and retrieval of time entries and related projects.
    /// </summary>
    public interface ITimeEntryService
    {
        /// <summary>
        /// Creates a new time entry after validating the project.
        /// </summary>
        /// <param name="entry">The time entry, which needs to be created.</param>
        /// <returns>The saved time entry.</returns>
        Task<TimeEntry> CreateTimeEntry(TimeEntry entry);

        /// <summary>
        /// Gets all the projects, which are available for time entry.
        /// </summary>
        /// <returns>A collection of all available projects.</returns>
        Task<IEnumerable<Project>> GetAvailableProjects();
		Task<IEnumerable<TimeEntry>> GetTimeEntriesForUser(int userId);
        
		//Task<IEnumerable<TimeEntry>> GetLeaderPeriodEntries(int leaderId, DateOnly start, DateOnly end);

	}
}