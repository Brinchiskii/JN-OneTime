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
        /// Submits a new monthly review for the specified user and period.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="periodStart">The start date of the review period.</param>
        /// <param name="periodEnd">The end date of the review period.</param>
        /// <returns>A <see cref="MonthlyReview"/> object representing the newly created monthly review.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a monthly review already exists or if there are no registered
        /// time entries for the period.</exception>
        public async Task<Timesheet> SubmitMonthlyReviewAsync(int userId, DateOnly periodStart, DateOnly periodEnd)
        {
            // Check if already existing review for this period.
            var existingReview = _context.Timesheets
                .FirstOrDefault(m => 
                    m.UserId == userId 
                    && m.PeriodStart == periodStart 
                    && m.PeriodEnd == periodEnd);

            if (existingReview is not null)
            {
                throw new InvalidOperationException("Monthly review already exists for the specified user and period.");
            }

            // Check if there are any time entries for this specific period.
            var hasEntries = await _context.TimeEntries
                .AnyAsync(t =>
                     t.UserId == userId 
                    && t.Date >= periodStart 
                    && t.Date <= periodEnd);

            if (!hasEntries)
            {
                throw new InvalidOperationException("There are now registrered entries for this period.");
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

		public async Task<Timesheet> UpdateTimeSheet(int timesheetId, int leaderId, int status, string? comment)
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

			sheet.DecidedByUserId = leaderId;
			sheet.DecidedAt = DateTime.Now;
			sheet.Comment = comment;

			await _context.SaveChangesAsync();

			return sheet;
		}
	}

}

