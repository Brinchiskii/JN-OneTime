using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Repository
{
    public class TimesheetRepository : ITimesheetRepository
    {
        private readonly OneTimeContext _context;

        public TimesheetRepository(OneTimeContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates new timesheet for the specified user and period.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="periodStart">The start date of the review period.</param>
        /// <param name="periodEnd">The end date of the review period.</param>
        /// <returns>A <see cref="Timesheet"/> object representing the newly created timesheet.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a monthly review already exists or if there are no registered
        /// time entries for the period.</exception>
        public async Task<Timesheet> CreateTimesheet(int userId, DateOnly periodStart, DateOnly periodEnd)
        {
            // Check if already existing review for this period.
            var existingReview = _context.Timesheets
                .FirstOrDefault(m => 
                    m.UserId == userId 
                    && m.PeriodStart == periodStart 
                    && m.PeriodEnd == periodEnd);

            if (existingReview is not null)
            {
                throw new InvalidOperationException("Timesheet already exists for the specified user and period.");
            }

            // Check if there are any time entries for this specific period.
            var hasEntries = await _context.TimeEntries
                .AnyAsync(t =>
                     t.UserId == userId 
                    && t.Date >= periodStart 
                    && t.Date <= periodEnd);

            if (!hasEntries)
            {
                throw new InvalidOperationException("There are no registered entries for this period.");
            }

            // Creates a new monthly review.
            var review = new Timesheet
            {
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Status = TimesheetStatus.Pending,
                DecidedByUserId = null,
                DecidedAt = null,
                Comment = null
            };

            // Saves the new review to the database.
            _context.Timesheets.Add(review);
            await _context.SaveChangesAsync();

            return review;
        }

		public async Task<Timesheet> UpdateTimeSheet(int timesheetId, int status, string? comment, int leaderId = 0)
		{
			var sheet = await _context.Timesheets.FindAsync(timesheetId);

			if (sheet is null)
				throw new InvalidOperationException("Timesheet not found.");

            sheet.Status = status switch
            {
                0 => TimesheetStatus.Pending,
                1 => TimesheetStatus.Approved,
                2 => TimesheetStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(status), "Invalid timesheet status value.")
            };

            if (leaderId != 0)
            {
                sheet.DecidedByUserId = leaderId;
            }
			sheet.DecidedAt = DateTime.Now;
			sheet.Comment = comment;

			await _context.SaveChangesAsync();

			return sheet;
		}
        
        /// <summary>
        /// Retrieves time entries for a specific leader, including related project and user details.
        /// </summary>
        /// <param name="leaderId">The unique identifier for the leader.</param>
        /// <param name="start">The start date for the period.</param>
        /// <param name="end">The end date for the period.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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
			
            var entries =  await _context.TimeEntries
                .Include(t => t.Project)
                .Include(t => t.User)
                .Where(t =>
                    t.User != null &&
                    t.User.ManagerId == leaderId &&
                    t.User.Role == UserRole.Employee &&
                    t.Date >= start &&
                    t.Date <= end && 
                           _context.Timesheets.Any(ts => 
                                   ts.UserId == t.UserId &&
                                   ts.Status == TimesheetStatus.Pending &&
                                   ts.PeriodStart <= start &&
                                   ts.PeriodEnd >= end
                                   ))
                .OrderBy(t => t.User.Name)
                .ThenBy(t => t.Project.Name)
                .ThenBy(t => t.Date)
                .ToListAsync();

            // Returns empty list if no entries were found.
            return entries.Count == 0 ? [] : entries;
        }
	}

}

