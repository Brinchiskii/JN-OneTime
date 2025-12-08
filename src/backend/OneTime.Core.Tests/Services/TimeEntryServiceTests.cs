using System;
using System.Linq;
using System.Threading.Tasks;
using OneTime.Core.Models;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Tests.Fakes;
using OneTime.Core.Tests.TestData;
using Xunit;

namespace OneTime.Core.Tests.Services
{
    public class TimeEntryServiceTests
    {
        private readonly FakeProjectRepository _fakeProjectRepo;
        private readonly FakeTimeEntryRepository _fakeTimeEntryRepo;
        private readonly TimeEntryService _service;

        public TimeEntryServiceTests()
        {
            _fakeProjectRepo = new FakeProjectRepository();
            _fakeTimeEntryRepo = new FakeTimeEntryRepository();
            _service = new TimeEntryService(_fakeTimeEntryRepo, _fakeProjectRepo);
        }

        [Fact]
        public async Task CreateTimeEntry_Throws_When_Project_Not_Found()
        {
            // Arrange: fake project repo indeholder ikke projektet
            var entry = MockData.CreateTimeEntry(projectId: 123);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateTimeEntry(entry));

            Assert.Equal("Project not found", ex.ParamName);
            Assert.False(_fakeTimeEntryRepo.AddCalled);
        }

        [Fact]
        public async Task CreateTimeEntry_Saves_When_Project_Exists()
        {
            // Arrange
            _fakeProjectRepo.Projects.Add(MockData.CreateProject(id: 10, name: "Projekt X"));
            var entry = MockData.CreateTimeEntry(projectId: 10, hours: 8m);

            // Act
            var result = await _service.CreateTimeEntry(entry);

            // Assert
            Assert.True(_fakeTimeEntryRepo.AddCalled);
            Assert.NotNull(_fakeTimeEntryRepo.AddedEntry);
            Assert.Equal(10, _fakeTimeEntryRepo.AddedEntry.ProjectId);
            Assert.Equal(8m, _fakeTimeEntryRepo.AddedEntry.Hours);
            Assert.Same(result, _fakeTimeEntryRepo.AddedEntry);
        }

        [Fact]
        public async Task CreateTimeEntry_Throws_When_Hours_Invalid()
        {
            // Arrange
            _fakeProjectRepo.Projects.Add(MockData.CreateProject(id: 10)); // projekt findes
            var entryZero = MockData.CreateTimeEntry(projectId: 10, hours: 0m);
            var entryTooMany = MockData.CreateTimeEntry(projectId: 10, hours: 25m);

            // Act & Assert
            var ex1 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.CreateTimeEntry(entryZero));
            Assert.Equal("Hours must be greater than zero and less than 24", ex1.ParamName);

            var ex2 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.CreateTimeEntry(entryTooMany));
            Assert.Equal("Hours must be greater than zero and less than 24", ex2.ParamName);

            Assert.False(_fakeTimeEntryRepo.AddCalled);
        }

        [Fact]
        public async Task CreateTimeEntry_Throws_On_Null_Input()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateTimeEntry(null!));
            Assert.False(_fakeTimeEntryRepo.AddCalled);
        }

        [Fact]
        public async Task CreateTimeEntry_Sets_Default_Date_When_Not_Set()
        {
            // Hvis du vælger at servicen skal sætte default date
            _fakeProjectRepo.Projects.Add(MockData.CreateProject(id: 10));

            var entry = MockData.CreateTimeEntry(projectId: 10, hours: 4m);

            var result = await _service.CreateTimeEntry(entry);

            Assert.NotEqual(default, result.Date);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today), result.Date);
            Assert.True(_fakeTimeEntryRepo.AddCalled);
        }

        [Fact]
        public async Task GetTimeEntryByUserWithDetails_UserId_NotOK()
        {
            // Arrange
            const int invalidUserIdZero = 0;
            const int invalidUserIdNegative = -1;

            // Act & Assert
            var ex1 = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTimeEntriesByUserWithDetails(invalidUserIdZero));
            Assert.Equal("UserId must be greater than zero", ex1.Message);
            var ex2 = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTimeEntriesByUserWithDetails(invalidUserIdNegative));
            Assert.Equal("UserId must be greater than zero", ex2.Message);

            Assert.Null(_fakeTimeEntryRepo.LastUserIdRequested);
        }

        [Fact]
        public async Task GetTimeEntryByUserWithDetails_UserId_OK()
        {
            // Arrange
            const int validUserId = 1;
            
            // Act + Assert
            await _service.GetTimeEntriesByUserWithDetails(validUserId);
            Assert.Equal(validUserId, _fakeTimeEntryRepo.LastUserIdRequested);
        }
        
        [Fact]
        public async Task GetTimeEntryByUserWithDetails_EmptyList()
        {
            // Arrange
            const int validUserId = 1;
            
            // Act + Assert
            var result = await _service.GetTimeEntriesByUserWithDetails(validUserId);
            Assert.Empty(result);
        }
        
        [Fact]
        public async Task GetTimeEntryByUserWithDetails_ListWithEntries()
        {
            // Arrange
            const int validUserId = 1;

            var date1 = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));
            var date2 = DateOnly.FromDateTime(DateTime.Today);
            var otherUserDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            // Seed entries for the valid user and another user to verify filtering and ordering
            _fakeTimeEntryRepo.SeedEntries(
                MockData.CreateTimeEntry(userId: validUserId, projectId: 10, hours: 4m, date: date2, note: "later"),
                MockData.CreateTimeEntry(userId: validUserId, projectId: 11, hours: 2m, date: date1, note: "earlier"),
                MockData.CreateTimeEntry(userId: 2, projectId: 12, hours: 3m, date: otherUserDate, note: "other user")
            );

            // Act
            var result = (await _service.GetTimeEntriesByUserWithDetails(validUserId)).ToList();

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, r => Assert.Equal(validUserId, r.UserId));
            Assert.Equal(2, result.Count);

            // Should be ordered by date ascending
            Assert.True(result[0].Date <= result[1].Date);
            Assert.Equal(new[] { date1, date2 }, result.Select(r => r.Date).ToArray());
            Assert.Equal(validUserId, _fakeTimeEntryRepo.LastUserIdRequested);
        }
    }
}
