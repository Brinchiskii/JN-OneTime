using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneTime.Core.Tests.Fakes
{
    /// <summary>
    /// Simple in-memory fake for ITimeEntryRepository used in unit-tests.
    /// </summary>
    public class FakeTimeEntryRepository : ITimeEntryRepository
    {
        private int _nextId = 1;

        // State useful for assertions in tests
        public TimeEntry AddedEntry { get; private set; }
        public bool AddCalled { get; private set; }

        // Data the fake will return
        public List<TimeEntry> EntriesToReturn { get; } = new();

        // Last parameters requested
        public int? LastUserIdRequested { get; private set; }
        public (int leaderId, DateOnly start, DateOnly end)? LastLeaderPeriodRequested { get; private set; }

        public Task<TimeEntry> Add(TimeEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            AddCalled = true;

            // Simulér autoincrement behaviour fra DB hvis id er 0
            if (entry.TimeEntryId == 0)
            {
                entry.TimeEntryId = _nextId++;
            }

            // Gem en kopi for at undgå utilsigtet mutation fra test-kode
            var stored = new TimeEntry
            {
                TimeEntryId = entry.TimeEntryId,
                UserId = entry.UserId,
                ProjectId = entry.ProjectId,
                Date = entry.Date,
                Note = entry.Note,
                Hours = entry.Hours,
                TimesheetId = entry.TimesheetId,
                Project = entry.Project,
                Timesheet = entry.Timesheet,
                User = entry.User
            };

            AddedEntry = stored;
            EntriesToReturn.Add(stored);

            // Returnér den samme instans som "gemt"
            return Task.FromResult(stored);
        }

        public Task<IEnumerable<TimeEntry>> GetByUserWithDetails(int userId)
        {
            LastUserIdRequested = userId;
            var result = EntriesToReturn.Where(t => t.UserId == userId).OrderBy(t => t.Date).ToList();
            return Task.FromResult<IEnumerable<TimeEntry>>(result);
        }

        // Helpers til tests
        public void Clear()
        {
            EntriesToReturn.Clear();
            AddedEntry = null;
            AddCalled = false;
            LastUserIdRequested = null;
            LastLeaderPeriodRequested = null;
            _nextId = 1;
        }

        public void SeedEntries(params TimeEntry[] entries)
        {
            foreach (var e in entries)
            {
                if (e.TimeEntryId == 0) e.TimeEntryId = _nextId++;
                EntriesToReturn.Add(e);
            }
        }
    }
}
