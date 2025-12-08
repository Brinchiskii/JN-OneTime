using Microsoft.AspNetCore.SignalR;
using OneTime.Api.Models;
using OneTime.Api.Models.TimeEntriesDto;
using OneTime.Api.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit.Sdk;

namespace OneTime.Api.Tests.Endpoints
{
    public class TimeEntriesCreateTests : IClassFixture<OneTimeApiFactory>
    {
        private readonly HttpClient _client;

        public TimeEntriesCreateTests(OneTimeApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateTimeEntry_Returns_Ok_When_Project_Exists()
        {
            // Arrange
            var dto = new TimeEntryCreateDto
            (
                UserId: 1,
                ProjectId: 1,
                Date: default,
                Note: "Integration test",
                Hours: 7.5m,
                TimesheetId: 1
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/timeentries", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<TimeEntryDto>();
            Assert.NotNull(created);
            Assert.Equal(dto.ProjectId, created.ProjectId);
            Assert.Equal(dto.Hours, created.Hours);
        }

        [Fact]
        public async Task CreateTimeEntry_Returns_BadRequest_When_Project_Does_Not_Exists()
        {
            var dto = new TimeEntryCreateDto(
                UserId: 1,
                ProjectId: 999,                              
                Date: DateOnly.FromDateTime(DateTime.Today),
                Note: "Invalid project",
                Hours: 5m,
                TimesheetId: 1
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/timeentries", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("Projekt not found", message);
        }
    }
}
