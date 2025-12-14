using System.Net;
using System.Net.Http.Json;
using OneTime.Api.Models;
using OneTime.Api.Models.OneTime.Api.Models;
using OneTime.Api.Models.TimeEntriesDto;
using OneTime.Api.Tests.TestHelpers;
using Xunit;

namespace OneTime.Api.Tests.Endpoints;

public class TimeEntriesControllerTests
{
    private static HttpClient CreateClient()
    {
        var factory = new OneTimeApiFactory();
        return factory.CreateClient();
    }

    [Fact]
    public async Task CreateTimeEntry_Returns_Ok_With_Created_Entry()
    {
        using var client = CreateClient();
        
        var dto = new TimeEntryCreateDto(
            UserId: 2,
            ProjectId: 1,
            Date: new DateOnly(2025, 12, 7),
            Note: "Integration test entry",
            Hours: 4m,
            TimesheetId: 2
        );

        var response = await client.PostAsJsonAsync("api/TimeEntries", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TimeEntryDto>();
        Assert.NotNull(created);

        Assert.True(created!.TimeEntryId > 0);
        Assert.Equal(dto.UserId, created.UserId);
        Assert.Equal(dto.ProjectId, created.ProjectId);
        Assert.Equal(dto.Date, created.Date);
        Assert.Equal(dto.Note, created.Note);
        Assert.Equal(dto.Hours, created.Hours);
        Assert.Equal(dto.TimesheetId, created.TimesheetId);
    }

    [Fact]
    public async Task CreateTimeEntry_InvalidProject_Returns_BadRequest()
    {
        using var client = CreateClient();

        var dto = new TimeEntryCreateDto(
            UserId: 2,
            ProjectId: 9999, // does not exist
            Date: new DateOnly(2025, 12, 7),
            Note: "Should fail",
            Hours: 4m,
            TimesheetId: 2
        );

        var response = await client.PostAsJsonAsync("api/TimeEntries", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var msg = await response.Content.ReadAsStringAsync();
        Assert.Contains("Projekt not found", msg);
    }

    [Fact]
    public async Task CreateTimeEntry_InvalidHours_Returns_BadRequest()
    {
        using var client = CreateClient();

        var dto = new TimeEntryCreateDto(
            UserId: 2,
            ProjectId: 1,
            Date: new DateOnly(2025, 12, 7),
            Note: "Should fail",
            Hours: 0m, // invalid per controller
            TimesheetId: 2
        );

        var response = await client.PostAsJsonAsync("api/TimeEntries", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var msg = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hours must be greater than zero and less than 24", msg);
    }

    [Fact]
    public async Task GetTimeEntriesForUser_Returns_Ok_With_Data()
    {
        using var client = CreateClient();
        
        var response = await client.GetAsync("api/TimeEntries/user/2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entries = await response.Content.ReadFromJsonAsync<List<TimeEntryDetailsDto>>();
        Assert.NotNull(entries);
        Assert.NotEmpty(entries!);

        Assert.All(entries!, e => Assert.Equal(2, e.UserId));
    }

    [Fact]
    public async Task GetTimeEntriesForUser_NoEntries_Returns_NoContent()
    {
        using var client = CreateClient();

        // No seeded entries for this user id
        var response = await client.GetAsync("api/TimeEntries/user/9999");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}