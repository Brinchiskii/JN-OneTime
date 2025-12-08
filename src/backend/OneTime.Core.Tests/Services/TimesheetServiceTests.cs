using System;
using System.Collections.Generic;
using System.Text;
using OneTime.Core.Services.Repository;
using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Tests.TestHelpers;
namespace OneTime.Core.Tests.Services
{
    public class TimesheetServiceTests
    {
        [Fact]
        public async Task SubmitTimesheet_Already_Exists_Throws()
        {
            // Arrange
            var userId = 1;
            var periodStart = new DateOnly(2025, 11, 1);
            var periodEnd = new DateOnly(2025, 11, 30);

            var context = OneTimeContextFactory.CreateInMemoryContext();

            context.Timesheets.Add(new Timesheet
            {
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Status = (int)TimesheetStatus.Pending
            });

            await context.SaveChangesAsync();

            var _timesheetRepo = new TimesheetRepository(context);
            var _service = new TimesheetService(_timesheetRepo);

            // Act + Assert 
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateTimesheet(userId, periodStart, periodEnd));
        }

        [Fact]
        public async Task SubmitTimesheet_No_TimeEntries_Throws()
        {
            // Arrange
            var userId = 1;
            var periodStart = new DateOnly(2025, 12, 1);
            var periodEnd = new DateOnly(2025, 12, 31);

            var context = OneTimeContextFactory.CreateInMemoryContext();

            var _timesheetRepo = new TimesheetRepository(context);
            var _service = new TimesheetService(_timesheetRepo);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateTimesheet(userId, periodStart, periodEnd));

        }

        [Fact]
        public async Task SubmitTimesheet_Succeeds()
        {
            // Arrange
            var userId = 1;
            var periodStart = new DateOnly(2025, 10, 1);
            var periodEnd = new DateOnly(2025, 10, 31);

            var context = OneTimeContextFactory.CreateInMemoryContext();

            context.TimeEntries.Add(new TimeEntry
            {
                UserId = userId,
                Date = new DateOnly(2025, 10, 15),
                Hours = 8m,
                ProjectId = 1,
                Note = "Test Entry"
            });

            await context.SaveChangesAsync();

            var _timesheetRepo = new TimesheetRepository(context);
            var _service = new TimesheetService(_timesheetRepo);

            // Act
            var timesheet = await _service.CreateTimesheet(userId, periodStart, periodEnd);

            // Assert
            Assert.NotNull(timesheet);
            Assert.Equal(userId, timesheet.UserId);
            Assert.Equal(periodStart, timesheet.PeriodStart);
            Assert.Equal(periodEnd, timesheet.PeriodEnd);
            Assert.Equal((int)TimesheetStatus.Pending, timesheet.Status);
        }

        [Fact]
        public async Task GetTimeentriesForPendingTimesheet_LeaderId_Is_Zero_Or_Negative()
        {
            var periodStart = new DateOnly(2025, 12, 1);
            var periodEnd = new DateOnly(2025, 12, 7);
            
            var context = OneTimeContextFactory.CreateInMemoryContext();
            
            var timesheetRepo = new TimesheetRepository(context);
            var timesheetService = new TimesheetService(timesheetRepo);
            
            var ex1 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimeentriesForPendingTimesheet(0, periodStart, periodEnd));
            Assert.Equal("Leader ID must be greater than zero.", ex1.ParamName);
            var ex2 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimeentriesForPendingTimesheet(-1, periodStart, periodEnd));
            Assert.Equal("Leader ID must be greater than zero.", ex2.ParamName);
        }

        [Fact]
        public async Task GetTimeentriesForPendingTimesheet_StartPeriod_Is_After_EndPeriod()
        {
            var context = OneTimeContextFactory.CreateInMemoryContext();
            
            var timesheetRepository = new TimesheetRepository(context);
            var timesheetService = new TimesheetService(timesheetRepository);
            
            var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimeentriesForPendingTimesheet(1, new DateOnly(2025, 12, 10), new DateOnly(2025, 12, 7)));
            Assert.Equal("Start date must be before or equal to end date.", ex.ParamName);
        }
        
        [Fact]
        public async Task GetTimeentriesForPendingTimesheet_Returns_TimeEntries()
        {
            // Arrange
            var context = OneTimeContextFactory.CreateInMemoryContext();

            // 1) Mock user
            context.JNUsers.Add(new JNUser
            {
                UserId = 1,
                Name = "Test Employee",
                Email = "test@example.com",
                PasswordHash = "",
                PasswordSalt = "",
                Role = 2,
                ManagerId = 1
            });

            // 2) Mock project
            context.Projects.Add(new Project
            {
                ProjectId = 1,
                Name = "Test Project",
                Status = 0
            });

            // 3) Mock timesheet
            context.Timesheets.Add(new Timesheet
            {
                UserId = 1,
                PeriodStart = new DateOnly(2025, 12, 1),
                PeriodEnd = new DateOnly(2025, 12, 31),
                Status = (int)Models.Enums.TimesheetStatus.Pending // Pending
            });

            // 4) Mock time entry
            context.TimeEntries.Add(new TimeEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = new DateOnly(2025, 12, 3),
                Note = null,
                Hours = 7,
                TimesheetId = 1
            });
            context.TimeEntries.Add(new TimeEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = new DateOnly(2025, 12, 4),
                Note = null,
                Hours = 8,
                TimesheetId = 1
            });

            await context.SaveChangesAsync();

            
            // Act + Assert
            var timesheetRepository = new TimesheetRepository(context);
            var timesheetService = new TimesheetService(timesheetRepository);

            var entries = await timesheetService.GetTimeentriesForPendingTimesheet(
                leaderId: 1,
                start: new DateOnly(2025, 12, 1),
                end: new DateOnly(2025, 12, 31));

            Assert.Collection(entries,
                item =>
                {
                    Assert.Equal(7, item.Hours);
                    Assert.Equal(new DateOnly(2025, 12, 3), item.Date);
                },
                item =>
                {
                    Assert.Equal(8, item.Hours);
                    Assert.Equal(new DateOnly(2025, 12, 4), item.Date);
                });
        }

        [Fact]
        public async Task GetTimeentriesForPendingTimesheet_Returns_EmptyList_When_No_Pending_Timesheets()
        {
            // Arrange
            var context = OneTimeContextFactory.CreateInMemoryContext();

            // 1) Mock user
            context.JNUsers.Add(new JNUser
            {
                UserId = 1,
                Name = "Test Employee",
                Email = "test@example.com",
                PasswordHash = "",
                PasswordSalt = "",
                Role = 2,
                ManagerId = 1
            });

            // 2) Mock project
            context.Projects.Add(new Project
            {
                ProjectId = 1,
                Name = "Test Project",
                Status = 0
            });

            // 3) Mock timesheet
            context.Timesheets.Add(new Timesheet
            {
                UserId = 1,
                PeriodStart = new DateOnly(2025, 12, 1),
                PeriodEnd = new DateOnly(2025, 12, 31),
                Status = (int)Models.Enums.TimesheetStatus.Pending // Pending
            });
            
            await context.SaveChangesAsync();
            
            // Act + Assert
            var service = new TimesheetRepository(context);
            
            var entries = await service.GetTimeentriesForPendingTimesheet(
                leaderId: 1,
                start: new DateOnly(2025, 12, 1),
                end: new DateOnly(2025, 12, 31));
            
            Assert.Empty(entries);
        }
    }
}
