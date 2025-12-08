using System;
using System.Threading.Tasks;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Interfaces;
using OneTime.Core.Services.Repository;
using OneTime.Core.Tests.Fakes;
using OneTime.Core.Tests.TestData;
using Xunit;

namespace OneTime.Core.Tests.Services
{
    public class TimeEntryServiceTests
    {
        [Fact]
        public async Task CreateTimeEntry_Throws_When_Project_Not_Found()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();

            var fakeTimeEntryRepo = new FakeTimeEntryRepository();
            var timeEntryService = new TimeEntryService(fakeTimeEntryRepo, fakeProjectRepo);

            var entry = MockData.CreateTimeEntry(projectId: 123);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => timeEntryService.CreateTimeEntry(entry));

            // Assert 
            Assert.Equal("Projekt not found", ex.Message);
            Assert.False(fakeTimeEntryRepo.AddCalled);
            Assert.Null(fakeTimeEntryRepo.AddedEntry);
        }

        [Fact]
        public async Task CreateTimeEntry_Saves_When_Project_Exists()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();
            fakeProjectRepo.Projects.Add(MockData.CreateProject(id: 10, name: "Projekt X"));

            var fakeTimeEntryRepo = new FakeTimeEntryRepository();

            var timeEntryService = new TimeEntryService(fakeTimeEntryRepo, fakeProjectRepo);

            var entry = MockData.CreateTimeEntry(projectId: 10, hours: 8m);

            // Act
            var result = await timeEntryService.CreateTimeEntry(entry);

            // Assert
            Assert.True(fakeTimeEntryRepo.AddCalled);
            Assert.NotNull(fakeTimeEntryRepo.AddedEntry);

            Assert.Equal(10, fakeTimeEntryRepo.AddedEntry.ProjectId);
            Assert.Equal(8m, fakeTimeEntryRepo.AddedEntry.Hours);

            Assert.Equal(result, fakeTimeEntryRepo.AddedEntry);
        }

        [Fact]
        public async Task CreateTimeEntry_Sets_Status_Pending_And_Default_Date_When_Not_Set()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();
            fakeProjectRepo.Projects.Add(MockData.CreateProject(id: 10));

            var fakeTimeEntryRepo = new FakeTimeEntryRepository();
            var timeEntryService = new TimeEntryService(fakeTimeEntryRepo, fakeProjectRepo);

            var entry = new TimeEntry
            {
                ProjectId = 10,
                Hours = 4m,
            };

            // Act
            var result = await timeEntryService.CreateTimeEntry(entry);

            Assert.Equal((int)TimeEntryStatus.Pending, result.Status);
            Assert.NotEqual(default, result.Date);
        }

        [Fact]
        public async Task GetAvailableProjects_Returns_Projects_From_Repository()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();
            fakeProjectRepo.Projects.Add(MockData.CreateProject(1, "P1"));
            fakeProjectRepo.Projects.Add(MockData.CreateProject(2, "P2"));

            var fakeTimeEntryRepo = new FakeTimeEntryRepository();
            var timeEntryService = new TimeEntryService(fakeTimeEntryRepo, fakeProjectRepo);

            // Act
            var result = await timeEntryService.GetAvailableProjects();

            var list = new List<Project>((int)result);
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].ProjectId);
            Assert.Equal(2, list[1].ProjectId);
        }

        [Fact]
        public async Task CreateTimeEntry_Saves_Correct_Data_For_User()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();
            fakeProjectRepo.Projects.Add(new Project
            {
                ProjectId = 10,
                Name = "Testprojekt"
            });

            var fakeTimeRepo = new FakeTimeEntryRepository();
            var timeEntryService = new TimeEntryService(fakeTimeRepo, fakeProjectRepo);

            var entry = new TimeEntry
            {
                UserId = 5,
                ProjectId = 10,
                Hours = 6m,
                Date = new DateOnly(2024, 6, 15),
                Note = "Test Note"
            };

            // Act
            var result = await timeEntryService.CreateTimeEntry(entry);

            // Assert
            Assert.True(fakeTimeRepo.AddCalled);
            Assert.NotNull(fakeTimeRepo.AddedEntry);
            Assert.Equal(5, fakeTimeRepo.AddedEntry.UserId);
            Assert.Equal(10, fakeTimeRepo.AddedEntry.ProjectId);
            Assert.Equal(6m, fakeTimeRepo.AddedEntry.Hours);
            Assert.Equal(new DateOnly(2024, 6, 15), fakeTimeRepo.AddedEntry.Date);
            Assert.Equal("Test Note", fakeTimeRepo.AddedEntry.Note);
            Assert.Equal((int)TimeEntryStatus.Pending, fakeTimeRepo.AddedEntry.Status);
        }

        [Fact]
        public async Task CreateTimeEntry_Throws_When_Hours_Invalid()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();
            fakeProjectRepo.Projects.Add(new Project
            {
                ProjectId = 10,
                Name = "Testprojekt"
            });
            var fakeTimeRepo = new FakeTimeEntryRepository();
            var timeEntryService = new TimeEntryService(fakeTimeRepo, fakeProjectRepo);

            var entryWithZeroHours = new TimeEntry
            {
                UserId = 5,
                ProjectId = 10,
                Hours = 0m,
                Date = new DateOnly(2024, 6, 15),
                Note = "Test Note"
            };

            var entryWithTooManyHours = new TimeEntry
            {
                UserId = 5,
                ProjectId = 10,
                Hours = 25m,
                Date = new DateOnly(2024, 6, 15),
                Note = "Test Note"
            };

            // Act & Assert
            var ex1 = await Assert.ThrowsAsync<Exception>(() => timeEntryService.CreateTimeEntry(entryWithZeroHours));
            Assert.Equal("Hours must be greater than zero and less than 24", ex1.Message);
            var ex2 = await Assert.ThrowsAsync<Exception>(() => timeEntryService.CreateTimeEntry(entryWithTooManyHours));
            Assert.Equal("Hours must be greater than zero and less than 24", ex2.Message);
        }

        [Fact]
        public async Task GetTimeEntriesForUser_Returns_Entries_From_Repository()
        {
            // Arrange
            var fakeProjectRepo = new FakeProjectRepository();
            var fakeTimeEntryRepo = new FakeTimeEntryRepository();

            fakeTimeEntryRepo.EntriesToReturn.AddRange(new[] 
            {
                new TimeEntry { TimeEntryId = 1, UserId = 42, Hours = 4m},
                new TimeEntry { TimeEntryId = 2, UserId = 42, Hours = 3.5m},
            });

            var timeEntryService = new TimeEntryService(fakeTimeEntryRepo, fakeProjectRepo);

            // Act
            var result = await timeEntryService.GetTimeEntriesForUser(42);

            // Assert
            var list = result.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].TimeEntryId);
            Assert.Equal(2, list[1].TimeEntryId);

            Assert.Equal(42, fakeTimeEntryRepo.LastUserIdRequested);

        }
    }
}
