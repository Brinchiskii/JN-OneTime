using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using OneTime.Api.Models.TimesheetsDto;
using OneTime.Api.Tests.TestHelpers;
using OneTime.Core.Models.Enums;
using Xunit;

namespace OneTime.Api.Tests.Endpoints;

public class TimesheetsControllerTests
{
    private static HttpClient CreateClient()
    {
        var factory = new OneTimeApiFactory();
        return factory.CreateClient();
    }

    private static async Task<int> CreateSampleTimesheetAsync(HttpClient client, int userId, DateOnly start, DateOnly end)
    {
        var submit = new SubmitTimesheetDto(userId, start, end);
        var submitResp = await client.PostAsJsonAsync("api/Timesheets/submit", submit);

        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);

        var created = await submitResp.Content.ReadFromJsonAsync<TimesheetDto>();
        Assert.NotNull(created);

        return created!.TimesheetId;
    }

    [Fact]
    public async Task Update_Approve_Succeeds()
    {
        using var client = CreateClient();

        var timesheetId = await CreateSampleTimesheetAsync(
            client,
            userId: 1,
            start: new DateOnly(2025, 10, 1),
            end: new DateOnly(2025, 10, 31)
        );

        var decision = new TimesheetDecisionDto(
            TimesheetId: timesheetId,
            LeaderId: 1,
            Status: (int)TimesheetStatus.Approved,
            Comment: "Looks good"
        );

        var response = await client.PostAsJsonAsync($"api/Timesheets/update/{timesheetId}", decision);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TimesheetDto>();
        Assert.NotNull(updated);
        Assert.Equal(TimesheetStatus.Approved, updated!.Status);
        Assert.NotNull(updated.DecidedAt);
        Assert.Equal("Looks good", updated.Comment);
    }

    [Fact]
    public async Task Update_Reject_Succeeds()
    {
        using var client = CreateClient();
        
        var decision = new TimesheetDecisionDto(
            TimesheetId: 2,
            LeaderId: 1,
            Status: (int)TimesheetStatus.Rejected,
            Comment: "Please correct entries"
        );

        var response = await client.PostAsJsonAsync($"api/Timesheets/update/{1}", decision);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TimesheetDto>();
        Assert.NotNull(updated);
        Assert.Equal(TimesheetStatus.Rejected, updated!.Status);
        Assert.NotNull(updated.DecidedAt);
        Assert.Equal("Please correct entries", updated.Comment);
    }

    [Fact]
    public async Task Update_InvalidStatus_Returns_BadRequest()
    {
        using var client = CreateClient();

        var timesheetId = await CreateSampleTimesheetAsync(
            client,
            userId: 1,
            start: new DateOnly(2025, 10, 1),
            end: new DateOnly(2025, 10, 31)
        );

        var decision = new TimesheetDecisionDto(
            TimesheetId: timesheetId,
            LeaderId: 1,
            Status: 99, // invalid
            Comment: "invalid"
        );

        var response = await client.PostAsJsonAsync($"api/Timesheets/update/{timesheetId}", decision);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var msg = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid timesheet status value.", msg);
    }

    [Fact]
    public async Task Update_NotFound_Returns_BadRequest()
    {
        using var client = CreateClient();

        var decision = new TimesheetDecisionDto(
            TimesheetId: 999,
            LeaderId: 1,
            Status: (int)TimesheetStatus.Approved,
            Comment: "irrelevant"
        );

        var response = await client.PostAsJsonAsync("api/Timesheets/update/999", decision);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var msg = await response.Content.ReadAsStringAsync();
        Assert.Contains("Timesheet not found.", msg);
    }

    [Fact]
    public async Task SubmitTimesheet_Already_Exists_Throws()
    {
        using var client = CreateClient();
        
        var dto = new SubmitTimesheetDto(
            UserId: 1,
            PeriodStart: new DateOnly(2025, 10, 1),
            PeriodEnd: new DateOnly(2025, 10, 31)
        );

        var first = await client.PostAsJsonAsync("api/Timesheets/submit", dto);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync("api/Timesheets/submit", dto);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        var message = await second.Content.ReadAsStringAsync();
        Assert.Contains("Timesheet already exists for the specified user and period.", message);
    }

    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_Returns_Ok_With_Data()
    {
        using var client = CreateClient();

        var leaderId = 1;
        var start = new DateOnly(2025, 12, 1);
        var end = new DateOnly(2025, 12, 31);
        var startParam = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endParam = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // Act
        var response = await client.GetAsync(
            $"api/Timesheets/leader/{leaderId}/team/?startDate={startParam}&endDate={endParam}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LeaderUsersProjectsResponseDto>();
        Assert.NotNull(result);

        Assert.NotEmpty(result!.Users);
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
        using var client = CreateClient();

        var leaderId = 1;
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 31);

        var startParam = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endParam = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var response = await client.GetAsync(
            $"api/Timesheets/leader/{leaderId}/team/?startDate={startParam}&endDate={endParam}"
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}