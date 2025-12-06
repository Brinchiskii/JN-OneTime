using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OneTime.Core.Services.Repository
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly OneTimeContext _context;

        /// <summary>
        /// Initializes a new instance of the TimeEntryRepository class using the specified database context.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TimeEntryRepository(OneTimeContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new time entry to the database.
        /// </summary>
        /// <param name="entry">The time entry that needs to be added to the database.</param>
        /// <returns>The added time entry.</returns>
        public async Task<TimeEntry> Add(TimeEntry entry)
        {
            _context.TimeEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        /// <summary>
        /// Retrieves time entries for a specific user, including related project and user details.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>A collection of all the time entries for the given user.</returns>
        public async Task<IEnumerable<TimeEntry>> GetByUserWithDetails(int userId)
		{
			return await _context.TimeEntries.Where(t => t.UserId == userId).Include(t => t.Project).Include(t => t.User).OrderBy(t => t.Date).ToListAsync();
		}

		public async Task<IEnumerable<TimeEntry>> GetLeaderWithDetailsForPeriod(int leaderId, DateOnly start, DateOnly end)
		{
			return await _context.TimeEntries.Include(t => t.Project).Include(t => t.User)
				.Where(t =>
					t.User != null &&
					t.User.Role == UserRole.Employee &&
					t.User.ManagerId == leaderId &&
					t.Date >= start &&
					t.Date <= end)
				.OrderBy(t => t.User.Name)
				.ThenBy(t => t.Project.Name)
				.ThenBy(t => t.Date)
				.ToListAsync();
		}
	}
}