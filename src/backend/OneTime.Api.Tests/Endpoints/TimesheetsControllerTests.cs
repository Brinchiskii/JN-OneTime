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
    
    [Fact]
    public async Task Submit_ValidInput_Returns200()
    {
        // Arrange
        var client = CreateClient();
        
        var dto = new SubmitTimesheetDto(
            UserId: 1,
            PeriodStart: new DateOnly(2025, 1, 1),
            PeriodEnd: new DateOnly(2025, 1, 31));

        // Act
        var response = await client.PostAsJsonAsync("/api/timesheets/submit", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TimesheetDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
    }
    
    [Fact]
    public async Task Submit_ServiceThrows_Returns400()
    {
        var client = CreateClient();
        
        var dto = new SubmitTimesheetDto(
            UserId: 1,
            PeriodStart: new DateOnly(2025, 2, 1),
            PeriodEnd: new DateOnly(2025, 1, 1));

        var response = await client.PostAsJsonAsync("/api/timesheets/submit", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_ValidInput_Returns200()
    {
        var client = CreateClient();
        
        await client.PostAsJsonAsync("/api/timesheets/submit",
            new SubmitTimesheetDto(
                1,
                new DateOnly(2025, 1, 1),
                new DateOnly(2025, 1, 31)));

        var dto = new TimesheetDecisionDto(
            TimesheetId: 1,
            LeaderId: 1,
            Status: (int)TimesheetStatus.Approved,
            Comment: "Looks good");

        var response = await client.PostAsJsonAsync("/api/timesheets/update/1", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TimesheetDto>();
        Assert.NotNull(result);
        Assert.Equal(TimesheetStatus.Approved, result.Status);
    }

    [Fact]
    public async Task Update_InvalidStatus_Returns400()
    {
        var client = CreateClient();
        
        var dto = new TimesheetDecisionDto(
            TimesheetId: 1,
            LeaderId: 1,
            Status: 99,
            Comment: "Invalid");

        var response = await client.PostAsJsonAsync("/api/timesheets/update/1", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_NoEntries_Returns204()
    {
        var client = CreateClient();
        
        var response = await client.GetAsync(
            "/api/timesheets/leader/1/team?startDate=2025-01-01&endDate=2025-01-31");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task GetTimesheetByUserAndDate_NotFound_Returns204()
    {
        var client = CreateClient();
        
        var response = await client.GetAsync(
            "/api/timesheets/user/1/time?startDate=2025-01-01&endDate=2025-01-31");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_Found_Returns200()
    {
        var client = CreateClient();
        
        await client.PostAsJsonAsync("/api/timesheets/submit",
            new SubmitTimesheetDto(
                1,
                new DateOnly(2025, 1, 1),
                new DateOnly(2025, 1, 31)));

        var response = await client.GetAsync(
            "/api/timesheets/user/1/time?startDate=2025-01-01&endDate=2025-01-31");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TimesheetDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
    }
    
}