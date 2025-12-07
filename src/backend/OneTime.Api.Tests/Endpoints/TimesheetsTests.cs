using OneTime.Api.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using OneTime.Core.Models.Enums;
using OneTime.Api.Models.TimesheetsDto;

namespace OneTime.Api.Tests.Endpoints
{
    public class TimesheetsTests : IClassFixture<OneTimeApiFactory>
    {
        private readonly HttpClient _client;

        public TimesheetsTests(OneTimeApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SubmitTimesheet_Already_Exists_Throws()
        {
            // Arrange
            var dto = new SubmitTimesheetDto(
                UserId: 1,
                PeriodStart: new DateOnly(2025, 11, 1),
                PeriodEnd: new DateOnly(2025, 11, 30)
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/Timesheets/submit", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("Timesheet already exists for the specified user and period.", message);
        }

        [Fact]
        public async Task SubmitTimesheet_No_TimeEntries_Throws()
        {
            // Arrange
            var dto = new SubmitTimesheetDto(
                UserId: 2,
                PeriodStart: new DateOnly(2025, 12, 1),
                PeriodEnd: new DateOnly(2025, 12, 31)
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/Timesheets/submit", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            //var message = await response.Content.ReadAsStringAsync();
            //Assert.Contains("There are no registered entries for this period.", message);
        }

        [Fact]
        public async Task SubmitTimesheet_Succeeds()
        {
            // Arrange
            var dto = new SubmitTimesheetDto(
                UserId: 1,
                PeriodStart: new DateOnly(2025, 10, 1),
                PeriodEnd: new DateOnly(2025, 10, 30)
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/Timesheets/submit", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<TimesheetDto>();
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal(new DateOnly(2025, 10, 1), result.PeriodStart);
            Assert.Equal(new DateOnly(2025, 10, 30), result.PeriodEnd);
            Assert.Equal(TimesheetStatus.Pending, result.Status);
            Assert.Null(result.DecidedAt);
            Assert.Null(result.Comment);
        }

        [Fact]
        public async Task GetTimeentriesForPendingTimesheet_Returns_Ok_With_Data()
        {
            // Arrange
            var leaderId = 1;
            var start = new DateOnly(2025, 12, 1);
            var end = new DateOnly(2025, 12, 31);
            var startParam = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endParam = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Act
            var response = await _client.GetAsync($"api/Timesheets/leader/{leaderId}/team/?startDate={startParam}&endDate={endParam}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<LeaderUsersProjectsResponseDto>();

            Assert.NotNull(result);
            Assert.NotEmpty(result.Users);
            Assert.True(result.Users.ContainsKey("Team Member"));
            var teamMemberProjects = result.Users["Team Member"];
            var projectGrouping = Assert.Single(teamMemberProjects);
            Assert.Equal(1, projectGrouping.Project.ProjectId);
            Assert.Equal("Test Project 1", projectGrouping.Project.Name);
            Assert.Equal(2, projectGrouping.Hours.Count);
            Assert.Equal(7m, projectGrouping.Hours["2025-12-05"]);
            Assert.Equal(8m, projectGrouping.Hours["2025-12-06"]);
        }

        [Fact]
        public async Task GetTimeentriesForPendingTimesheet_NoEntries_Returns_NoContent()
        {
            // Arrange
            var leaderId = 1;
            
            // Month with no entries
            var start = new DateOnly(2026, 1, 1); 
            var end = new DateOnly(2026, 1, 31);
            var startParam = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endParam = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Act
            var response = await _client.GetAsync($"api/Timesheets/leader/{leaderId}/team/?startDate={startParam}&endDate={endParam}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
