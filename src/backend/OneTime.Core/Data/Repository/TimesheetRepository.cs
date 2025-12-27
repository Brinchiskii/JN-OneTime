using Microsoft.AspNetCore.Routing;
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
        /// Creates a timesheet for a user and period. Includes validations to keep current API behavior.
        /// </summary>
        // public async Task<Timesheet> CreateTimesheet(int userId, DateOnly periodStart, DateOnly periodEnd)
        // {
        //     // Keep legacy behavior for callers that use repository directly (API/tests)
        //     var exists = await ExistsForPeriod(userId, periodStart, periodEnd);
        //     if (exists)
        //         throw new InvalidOperationException("Timesheet already exists for the specified user and period.");
        //
        //     var hasEntries = await HasTimeEntriesInPeriod(userId, periodStart, periodEnd);
        //     if (!hasEntries)
        //         throw new InvalidOperationException("There are no registered entries for this period.");
        //
        //     var sheet = new Timesheet
        //     {
        //         UserId = userId,
        //         PeriodStart = periodStart,
        //         PeriodEnd = periodEnd,
        //         Status = (int)TimesheetStatus.Pending,
        //         DecidedByUserId = null,
        //         DecidedAt = null,
        //         Comment = null
        //     };
        //
        //     return await Add(sheet);
        // }

        /// <summary>
        /// Adds a new timesheet to the database.
        /// </summary>
        /// <param name="sheet">The timesheet to be added to the database</param>
        /// <returns>A <see cref="Timesheet"/> object representing the newly created timesheet.</returns>
        public async Task<Timesheet> Add(Timesheet sheet)
        {
            _context.Timesheets.Add(sheet);
            await _context.SaveChangesAsync();
            return sheet;
        }
        
        /// <summary>
        /// Checks in the database if a timesheet already exists for the given period.
        /// </summary>
        /// <param name="userId">The unique identifier for the user</param>
        /// <param name="periodStart">The start date for the timesheet</param>
        /// <param name="periodEnd">The end date for the timesheet</param>
        /// <returns>True/False whether timesheet already exists</returns>
        public Task<bool> ExistsForPeriod(int userId, DateOnly periodStart, DateOnly periodEnd)
        {
            var exists = _context.Timesheets.Any(m =>
                m.UserId == userId &&
                m.PeriodStart == periodStart &&
                m.PeriodEnd == periodEnd);

            return Task.FromResult(exists);
        }

        /// <summary>
        /// Checks if there are any time entries in the specified period.
        /// </summary>
        /// <param name="userId">The unique identifier for the user</param>
        /// <param name="periodStart">The start date for the timesheet</param>
        /// <param name="periodEnd">The end date for the timesheet</param>
        /// <returns>True/False whether </returns>
        public async Task<bool> HasTimeEntriesInPeriod(int userId, DateOnly periodStart, DateOnly periodEnd)
        {
            return await _context.TimeEntries.AnyAsync(t =>
                t.UserId == userId &&
                t.Date >= periodStart &&
                t.Date <= periodEnd);
        }
        
        public async Task<Timesheet> UpdateTimeSheet(int timesheetId, int status, string? comment, int leaderId = 0)
        {
            var sheet = await _context.Timesheets.FindAsync(timesheetId);
            if (sheet is null)
                throw new InvalidOperationException("Timesheet not found.");

            // Minimal persistence-only updates; business validation/mapping happens in service layer
            sheet.Status = status;
            if (leaderId != 0)
            {
                sheet.DecidedByUserId = leaderId;
            }
            sheet.DecidedAt = DateTime.Now;
            sheet.Comment = comment;

            await _context.SaveChangesAsync();
            return sheet;
        }

        public async Task<Timesheet?> GetById(int timesheetId)
        {
            return await _context.Timesheets.FindAsync(timesheetId);
        }

        public async Task Update(Timesheet sheet)
        {
            _context.Timesheets.Update(sheet);
            await _context.SaveChangesAsync();
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
            return await _context.TimeEntries
                .Include(t => t.Project)
                .Include(t => t.User)
                .Include(t => t.Timesheet)
                .Where(t =>
                    t.User.ManagerId == leaderId &&
                    t.User.Role == (int)UserRole.Employee &&
                    t.Timesheet.Status == (int)TimesheetStatus.Pending &&
                    t.Timesheet.PeriodStart == start &&
                    t.Timesheet.PeriodEnd == end
                )
                .OrderBy(t => t.User.Name)
                .ThenBy(t => t.Project.Name)
                .ThenBy(t => t.Date)
                .ToListAsync();

            // Returns empty list if no entries were found.
            //return entries.Count == 0 ? [] : entries;
        }

        public async Task<Timesheet?> GetTimesheetByUserAndDate(int userId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.Timesheets
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.PeriodStart == startDate &&
                    t.PeriodEnd == endDate);
        }
    }

}

