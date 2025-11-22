using OneTime.Api.Tests.TestHelpers;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;
using OneTime.Core.Models;

namespace OneTime.Api.Tests.Endpoints
{
    public class TimeEntriesProjectsTests : IClassFixture<OneTimeApiFactory>
    {
        private readonly HttpClient _client;

        public TimeEntriesProjectsTests(OneTimeApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAvailableProjects_Returns_Ok_With_Projects()
        {
            // Act
            var response = await _client.GetAsync("api/timeentries/projects");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
            Assert.NotNull(projects);
            Assert.True(projects.Count >= 2);
        }
    }
}
