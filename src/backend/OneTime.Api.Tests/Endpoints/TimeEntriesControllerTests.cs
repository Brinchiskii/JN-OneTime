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
    public async Task SaveTimeEntries_ValidInput_Returns200()
    {
        var client = CreateClient();
        
        var entries = new List<TimeEntryCreateDto>
        {
            new(
                UserId: 1,
                ProjectId: 1,
                Date: new DateOnly(2025, 1, 1),
                Note: "Test",
                Hours: 8,
                TimesheetId: 1)
        };

        var response = await client.PostAsJsonAsync("/api/timeentries/bulk/1", entries);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task SaveTimeEntries_InvalidTimesheetId_Returns400()
    {
        var client = CreateClient();
        
        var entries = new List<TimeEntryCreateDto>();

        var response = await client.PostAsJsonAsync("/api/timeentries/bulk/0", entries);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetTimeEntriesForUser_NoEntries_Returns204()
    {
        var client = CreateClient();
        
        var response = await client.GetAsync("/api/timeentries/user/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task GetTimeEntriesForUser_WithEntries_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        await client.PostAsJsonAsync("/api/users", new
        {
            Name = "Test User",
            Email = "test@test.com",
            Password = "password",
            Role = 0,
        });
        
        await client.PostAsJsonAsync("/api/projects", new
        {
            Name = "Test Project",
            Status = 0
        });
        
        var entries = new List<TimeEntryCreateDto>
        {
            new(
                UserId: 1,
                ProjectId: 1,
                Date: new DateOnly(2025, 1, 1),
                Note: "Worked",
                Hours: 7,
                TimesheetId: 1)
        };

        await client.PostAsJsonAsync("/api/timeentries/bulk/1", entries);

        // Act
        var response = await client.GetAsync("/api/timeentries/user/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<TimeEntryDetailsDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(7, result[0].Hours);
    }


    [Fact]
    public async Task GetTimeEntriesForTimesheet_Returns200()
    {
        var client = CreateClient();

        await client.PostAsJsonAsync("/api/users", new
        {
            Name = "Test User",
            Email = "test@test.com",
            Password = "password",
            Role = 0
        });

        await client.PostAsJsonAsync("/api/projects", new
        {
            Name = "Test Project",
            Status = 0
        });

        await client.PostAsJsonAsync("/api/timesheets/submit", new
        {
            UserId = 1,
            PeriodStart = new DateOnly(2025, 1, 1),
            PeriodEnd = new DateOnly(2025, 1, 31)
        });
        
        var entries = new List<TimeEntryCreateDto>
        {
            new(
                UserId: 1,
                ProjectId: 1,
                Date: new DateOnly(2025, 1, 2),
                Note: "More work",
                Hours: 6,
                TimesheetId: 1)
        };

        await client.PostAsJsonAsync("/api/timeentries/bulk/1", entries);

        // Act
        var response = await client.GetAsync("/api/timeentries/user/1/timesheet/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<TimeEntryDetailsDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(6, result[0].Hours);
    }

    
    [Fact]
    public async Task GetTimeEntriesForTimesheet_NoEntries_Returns200_WithEmptyList()
    {
        var client = CreateClient();
        
        var response = await client.GetAsync("/api/timeentries/user/1/timesheet/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<TimeEntryDetailsDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}