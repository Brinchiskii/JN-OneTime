using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Repository
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly OneTimeContext _context;

        public TimeEntryRepository(OneTimeContext context)
        {
            _context = context;
        }

        public async Task<TimeEntry> Add(TimeEntry entry)
        {
            _context.TimeEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }
    }
}
