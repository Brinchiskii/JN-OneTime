using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry> Add(TimeEntry entry);
		Task<IEnumerable<TimeEntry>> GetByUserWithDetails(int userId);

	}
}