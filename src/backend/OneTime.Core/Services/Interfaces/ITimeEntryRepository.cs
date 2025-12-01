using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    /// <summary>
    /// Handles data operations for time entries.
    /// </summary>
    public interface ITimeEntryRepository
    {
        /// <summary>
        /// Adds new time entry to the database.
        /// </summary>
        /// <param name="entry">The time entry that needs to be added to the database.</param>
        /// <returns>The added time entry.</returns>
        Task<TimeEntry> Add(TimeEntry entry);
		Task<IEnumerable<TimeEntry>> GetByUserWithDetails(int userId);
		Task<IEnumerable<TimeEntry>> GetLeaderWithDetailsForPeriod(int leaderId, DateOnly start, DateOnly end);
	}
}