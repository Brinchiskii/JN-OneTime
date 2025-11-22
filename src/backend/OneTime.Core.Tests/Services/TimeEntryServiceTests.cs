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

            Assert.Equal(TimeEntryStatus.Pending, result.Status);
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

            var list = new List<Project>(result);
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].ProjectId);
            Assert.Equal(2, list[1].ProjectId);
        }
    }
}
