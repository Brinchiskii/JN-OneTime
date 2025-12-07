using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Api.Tests.TestHelpers
{
    /// <summary>
    /// Provides a custom web factory for integration testing the OneTime API.
    /// </summary>
    public class OneTimeApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationsTesting");

            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<OneTimeContext>();

                // Resets the database to ensure a clean state for each test run
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Adds data to the in-memory database for testing
                SeedTestData(context);
            });
        }

        // private void SeedTestData(OneTimeContext context)
        // {
        //     context.Projects.AddRange(
        //         new Project { ProjectId = 1, Name = "Test Project 1" },
        //         new Project { ProjectId = 2, Name = "Test Project 2" }
        //     );
        //
        //     context.TimeEntries.AddRange(
        //         new TimeEntry { TimeEntryId = 1, UserId = 1, ProjectId = 1, Date = new DateOnly(2025, 10, 15), Hours = 8m, Note = "Worked on feature A", Status = (int)TimeEntryStatus.Pending },
        //         new TimeEntry { TimeEntryId = 2, UserId = 1, ProjectId = 2, Date = new DateOnly(2025, 10, 20), Hours = 6m, Note = "Fixed bug B", Status = (int)TimeEntryStatus.Approved }
        //     );
        //
        //     context.Timesheets.AddRange(
        //         new Timesheet { UserId = 1, PeriodStart = new DateOnly(2025, 11, 1), PeriodEnd = new DateOnly(2025, 11, 30), Status = TimesheetStatus.Pending }
        //     );
        //
        //     context.SaveChanges();
        // }
        
        private void SeedTestData(OneTimeContext context)
        {
            var leader = new JNUser
            {
                UserId = 1,
                Name = "Team Lead",
                Email = "teamlead@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Role = 1
            };

            var employee = new JNUser
            {
                UserId = 2,
                Name = "Team Member",
                Email = "teammember@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Role = 2,
                ManagerId = leader.UserId
            };

            context.JNUsers.AddRange(leader, employee);

            context.Projects.AddRange(
                new Project { ProjectId = 1, Name = "Test Project 1" },
                new Project { ProjectId = 2, Name = "Test Project 2" }
            );

            context.TimeEntries.AddRange(
                new TimeEntry { TimeEntryId = 1, UserId = 1, ProjectId = 1, Date = new DateOnly(2025, 10, 15), Hours = 8m, Note = "Worked on feature A", Status = (int)TimeEntryStatus.Pending },
                new TimeEntry { TimeEntryId = 2, UserId = 1, ProjectId = 2, Date = new DateOnly(2025, 10, 20), Hours = 6m, Note = "Fixed bug B", Status = (int)TimeEntryStatus.Approved },
                new TimeEntry { TimeEntryId = 3, UserId = 2, ProjectId = 1, Date = new DateOnly(2025, 12, 5), Hours = 7m, Note = "Team member task 1", Status = (int)TimeEntryStatus.Pending },
                new TimeEntry { TimeEntryId = 4, UserId = 2, ProjectId = 1, Date = new DateOnly(2025, 12, 6), Hours = 8m, Note = "Team member task 2", Status = (int)TimeEntryStatus.Pending }
            );

            context.Timesheets.AddRange(
                new Timesheet { UserId = 1, PeriodStart = new DateOnly(2025, 11, 1), PeriodEnd = new DateOnly(2025, 11, 30), Status = TimesheetStatus.Pending },
                new Timesheet { UserId = 2, PeriodStart = new DateOnly(2025, 12, 1), PeriodEnd = new DateOnly(2025, 12, 31), Status = TimesheetStatus.Pending }
            );

            context.SaveChanges();
        }
    }
}
