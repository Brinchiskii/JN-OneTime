using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections;
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
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
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
        /// Deletes all time entry records associated with the specified timesheet identifier.
        /// </summary>
        /// <remarks>If no time entries are found for the specified timesheet identifier, no action is
        /// taken. This method does not save changes to the database; callers must explicitly save changes to persist
        /// the deletions.</remarks>
        /// <param name="timesheetId">The unique identifier of the timesheet whose time entries are to be deleted.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteEntriesByTimesheetId(int timesheetId)
        {
            var entries = await _context.TimeEntries
                .Where(t => t.TimesheetId == timesheetId)
                .ToListAsync();

            if (entries.Any())
            {
                _context.TimeEntries.RemoveRange(entries);
            }
        }

        /// <summary>
        /// Asynchronously adds a collection of time entries to the data store.
        /// </summary>
        /// <param name="entries">The collection of <see cref="TimeEntry"/> objects to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public async Task AddTimeEntries(IEnumerable<TimeEntry> entries)
        {
            await _context.TimeEntries.AddRangeAsync(entries);
        }

        /// <summary>
        /// Retrieves time entries for a specific user, including related project and user details.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>A collection of all the time entries for the given user.</returns>
        public async Task<IEnumerable<TimeEntry>> GetByUserWithDetails(int userId)
		{
			return await _context.TimeEntries
                .Where(t => t.UserId == userId)
                .Include(t => t.Project)
                .Include(t => t.User)
                .OrderBy(t => t.Date)
                .ToListAsync();
		}

        public async Task<IEnumerable<TimeEntry>> GetWeeklyTimeEntriesByUser(int userId, int timesheetId)
        {
            return await _context.TimeEntries
                .Include(t => t.Project)
                .Include(t => t.User)
                .Where(t =>
                    t.UserId== userId &&
                    t.TimesheetId == timesheetId
                )
                .ToListAsync();
        }
	}
}