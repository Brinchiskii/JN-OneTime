using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Tests.Fakes
{
    /// <summary>
    /// Provides a in-memory implementation of the ITimeEntryRepository interface for testing.
    /// </summary>
    public class FakeTimeEntryRepository : ITimeEntryRepository
    {
        public TimeEntry AddedEntry { get; private set; }
        public bool AddCalled { get; private set; }

        public Task<TimeEntry> Add (TimeEntry entry)
        {
            AddCalled = true;
            AddedEntry = entry;
            return Task.FromResult(entry);
        }
    }
}
