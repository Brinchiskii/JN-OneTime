using System;
using System.Collections.Generic;
using System.Text;
using OneTime.Core.Services.Repository;
using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Tests.TestHelpers;
namespace OneTime.Core.Tests.Services
{
    public class TimesheetServiceTests
    {
        [Fact]
        public async Task SubmitMonthlyReview_Already_Exists_Throws()
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
                Status = TimesheetStatus.Pending
            });

            await context.SaveChangesAsync();

            var service = new TimesheetService(context);

            // Act + Assert 
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SubmitMonthlyReviewAsync(userId, periodStart, periodEnd));
        }

        [Fact]
        public async Task SubmitMonthlyReview_No_TimeEntries_Throws()
        {
            // Arrange
            var userId = 1;
            var periodStart = new DateOnly(2025, 12, 1);
            var periodEnd = new DateOnly(2025, 12, 31);

            var context = OneTimeContextFactory.CreateInMemoryContext();

            var service = new TimesheetService(context);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SubmitMonthlyReviewAsync(userId, periodStart, periodEnd));

        }

        [Fact]
        public async Task SubmitMonthlyReview_Succeeds()
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

            var service = new TimesheetService(context);

            // Act
            var review = await service.SubmitMonthlyReviewAsync(userId, periodStart, periodEnd);

            // Assert
            Assert.NotNull(review);
            Assert.Equal(userId, review.UserId);
            Assert.Equal(periodStart, review.PeriodStart);
            Assert.Equal(periodEnd, review.PeriodEnd);
            Assert.Equal(TimesheetStatus.Pending, review.Status);
        }
    }
}
