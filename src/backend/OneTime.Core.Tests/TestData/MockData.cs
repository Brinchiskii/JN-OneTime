using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Tests.TestData
{
    /// <summary>
    /// Creates mock data to use for testing.
    /// </summary>
    public static class MockData
    {
        public static Project CreateProject(int id = 10, string name = "Testprojekt")
        {
            return new Project
            {
                ProjectId = id,
                Name = name
            };
        }

        public static TimeEntry CreateTimeEntry(int projectId = 10, decimal hours = 7.5m)
        {
            return new TimeEntry
            {
                ProjectId = projectId,
                Hours = hours,
                Date = default,
                Status = (int)TimeEntryStatus.Pending
            };
        }
    }
}
