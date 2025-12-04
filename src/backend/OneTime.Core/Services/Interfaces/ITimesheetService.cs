using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Interfaces
{
    public interface ITimesheetService
    {
        /// <summary>
        /// submits a monthly review for the specified user and period.
        /// </summary>
        /// <param name="userId">Unique identifier for the user.</param>
        /// <param name="periodStart">Start of the review period.</param>
        /// <param name="periodEnd">End of the review period.</param>
        /// <returns>The monthly review object.</returns>
        Task<Timesheet> CreateTimesheet(int userId, DateOnly periodStart, DateOnly periodEnd);
        Task<Timesheet> UpdateTimeSheet(int timesheetId, int status, string? comment, int leaderId);

	}
}
