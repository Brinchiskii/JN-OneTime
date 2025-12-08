using OneTime.Core.Models;
using System;

namespace OneTime.Core.Tests.TestData
{
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

        public static TimeEntry CreateTimeEntry(
            int projectId = 10,
            decimal hours = 7.5m,
            int userId = 1,
            DateOnly? date = null,
            int timesheetId = 1,
            string note = "")
        {
            return new TimeEntry
            {
                ProjectId = projectId,
                Hours = hours,
                UserId = userId,
                Date = date ?? DateOnly.FromDateTime(DateTime.Today),
                TimesheetId = timesheetId,
                Note = note ?? string.Empty
            };
        }
    }
}