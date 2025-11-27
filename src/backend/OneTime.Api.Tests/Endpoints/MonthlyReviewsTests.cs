using OneTime.Api.Models;
using OneTime.Api.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace OneTime.Api.Tests.Endpoints
{
    public class MonthlyReviewsTests : IClassFixture<OneTimeApiFactory>
    {
        private readonly HttpClient _client;

        public MonthlyReviewsTests(OneTimeApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SubmitMonthlyReview_Already_Exists_Throws()
        {
            // Arrange
            var dto = new SubmitMonthlyReviewDto(
                UserId: 1,
                PeriodStart: new DateOnly(2025, 11, 1),
                PeriodEnd: new DateOnly(2025, 11, 30)
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/MonthlyReviews/submit", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("Monthly review already exists for the specified user and period.", message);
        }

        [Fact]
        public async Task SubmitMonthlyReview_No_TimeEntries_Throws()
        {
            // Arrange
            var dto = new SubmitMonthlyReviewDto(
                UserId: 2,
                PeriodStart: new DateOnly(2025, 12, 1),
                PeriodEnd: new DateOnly(2025, 12, 31)
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/MonthlyReviews/submit", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("There are now registrered entries for this period.", message);
        }

        [Fact]
        public async Task SubmitMonthlyReview_Succeeds()
        {
            // Arrange
            var dto = new SubmitMonthlyReviewDto(
                UserId: 1,
                PeriodStart: new DateOnly(2025, 10, 1),
                PeriodEnd: new DateOnly(2025, 10, 30)
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/MonthlyReviews/submit", dto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<MonthlyReviewDto>();
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal(new DateOnly(2025, 10, 1), result.PeriodStart);
            Assert.Equal(new DateOnly(2025, 10, 30), result.PeriodEnd);
            Assert.Equal("Pending", result.Status);
            Assert.Null(result.DecidedAt);
            Assert.Null(result.Comment);
        }
    }
}
