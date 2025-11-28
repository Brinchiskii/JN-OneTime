using OneTime.Api.Models;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Api.Tests.Converters
{
    public class TimeEntryConverterTests
    {
        [Fact]
        public void ToEntity_Maps_All_Fields_Correctly()
        {
            // Arrange 
            var dto = new TimeEntryCreateDto(
                UserId: 42,
                ProjectId: 10,
                Date: DateOnly.FromDateTime(new DateTime(2025, 3, 5)),
                Note: "Test Note",
                Hours: 7.5m
            );

            // Act
            var entity = TimeEntryConverter.ToEntity( dto );

            // Assert
            Assert.NotNull( entity );
            Assert.Equal(dto.UserId, entity.UserId );
            Assert.Equal(dto.ProjectId, entity.ProjectId );
            Assert.Equal(dto.Date, entity.Date );
            Assert.Equal(dto.Note, entity.Note );
            Assert.Equal(dto.Hours, entity.Hours );
        }

        [Fact]
        public void ToDto_Maps_All_Fields_Correctly()
        {
            // Arrange
            var entity = new TimeEntry
            {
                TimeEntryId = 1,
                UserId = 42,
                ProjectId = 10,
                Date = DateOnly.FromDateTime(new DateTime(2025, 3, 5)),
                Note = "Test Note",
                Hours = 4m,
                Status = (int)TimeEntryStatus.Pending
            };

            // Act
            var dto = TimeEntryConverter.ToDto( entity );

            // Assert
            Assert.NotNull( dto );
            Assert.Equal(entity.TimeEntryId, dto.TimeEntryId );
            Assert.Equal(entity.UserId, dto.UserId );
            Assert.Equal(entity.ProjectId, dto.ProjectId );
            Assert.Equal(entity.Date, dto.Date );
            Assert.Equal(entity.Note, dto.Note );
            Assert.Equal(entity.Hours, dto.Hours );
            Assert.Equal((TimeEntryStatus)entity.Status, dto.Status );
        }
    }
}
